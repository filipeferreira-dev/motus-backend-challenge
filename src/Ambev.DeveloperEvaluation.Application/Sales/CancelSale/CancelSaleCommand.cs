using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSale;

public record CancelSaleCommand : IRequest<CancelSaleResponse>
{
    public Guid Id { get; }
    public CancelSaleCommand(Guid id) => Id = id;
}

public class CancelSaleResponse
{
    public bool Success { get; set; }
}
