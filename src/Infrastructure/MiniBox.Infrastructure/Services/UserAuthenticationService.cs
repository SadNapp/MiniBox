using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using MiniBox.Application.Common.Interfaces;
using MiniBox.Infrastructure.Authentication;
using MiniBox.Infrastructure.Persistence;
using Npgsql;

namespace MiniBox.Infrastructure.Services;

public sealed class UserAuthenticationService(ApplicationDbContext db, PasswordHasher passwords, JwtTokenGenerator tokens) : IUserAuthenticationService
{
    public async Task<AuthenticationResult?> RegisterAsync(string firstName, string lastName, string email, string password, CancellationToken cancellationToken)
    {
        var normalizedEmail = NormalizeEmail(email);
        if (await db.Users.AnyAsync(user => user.Email == normalizedEmail, cancellationToken)) return null;
        var user = new MiniBox.Domain.Entities.Users.User
        {
            FirstName = firstName.Trim(), LastName = lastName.Trim(), Email = normalizedEmail, PasswordHash = passwords.Hash(password)
        };
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        db.Users.Add(user);
        try { await db.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            await transaction.RollbackAsync(cancellationToken);
            db.Entry(user).State = EntityState.Detached;
            return null;
        }
        var result = await IssueTokensAsync(user, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return result;
    }

    public async Task<AuthenticationResult?> LoginAsync(string email, string password, CancellationToken cancellationToken)
    {
        var normalizedEmail = NormalizeEmail(email);
        var user = await db.Users.SingleOrDefaultAsync(candidate => candidate.Email == normalizedEmail, cancellationToken);
        if (user is null || !passwords.Verify(password, user.PasswordHash)) return null;
        return await IssueTokensAsync(user, cancellationToken);
    }

    public async Task<AuthenticationResult?> RefreshAsync(string refreshToken, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken)) return null;
        var tokenHash = HashToken(refreshToken);
        var existing = await db.RefreshTokens.Include(token => token.User)
            .SingleOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);
        var now = DateTime.UtcNow;
        if (existing is null || existing.RevokedAtUtc is not null || existing.ExpiresAtUtc <= now) return null;

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var revoked = await db.RefreshTokens.Where(token => token.Id == existing.Id && token.RevokedAtUtc == null && token.ExpiresAtUtc > now)
            .ExecuteUpdateAsync(update => update.SetProperty(token => token.RevokedAtUtc, now), cancellationToken);
        if (revoked != 1) return null;
        var result = await IssueTokensAsync(existing.User, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return result;
    }

    private async Task<AuthenticationResult> IssueTokensAsync(MiniBox.Domain.Entities.Users.User user, CancellationToken cancellationToken)
    {
        var access = tokens.Generate(user.UserId, user.Email, user.Role.ToString());
        var refresh = tokens.GenerateRefreshToken();
        db.RefreshTokens.Add(new MiniBox.Domain.Entities.Users.RefreshToken
        {
            UserId = user.UserId,
            TokenHash = HashToken(refresh.Token),
            ExpiresAtUtc = refresh.ExpiresAtUtc
        });
        await db.SaveChangesAsync(cancellationToken);
        return new AuthenticationResult(user.UserId, user.Email, access.Token, refresh.Token, access.ExpiresAtUtc, refresh.ExpiresAtUtc, user.FirstName, user.LastName);
    }

    private static string NormalizeEmail(string email) => email.Trim().ToUpperInvariant();

    private static string HashToken(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
