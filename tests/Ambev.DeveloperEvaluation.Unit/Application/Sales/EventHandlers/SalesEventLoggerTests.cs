using Ambev.DeveloperEvaluation.Application.Sales.EventHandlers;
using Ambev.DeveloperEvaluation.Domain.Events;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales.EventHandlers;

public class SalesEventLoggerTests
{
    private readonly SalesEventLogger _logger = new();

    [Fact(DisplayName = "Handles SaleCreated without throwing")]
    public async Task Handle_SaleCreated_DoesNotThrow()
    {
        var evt = new SaleCreated
        {
            SaleId = Guid.NewGuid(),
            SaleNumber = "S-1",
            OccurredAt = DateTime.UtcNow,
            TotalAmount = 100m,
            ItemCount = 2
        };

        var act = () => _logger.Handle(evt);

        await act.Should().NotThrowAsync();
    }

    [Fact(DisplayName = "Handles SaleModified without throwing")]
    public async Task Handle_SaleModified_DoesNotThrow()
    {
        var evt = new SaleModified
        {
            SaleId = Guid.NewGuid(),
            SaleNumber = "S-1",
            OccurredAt = DateTime.UtcNow,
            TotalAmount = 150m
        };

        var act = () => _logger.Handle(evt);

        await act.Should().NotThrowAsync();
    }

    [Fact(DisplayName = "Handles SaleCancelled without throwing")]
    public async Task Handle_SaleCancelled_DoesNotThrow()
    {
        var evt = new SaleCancelled
        {
            SaleId = Guid.NewGuid(),
            SaleNumber = "S-1",
            OccurredAt = DateTime.UtcNow
        };

        var act = () => _logger.Handle(evt);

        await act.Should().NotThrowAsync();
    }

    [Fact(DisplayName = "Handles ItemCancelled without throwing")]
    public async Task Handle_ItemCancelled_DoesNotThrow()
    {
        var evt = new ItemCancelled
        {
            SaleId = Guid.NewGuid(),
            SaleNumber = "S-1",
            SaleItemId = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            OccurredAt = DateTime.UtcNow
        };

        var act = () => _logger.Handle(evt);

        await act.Should().NotThrowAsync();
    }
}
