using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniBox.Domain.Entities.Users;

namespace MiniBox.Infrastructure.Persistence.Configurations;

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> entity)
    {
        entity.HasKey(token => token.Id);
        entity.Property(token => token.TokenHash).HasMaxLength(64).IsRequired();
        entity.HasIndex(token => token.TokenHash).IsUnique();
        entity.HasIndex(token => new { token.UserId, token.ExpiresAtUtc });
        entity.HasOne(token => token.User).WithMany(user => user.RefreshTokens).HasForeignKey(token => token.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}
