using Ambev.DeveloperEvaluation.Common.Logging;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Common.Logging;

public class LoggingExtensionTests
{
    [Fact(DisplayName = "AddDefaultLogging configures Serilog without throwing and returns the builder")]
    public void AddDefaultLogging_ReturnsBuilder()
    {
        var builder = WebApplication.CreateBuilder();

        var returned = builder.AddDefaultLogging();

        returned.Should().BeSameAs(builder);
        using var sp = builder.Services.BuildServiceProvider();
        sp.GetService<ILoggerFactory>().Should().NotBeNull();
    }

    [Fact(DisplayName = "UseDefaultLogging executes against a built app without throwing")]
    public async Task UseDefaultLogging_DoesNotThrow()
    {
        var builder = WebApplication.CreateBuilder();
        builder.AddDefaultLogging();
        await using var app = builder.Build();

        var act = () => app.UseDefaultLogging();

        act.Should().NotThrow();
    }
}
