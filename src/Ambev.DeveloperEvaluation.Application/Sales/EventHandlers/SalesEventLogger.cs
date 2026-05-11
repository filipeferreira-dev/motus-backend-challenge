using Ambev.DeveloperEvaluation.Domain.Events;
using Rebus.Handlers;
using Serilog;

namespace Ambev.DeveloperEvaluation.Application.Sales.EventHandlers;

public class SalesEventLogger :
    IHandleMessages<SaleCreated>,
    IHandleMessages<SaleModified>,
    IHandleMessages<SaleCancelled>,
    IHandleMessages<ItemCancelled>
{
    public Task Handle(SaleCreated message)
    {
        Log.Information(
            "[SaleCreated] SaleId={SaleId} SaleNumber={SaleNumber} TotalAmount={TotalAmount} ItemCount={ItemCount} OccurredAt={OccurredAt}",
            message.SaleId, message.SaleNumber, message.TotalAmount, message.ItemCount, message.OccurredAt);
        return Task.CompletedTask;
    }

    public Task Handle(SaleModified message)
    {
        Log.Information(
            "[SaleModified] SaleId={SaleId} SaleNumber={SaleNumber} TotalAmount={TotalAmount} OccurredAt={OccurredAt}",
            message.SaleId, message.SaleNumber, message.TotalAmount, message.OccurredAt);
        return Task.CompletedTask;
    }

    public Task Handle(SaleCancelled message)
    {
        Log.Information(
            "[SaleCancelled] SaleId={SaleId} SaleNumber={SaleNumber} OccurredAt={OccurredAt}",
            message.SaleId, message.SaleNumber, message.OccurredAt);
        return Task.CompletedTask;
    }

    public Task Handle(ItemCancelled message)
    {
        Log.Information(
            "[ItemCancelled] SaleId={SaleId} SaleNumber={SaleNumber} SaleItemId={SaleItemId} ProductId={ProductId} OccurredAt={OccurredAt}",
            message.SaleId, message.SaleNumber, message.SaleItemId, message.ProductId, message.OccurredAt);
        return Task.CompletedTask;
    }
}
