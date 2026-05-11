using Ambev.DeveloperEvaluation.Common.HealthChecks;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Common.HealthChecks;

public class HealthChecksExtensionTests
{
    [Fact(DisplayName = "AddBasicHealthChecks registers liveness and readiness checks")]
    public async Task AddBasicHealthChecks_RegistersChecks()
    {
        var builder = WebApplication.CreateBuilder();

        builder.AddBasicHealthChecks();
        await using var app = builder.Build();

        var service = app.Services.GetRequiredService<HealthCheckService>();
        var report = await service.CheckHealthAsync();

        report.Status.Should().Be(HealthStatus.Healthy);
        report.Entries.Should().ContainKey("Liveness");
        report.Entries.Should().ContainKey("Readiness");
    }

    [Fact(DisplayName = "UseBasicHealthChecks wires endpoints without throwing")]
    public async Task UseBasicHealthChecks_WiresEndpoints()
    {
        var builder = WebApplication.CreateBuilder();
        builder.AddBasicHealthChecks();
        await using var app = builder.Build();

        var act = () => app.UseBasicHealthChecks();

        act.Should().NotThrow();
    }
}
