using Ambev.DeveloperEvaluation.Domain.Entities;
using Bogus;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;

public static class SaleTestData
{
    private static readonly Faker SaleFaker = new();

    public static Sale GenerateValidSale(int itemCount = 1, int quantityPerItem = 2)
    {
        var sale = Sale.Create(
            saleNumber: $"S-{SaleFaker.Random.AlphaNumeric(8).ToUpperInvariant()}",
            saleDate: DateTime.UtcNow,
            customerId: Guid.NewGuid(),
            customerName: SaleFaker.Person.FullName,
            branchId: Guid.NewGuid(),
            branchName: SaleFaker.Company.CompanyName());

        for (var i = 0; i < itemCount; i++)
        {
            sale.AddItem(
                productId: Guid.NewGuid(),
                productName: SaleFaker.Commerce.ProductName(),
                quantity: quantityPerItem,
                unitPrice: Math.Round(SaleFaker.Random.Decimal(1m, 100m), 2));
        }

        return sale;
    }
}
