using Ambev.DeveloperEvaluation.Application.Sales.EventHandlers;
using Ambev.DeveloperEvaluation.Domain.Events;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Rebus.Config;
using Rebus.Routing.TypeBased;
using Rebus.Serilog;
using Rebus.Transport.InMem;

namespace Ambev.DeveloperEvaluation.IoC.ModuleInitializers;

public class MessagingModuleInitializer : IModuleInitializer
{
    private const string Queue = "sales.local";

    public void Initialize(WebApplicationBuilder builder)
    {
        builder.Services.AddRebus(configure => configure
            .Logging(l => l.Serilog())
            .Transport(t => t.UseInMemoryTransport(new InMemNetwork(true), Queue))
            .Routing(r => r.TypeBased()
                .Map<SaleCreated>(Queue)
                .Map<SaleModified>(Queue)
                .Map<SaleCancelled>(Queue)
                .Map<ItemCancelled>(Queue)));

        builder.Services.AutoRegisterHandlersFromAssemblyOf<SalesEventLogger>();
    }
}
