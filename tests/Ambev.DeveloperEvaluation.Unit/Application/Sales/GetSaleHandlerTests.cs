using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales;

public class GetSaleHandlerTests
{
    private readonly ISaleRepository _repo = Substitute.For<ISaleRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private readonly GetSaleHandler _handler;

    public GetSaleHandlerTests()
    {
        _handler = new GetSaleHandler(_repo, _mapper);
    }

    [Fact(DisplayName = "Existing sale returns mapped result")]
    public async Task Handle_Existing_ReturnsResult()
    {
        var sale = SaleTestData.GenerateValidSale();
        _repo.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);
        _mapper.Map<GetSaleResult>(sale).Returns(new GetSaleResult { Id = sale.Id });

        var result = await _handler.Handle(new GetSaleCommand(sale.Id), CancellationToken.None);

        result.Id.Should().Be(sale.Id);
    }

    [Fact(DisplayName = "Missing sale throws KeyNotFoundException")]
    public async Task Handle_Missing_Throws()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Sale?)null);

        var act = () => _handler.Handle(new GetSaleCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact(DisplayName = "Empty Guid fails validation")]
    public async Task Handle_EmptyId_Throws()
    {
        var act = () => _handler.Handle(new GetSaleCommand(Guid.Empty), CancellationToken.None);
        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
    }
}
