using Ambev.DeveloperEvaluation.Application.Sales.CancelSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using FluentAssertions;
using NSubstitute;
using Rebus.Bus;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales;

public class CancelSaleHandlerTests
{
    private readonly ISaleRepository _repo = Substitute.For<ISaleRepository>();
    private readonly IBus _bus = Substitute.For<IBus>();
    private readonly CancelSaleHandler _handler;

    public CancelSaleHandlerTests()
    {
        _handler = new CancelSaleHandler(_repo, _bus);
    }

    [Fact(DisplayName = "Cancel marks sale cancelled, persists, and publishes")]
    public async Task Handle_Valid_Cancels()
    {
        var sale = SaleTestData.GenerateValidSale();
        _repo.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);
        _repo.UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>()).Returns(ci => ci.Arg<Sale>());

        var response = await _handler.Handle(new CancelSaleCommand(sale.Id), CancellationToken.None);

        response.Success.Should().BeTrue();
        sale.Status.Should().Be(SaleStatus.Cancelled);
        await _repo.Received(1).UpdateAsync(sale, Arg.Any<CancellationToken>());
        await _bus.Received(1).Publish(Arg.Any<SaleCancelled>(), Arg.Any<Dictionary<string, string>>());
    }

    [Fact(DisplayName = "Missing sale throws KeyNotFoundException")]
    public async Task Handle_Missing_Throws()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Sale?)null);

        var act = () => _handler.Handle(new CancelSaleCommand(Guid.NewGuid()), CancellationToken.None);
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
