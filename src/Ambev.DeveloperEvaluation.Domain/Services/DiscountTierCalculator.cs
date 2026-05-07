namespace Ambev.DeveloperEvaluation.Domain.Services;

public static class DiscountTierCalculator
{
    public const int MaxItemsPerProduct = 20;

    public static decimal PercentFor(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than zero");
        if (quantity > MaxItemsPerProduct)
            throw new DomainException($"Cannot sell more than {MaxItemsPerProduct} identical items");
        if (quantity >= 10) return 0.20m;
        if (quantity >= 4) return 0.10m;
        return 0m;
    }
}
