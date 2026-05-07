using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Application.Sales.TestData;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Rebus.Bus;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales;

public class CreateSaleHandlerTests
{
    private readonly ISaleRepository _repo = Substitute.For<ISaleRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private readonly IBus _bus = Substitute.For<IBus>();
    private readonly CreateSaleHandler _handler;

    public CreateSaleHandlerTests()
    {
        _handler = new CreateSaleHandler(_repo, _mapper, _bus);
    }

    [Fact(DisplayName = "Valid command persists sale and publishes SaleCreated")]
    public async Task Handle_Valid_PersistsAndPublishes()
    {
        var command = CreateSaleHandlerTestData.GenerateValidCommand();
        _repo.SaleNumberExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        _repo.CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
             .Returns(ci => ci.Arg<Sale>());
        _mapper.Map<CreateSaleResult>(Arg.Any<Sale>()).Returns(new CreateSaleResult());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        await _repo.Received(1).CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
        await _bus.Received(1).Publish(Arg.Any<SaleCreated>(), Arg.Any<Dictionary<string, string>>());
    }

    [Fact(DisplayName = "Invalid command throws ValidationException and skips publish")]
    public async Task Handle_Invalid_Throws()
    {
        var command = new CreateSaleCommand(); // empty -> validator fails

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
        await _repo.DidNotReceive().CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
        await _bus.DidNotReceive().Publish(Arg.Any<SaleCreated>(), Arg.Any<Dictionary<string, string>>());
    }

    [Fact(DisplayName = "Quantity > 20 in any item bubbles DomainException and does not publish")]
    public async Task Handle_QuantityOver20_Throws_NoPublish()
    {
        var command = CreateSaleHandlerTestData.GenerateValidCommand();
        command.Items[0].Quantity = 21;

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
        await _repo.DidNotReceive().CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
        await _bus.DidNotReceive().Publish(Arg.Any<SaleCreated>(), Arg.Any<Dictionary<string, string>>());
    }

    [Fact(DisplayName = "Existing sale number returns InvalidOperationException")]
    public async Task Handle_DuplicateSaleNumber_Throws()
    {
        var command = CreateSaleHandlerTestData.GenerateValidCommand();
        _repo.SaleNumberExistsAsync(command.SaleNumber, Arg.Any<CancellationToken>()).Returns(true);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
