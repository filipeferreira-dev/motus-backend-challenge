using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Ambev.DeveloperEvaluation.Common.Security;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Common.Security;

public class JwtTokenGeneratorTests
{
    private const string SecretKey = "test-secret-key-must-be-at-least-32-bytes-long-1234567890";

    private static IConfiguration BuildConfig(string? secret = SecretKey)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Jwt:SecretKey"] = secret })
            .Build();
    }

    private static IUser BuildUser(string id = "user-1", string username = "demo-user", string role = "Customer")
    {
        var user = Substitute.For<IUser>();
        user.Id.Returns(id);
        user.Username.Returns(username);
        user.Role.Returns(role);
        return user;
    }

    [Fact(DisplayName = "GenerateToken returns a parseable JWT")]
    public void GenerateToken_ReturnsParseableToken()
    {
        var generator = new JwtTokenGenerator(BuildConfig());

        var token = generator.GenerateToken(BuildUser());

        token.Should().NotBeNullOrWhiteSpace();
        var parsed = new JwtSecurityTokenHandler().ReadJwtToken(token);
        parsed.SignatureAlgorithm.Should().Be("HS256");
    }

    [Fact(DisplayName = "GenerateToken embeds id, username, and role claims")]
    public void GenerateToken_EmbedsExpectedClaims()
    {
        var generator = new JwtTokenGenerator(BuildConfig());

        var token = generator.GenerateToken(BuildUser(id: "abc-123", username: "alice", role: "Admin"));

        var parsed = new JwtSecurityTokenHandler().ReadJwtToken(token);
        // JwtSecurityTokenHandler abbreviates the well-known ClaimTypes URIs to their short JWT names on parse.
        parsed.Claims.Should().Contain(c => c.Type == "nameid" && c.Value == "abc-123");
        parsed.Claims.Should().Contain(c => c.Type == "unique_name" && c.Value == "alice");
        parsed.Claims.Should().Contain(c => c.Type == "role" && c.Value == "Admin");
    }

    [Fact(DisplayName = "GenerateToken sets ~8h expiry")]
    public void GenerateToken_SetsExpiryAround8Hours()
    {
        var generator = new JwtTokenGenerator(BuildConfig());

        var token = generator.GenerateToken(BuildUser());

        var parsed = new JwtSecurityTokenHandler().ReadJwtToken(token);
        var ttl = parsed.ValidTo - DateTime.UtcNow;
        ttl.Should().BeGreaterThan(TimeSpan.FromHours(7.9));
        ttl.Should().BeLessThan(TimeSpan.FromHours(8.1));
    }
}
