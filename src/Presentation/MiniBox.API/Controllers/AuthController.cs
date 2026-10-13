using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using MiniBox.Application.Features.Users.Commands;

namespace MiniBox.API.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IMediator mediator) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        if (request is null) return BadRequest(new { message = "Заповніть дані реєстрації." });
        var result = await mediator.Send(new RegisterUserCommand(request.FirstName, request.LastName, request.Email, request.Password), cancellationToken);
        if (result is null)
            return Conflict(new { message = "Користувач із таким email вже існує." });
        return Created("/api/auth/me", new { userId = result.UserId, result.Email, result.FirstName, result.LastName, result.AccessToken, result.RefreshToken, result.ExpiresAtUtc, result.RefreshExpiresAtUtc });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        if (request is null) return BadRequest(new { message = "Вкажіть email і пароль." });
        var result = await mediator.Send(new LoginUserCommand(request.Email, request.Password), cancellationToken);
        if (result is null)
            return Unauthorized(new { message = "Неправильний email або пароль." });
        return Ok(new { userId = result.UserId, result.Email, result.FirstName, result.LastName, result.AccessToken, result.RefreshToken, result.ExpiresAtUtc, result.RefreshExpiresAtUtc });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshRequest request, CancellationToken cancellationToken)
    {
        if (request is null) return BadRequest(new { message = "Відсутній refresh token." });
        var result = await mediator.Send(new RefreshUserCommand(request.RefreshToken), cancellationToken);
        if (result is null) return Unauthorized(new { message = "Refresh token недійсний або вже використаний." });
        return Ok(new { userId = result.UserId, result.Email, result.FirstName, result.LastName, result.AccessToken, result.RefreshToken, result.ExpiresAtUtc, result.RefreshExpiresAtUtc });
    }
}

public sealed record RegisterRequest(string FirstName, string LastName, string Email, string Password);
public sealed record LoginRequest(string Email, string Password);
public sealed record RefreshRequest(string RefreshToken);
