using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Services;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

public class SaleItem : BaseEntity
{
    public Guid SaleId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal LineDiscount { get; set; }
    public decimal LineTotal { get; set; }
    public bool IsCancelled { get; set; }

    private SaleItem() { }

    internal static SaleItem Create(Guid productId, string productName, int quantity, decimal unitPrice)
    {
        if (productId == Guid.Empty)
            throw new DomainException("ProductId is required");
        if (string.IsNullOrWhiteSpace(productName))
            throw new DomainException("ProductName is required");
        if (unitPrice <= 0)
            throw new DomainException("UnitPrice must be greater than zero");

        var discountPercent = DiscountTierCalculator.PercentFor(quantity);
        var gross = quantity * unitPrice;
        var lineDiscount = gross * discountPercent;

        return new SaleItem
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            ProductName = productName,
            Quantity = quantity,
            UnitPrice = unitPrice,
            DiscountPercent = discountPercent,
            LineDiscount = lineDiscount,
            LineTotal = gross - lineDiscount,
            IsCancelled = false
        };
    }

    internal void Cancel()
    {
        if (IsCancelled) return;
        IsCancelled = true;
        LineTotal = 0m;
        LineDiscount = 0m;
    }

    internal decimal Contribution => IsCancelled ? 0m : LineTotal;
}
