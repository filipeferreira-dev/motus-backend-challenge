using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.GetSale;

public record GetSaleCommand : IRequest<GetSaleResult>
{
    public Guid Id { get; }
    public GetSaleCommand(Guid id) => Id = id;
}
