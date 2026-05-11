using Ambev.DeveloperEvaluation.Application.Auth.AuthenticateUser;
using Ambev.DeveloperEvaluation.Common.Security;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Auth;

public class AuthenticateUserHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IJwtTokenGenerator _jwtTokenGenerator = Substitute.For<IJwtTokenGenerator>();
    private readonly AuthenticateUserHandler _handler;

    public AuthenticateUserHandlerTests()
    {
        _handler = new AuthenticateUserHandler(_userRepository, _passwordHasher, _jwtTokenGenerator);
    }

    private AuthenticateUserCommand ValidCommand(string email = "user@example.com", string password = "Pwd@1234")
        => new() { Email = email, Password = password };

    [Fact(DisplayName = "Valid credentials + active user returns result with token")]
    public async Task Handle_ValidActiveUser_ReturnsResultWithToken()
    {
        var user = UserTestData.GenerateValidUser();
        user.Status = UserStatus.Active;

        var command = ValidCommand(user.Email, "Pwd@1234");
        _userRepository.GetByEmailAsync(user.Email, Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.VerifyPassword("Pwd@1234", user.Password).Returns(true);
        _jwtTokenGenerator.GenerateToken(user).Returns("signed-jwt");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Token.Should().Be("signed-jwt");
        result.Email.Should().Be(user.Email);
        result.Name.Should().Be(user.Username);
        result.Role.Should().Be(user.Role.ToString());
    }

    [Fact(DisplayName = "Unknown email throws UnauthorizedAccessException")]
    public async Task Handle_UserNotFound_ThrowsUnauthorized()
    {
        _userRepository.GetByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((User?)null);

        var act = () => _handler.Handle(ValidCommand(), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("Invalid credentials");
        _jwtTokenGenerator.DidNotReceiveWithAnyArgs().GenerateToken(default!);
    }

    [Fact(DisplayName = "Wrong password throws UnauthorizedAccessException")]
    public async Task Handle_WrongPassword_ThrowsUnauthorized()
    {
        var user = UserTestData.GenerateValidUser();
        user.Status = UserStatus.Active;
        _userRepository.GetByEmailAsync(user.Email, Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.VerifyPassword(Arg.Any<string>(), user.Password).Returns(false);

        var act = () => _handler.Handle(ValidCommand(user.Email), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("Invalid credentials");
        _jwtTokenGenerator.DidNotReceiveWithAnyArgs().GenerateToken(default!);
    }

    [Fact(DisplayName = "Inactive user with valid password throws UnauthorizedAccessException")]
    public async Task Handle_InactiveUser_ThrowsUnauthorized()
    {
        var user = UserTestData.GenerateValidUser();
        user.Status = UserStatus.Suspended;
        _userRepository.GetByEmailAsync(user.Email, Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.VerifyPassword(Arg.Any<string>(), user.Password).Returns(true);

        var act = () => _handler.Handle(ValidCommand(user.Email), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("User is not active");
        _jwtTokenGenerator.DidNotReceiveWithAnyArgs().GenerateToken(default!);
    }
}
