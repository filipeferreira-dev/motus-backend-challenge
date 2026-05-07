using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.DeleteSale;

public record DeleteSaleCommand : IRequest<DeleteSaleResponse>
{
    public Guid Id { get; }
    public DeleteSaleCommand(Guid id) => Id = id;
}

public class DeleteSaleResponse
{
    public bool Success { get; set; }
}
