using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Services;

namespace Ambev.DeveloperEvaluation.Domain.Specifications;

public class MaxItemsPerProductSpecification : ISpecification<SaleItem>
{
    public bool IsSatisfiedBy(SaleItem item)
        => item.Quantity > 0 && item.Quantity <= DiscountTierCalculator.MaxItemsPerProduct;
}
