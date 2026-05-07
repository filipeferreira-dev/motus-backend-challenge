using Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using FluentAssertions;
using NSubstitute;
using Rebus.Bus;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales;

public class CancelSaleItemHandlerTests
{
    private readonly ISaleRepository _repo = Substitute.For<ISaleRepository>();
    private readonly IBus _bus = Substitute.For<IBus>();
    private readonly CancelSaleItemHandler _handler;

    public CancelSaleItemHandlerTests()
    {
        _handler = new CancelSaleItemHandler(_repo, _bus);
    }

    [Fact(DisplayName = "Existing item is cancelled, total drops, ItemCancelled is published")]
    public async Task Handle_Valid_Cancels()
    {
        var sale = SaleTestData.GenerateValidSale(itemCount: 2, quantityPerItem: 2);
        var initialTotal = sale.TotalAmount;
        var item = sale.Items.First();
        _repo.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);

        var response = await _handler.Handle(new CancelSaleItemCommand(sale.Id, item.Id), CancellationToken.None);

        response.Success.Should().BeTrue();
        item.IsCancelled.Should().BeTrue();
        sale.TotalAmount.Should().BeLessThan(initialTotal);
        await _bus.Received(1).Publish(Arg.Any<ItemCancelled>(), Arg.Any<Dictionary<string, string>>());
    }

    [Fact(DisplayName = "Missing sale throws KeyNotFoundException")]
    public async Task Handle_MissingSale_Throws()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Sale?)null);

        var act = () => _handler.Handle(new CancelSaleItemCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact(DisplayName = "Missing item on existing sale throws KeyNotFoundException")]
    public async Task Handle_MissingItem_Throws()
    {
        var sale = SaleTestData.GenerateValidSale();
        _repo.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);

        var act = () => _handler.Handle(new CancelSaleItemCommand(sale.Id, Guid.NewGuid()), CancellationToken.None);
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
