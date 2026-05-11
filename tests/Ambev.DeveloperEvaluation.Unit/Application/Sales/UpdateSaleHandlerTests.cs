using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Rebus.Bus;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales;

public class UpdateSaleHandlerTests
{
    private readonly ISaleRepository _repo = Substitute.For<ISaleRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private readonly IBus _bus = Substitute.For<IBus>();
    private readonly UpdateSaleHandler _handler;

    public UpdateSaleHandlerTests()
    {
        _handler = new UpdateSaleHandler(_repo, _mapper, _bus);
    }

    [Fact(DisplayName = "Update replaces items, recalculates total, publishes SaleModified")]
    public async Task Handle_Valid_UpdatesAndPublishes()
    {
        var sale = SaleTestData.GenerateValidSale(itemCount: 1, quantityPerItem: 1);
        _repo.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);
        _repo.UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>()).Returns(ci => ci.Arg<Sale>());
        _mapper.Map<GetSaleResult>(Arg.Any<Sale>()).Returns(new GetSaleResult { Id = sale.Id });

        var command = new UpdateSaleCommand
        {
            Id = sale.Id,
            SaleDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = "C",
            BranchId = Guid.NewGuid(),
            BranchName = "B",
            Items = new List<SaleItemInput>
            {
                new() { ProductId = Guid.NewGuid(), ProductName = "P", Quantity = 5, UnitPrice = 10m }
            }
        };

        await _handler.Handle(command, CancellationToken.None);

        sale.Items.Should().ContainSingle();
        sale.TotalAmount.Should().Be(45m); // 5 * 10 - 10% = 45
        await _bus.Received(1).Publish(Arg.Any<SaleModified>(), Arg.Any<Dictionary<string, string>>());
    }

    [Fact(DisplayName = "Missing sale throws KeyNotFoundException")]
    public async Task Handle_Missing_Throws()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Sale?)null);

        var command = new UpdateSaleCommand
        {
            Id = Guid.NewGuid(),
            SaleDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = "C",
            BranchId = Guid.NewGuid(),
            BranchName = "B",
            Items = new List<SaleItemInput> { new() { ProductId = Guid.NewGuid(), ProductName = "P", Quantity = 1, UnitPrice = 1m } }
        };

        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
