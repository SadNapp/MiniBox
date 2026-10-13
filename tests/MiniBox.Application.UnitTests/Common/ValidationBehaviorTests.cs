namespace MiniBox.Application.UnitTests.Common;

public class ValidationBehaviorTests
{
    [Test]
    public void RegisterUserValidator_RejectsWeakPasswordAndInvalidEmail()
    {
        var validator = new MiniBox.Application.Features.Users.Commands.RegisterUserValidator();
        var result = validator.Validate(new MiniBox.Application.Features.Users.Commands.RegisterUserCommand(
            "", "Lovelace", "not-an-email", "123"));

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Select(error => error.PropertyName),
            Does.Contain(nameof(MiniBox.Application.Features.Users.Commands.RegisterUserCommand.Email))
                .And.Contain(nameof(MiniBox.Application.Features.Users.Commands.RegisterUserCommand.Password))
                .And.Contain(nameof(MiniBox.Application.Features.Users.Commands.RegisterUserCommand.FirstName)));
    }

    [Test]
    public void RegisterUserValidator_AcceptsValidRegistration()
    {
        var validator = new MiniBox.Application.Features.Users.Commands.RegisterUserValidator();
        var result = validator.Validate(new MiniBox.Application.Features.Users.Commands.RegisterUserCommand(
            "Ada", "Lovelace", "ada@example.test", "strong-password"));

        Assert.That(result.IsValid, Is.True);
    }
}
