using Ambev.DeveloperEvaluation.Common.Security;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Common.Security;

public class BCryptPasswordHasherTests
{
    private readonly BCryptPasswordHasher _hasher = new();

    [Fact(DisplayName = "HashPassword produces non-empty hash distinct from input")]
    public void HashPassword_ProducesUsableHash()
    {
        var hash = _hasher.HashPassword("Pwd@1234");

        hash.Should().NotBeNullOrWhiteSpace();
        hash.Should().NotBe("Pwd@1234");
    }

    [Fact(DisplayName = "VerifyPassword returns true for the original password")]
    public void VerifyPassword_RoundTrips()
    {
        var hash = _hasher.HashPassword("Pwd@1234");

        _hasher.VerifyPassword("Pwd@1234", hash).Should().BeTrue();
    }

    [Fact(DisplayName = "VerifyPassword returns false for a different password")]
    public void VerifyPassword_RejectsWrongPassword()
    {
        var hash = _hasher.HashPassword("Pwd@1234");

        _hasher.VerifyPassword("WrongPassword", hash).Should().BeFalse();
    }
}
