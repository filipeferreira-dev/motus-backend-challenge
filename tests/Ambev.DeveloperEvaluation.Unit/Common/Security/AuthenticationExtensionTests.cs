using Ambev.DeveloperEvaluation.Common.Security;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Common.Security;

public class AuthenticationExtensionTests
{
    private static IConfiguration BuildConfig(string? secret) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Jwt:SecretKey"] = secret })
            .Build();

    [Fact(DisplayName = "AddJwtAuthentication registers IJwtTokenGenerator")]
    public void AddJwtAuthentication_RegistersTokenGenerator()
    {
        var services = new ServiceCollection();
        var config = BuildConfig("any-secret-long-enough-to-pass");
        services.AddSingleton<IConfiguration>(config);

        services.AddJwtAuthentication(config);
        var provider = services.BuildServiceProvider();

        provider.GetService<IJwtTokenGenerator>().Should().NotBeNull();
    }

    [Theory(DisplayName = "AddJwtAuthentication throws when Jwt:SecretKey is missing or whitespace")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AddJwtAuthentication_Throws_WhenSecretMissing(string? secret)
    {
        var services = new ServiceCollection();
        var config = BuildConfig(secret);

        var act = () => services.AddJwtAuthentication(config);

        act.Should().Throw<ArgumentException>();
    }
}
