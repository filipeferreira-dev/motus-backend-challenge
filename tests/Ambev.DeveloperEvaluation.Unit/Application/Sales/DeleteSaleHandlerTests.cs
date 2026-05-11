using Ambev.DeveloperEvaluation.Application.Sales.DeleteSale;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales;

public class DeleteSaleHandlerTests
{
    private readonly ISaleRepository _repo = Substitute.For<ISaleRepository>();
    private readonly DeleteSaleHandler _handler;

    public DeleteSaleHandlerTests()
    {
        _handler = new DeleteSaleHandler(_repo);
    }

    [Fact(DisplayName = "Existing sale is deleted")]
    public async Task Handle_Existing_Deletes()
    {
        var id = Guid.NewGuid();
        _repo.DeleteAsync(id, Arg.Any<CancellationToken>()).Returns(true);

        var result = await _handler.Handle(new DeleteSaleCommand(id), CancellationToken.None);

        result.Success.Should().BeTrue();
        await _repo.Received(1).DeleteAsync(id, Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Missing sale throws KeyNotFoundException")]
    public async Task Handle_Missing_Throws()
    {
        _repo.DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);

        var act = () => _handler.Handle(new DeleteSaleCommand(Guid.NewGuid()), CancellationToken.None);
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
