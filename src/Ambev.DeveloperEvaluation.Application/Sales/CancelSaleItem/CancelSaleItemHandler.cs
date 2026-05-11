using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentValidation;
using MediatR;
using Rebus.Bus;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;

public class CancelSaleItemHandler : IRequestHandler<CancelSaleItemCommand, CancelSaleItemResponse>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IBus _bus;

    public CancelSaleItemHandler(ISaleRepository saleRepository, IBus bus)
    {
        _saleRepository = saleRepository;
        _bus = bus;
    }

    public async Task<CancelSaleItemResponse> Handle(CancelSaleItemCommand request, CancellationToken cancellationToken)
    {
        var validator = new CancelSaleItemValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var sale = await _saleRepository.GetByIdAsync(request.SaleId, cancellationToken)
            ?? throw new KeyNotFoundException($"Sale with ID {request.SaleId} not found");

        var item = sale.Items.FirstOrDefault(i => i.Id == request.ItemId)
            ?? throw new KeyNotFoundException($"SaleItem {request.ItemId} not found on sale {request.SaleId}");

        sale.CancelItem(request.ItemId);
        await _saleRepository.UpdateAsync(sale, cancellationToken);

        await _bus.Publish(new ItemCancelled
        {
            SaleId = sale.Id,
            SaleNumber = sale.SaleNumber,
            SaleItemId = item.Id,
            ProductId = item.ProductId,
            OccurredAt = DateTime.UtcNow
        });

        return new CancelSaleItemResponse { Success = true, NewTotalAmount = sale.TotalAmount };
    }
}
