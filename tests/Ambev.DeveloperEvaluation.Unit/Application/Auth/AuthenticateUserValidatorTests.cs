using Ambev.DeveloperEvaluation.Application.Auth.AuthenticateUser;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Auth;

public class AuthenticateUserValidatorTests
{
    private readonly AuthenticateUserValidator _validator = new();

    [Fact(DisplayName = "Valid email and 6+ char password passes")]
    public void Valid_Passes()
    {
        var result = _validator.Validate(new AuthenticateUserCommand { Email = "user@example.com", Password = "secret1" });

        result.IsValid.Should().BeTrue();
    }

    [Theory(DisplayName = "Empty or malformed email fails")]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void InvalidEmail_Fails(string email)
    {
        var result = _validator.Validate(new AuthenticateUserCommand { Email = email, Password = "secret1" });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AuthenticateUserCommand.Email));
    }

    [Theory(DisplayName = "Empty or short password fails")]
    [InlineData("")]
    [InlineData("12345")]
    public void InvalidPassword_Fails(string password)
    {
        var result = _validator.Validate(new AuthenticateUserCommand { Email = "user@example.com", Password = password });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AuthenticateUserCommand.Password));
    }
}
