using Ambev.DeveloperEvaluation.Application.Sales.ListSales;
using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales;

public class ListSalesHandlerTests
{
    private readonly ISaleRepository _repo = Substitute.For<ISaleRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private readonly ListSalesHandler _handler;

    public ListSalesHandlerTests()
    {
        _handler = new ListSalesHandler(_repo, _mapper);
    }

    [Fact(DisplayName = "Handler propagates filter values into the repository call")]
    public async Task Handle_PassesFiltersToRepository()
    {
        var customerId = Guid.NewGuid();
        var paged = new PagedResult<Sale>(Array.Empty<Sale>(), 0, 1, 10);
        _repo.ListAsync(Arg.Any<SaleListFilters>(), Arg.Any<CancellationToken>()).Returns(paged);

        var command = new ListSalesCommand
        {
            Page = 2,
            Size = 5,
            Order = "saleDate desc",
            CustomerId = customerId,
            MinTotalAmount = 100m
        };

        await _handler.Handle(command, CancellationToken.None);

        await _repo.Received(1).ListAsync(
            Arg.Is<SaleListFilters>(f =>
                f.Page == 2 && f.Size == 5 && f.Order == "saleDate desc" &&
                f.CustomerId == customerId && f.MinTotalAmount == 100m),
            Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Handler maps sales to summary DTOs")]
    public async Task Handle_MapsResults()
    {
        var sales = new[] { SaleTestData.GenerateValidSale(), SaleTestData.GenerateValidSale() };
        var paged = new PagedResult<Sale>(sales, sales.Length, 1, 10);
        _repo.ListAsync(Arg.Any<SaleListFilters>(), Arg.Any<CancellationToken>()).Returns(paged);
        _mapper.Map<SaleSummaryDto>(Arg.Any<Sale>()).Returns(new SaleSummaryDto());

        var result = await _handler.Handle(new ListSalesCommand(), CancellationToken.None);

        result.TotalCount.Should().Be(2);
        result.Items.Should().HaveCount(2);
    }

    [Fact(DisplayName = "Page < 1 fails validation")]
    public async Task Handle_InvalidPage_Throws()
    {
        var act = () => _handler.Handle(new ListSalesCommand { Page = 0 }, CancellationToken.None);
        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
    }
}
