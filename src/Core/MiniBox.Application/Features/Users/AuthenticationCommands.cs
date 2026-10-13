using FluentValidation;
using MediatR;
using MiniBox.Application.Common.Interfaces;

namespace MiniBox.Application.Features.Users.Commands;

public sealed record RegisterUserCommand(string FirstName, string LastName, string Email, string Password) : IRequest<AuthenticationResult?>;
public sealed record LoginUserCommand(string Email, string Password) : IRequest<AuthenticationResult?>;
public sealed record RefreshUserCommand(string RefreshToken) : IRequest<AuthenticationResult?>;

public sealed class RegisterUserHandler(IUserAuthenticationService users) : IRequestHandler<RegisterUserCommand, AuthenticationResult?>
{
    public Task<AuthenticationResult?> Handle(RegisterUserCommand request, CancellationToken cancellationToken) =>
        users.RegisterAsync(request.FirstName, request.LastName, request.Email, request.Password, cancellationToken);
}

public sealed class LoginUserHandler(IUserAuthenticationService users) : IRequestHandler<LoginUserCommand, AuthenticationResult?>
{
    public Task<AuthenticationResult?> Handle(LoginUserCommand request, CancellationToken cancellationToken) =>
        users.LoginAsync(request.Email, request.Password, cancellationToken);
}

public sealed class RefreshUserHandler(IUserAuthenticationService users) : IRequestHandler<RefreshUserCommand, AuthenticationResult?>
{
    public Task<AuthenticationResult?> Handle(RefreshUserCommand request, CancellationToken cancellationToken) =>
        users.RefreshAsync(request.RefreshToken, cancellationToken);
}

public sealed class RegisterUserValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserValidator()
    {
        RuleFor(command => command.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(command => command.LastName).NotEmpty().MaximumLength(100);
        RuleFor(command => command.Email).NotEmpty().EmailAddress().MaximumLength(320);
        RuleFor(command => command.Password).NotEmpty().MinimumLength(8).MaximumLength(128);
    }
}

public sealed class LoginUserValidator : AbstractValidator<LoginUserCommand>
{
    public LoginUserValidator()
    {
        RuleFor(command => command.Email).NotEmpty().EmailAddress().MaximumLength(320);
        RuleFor(command => command.Password).NotEmpty();
    }
}

public sealed class RefreshUserValidator : AbstractValidator<RefreshUserCommand>
{
    public RefreshUserValidator() => RuleFor(command => command.RefreshToken).NotEmpty().MinimumLength(40).MaximumLength(128);
}
