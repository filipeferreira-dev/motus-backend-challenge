using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Bogus;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales.TestData;

public static class CreateSaleHandlerTestData
{
    private static readonly Faker<SaleItemInput> ItemFaker = new Faker<SaleItemInput>()
        .RuleFor(i => i.ProductId, _ => Guid.NewGuid())
        .RuleFor(i => i.ProductName, f => f.Commerce.ProductName())
        .RuleFor(i => i.Quantity, f => f.Random.Int(1, 3))
        .RuleFor(i => i.UnitPrice, f => Math.Round(f.Random.Decimal(1m, 50m), 2));

    private static readonly Faker<CreateSaleCommand> Faker = new Faker<CreateSaleCommand>()
        .RuleFor(s => s.SaleNumber, f => $"S-{f.Random.AlphaNumeric(8).ToUpperInvariant()}")
        .RuleFor(s => s.SaleDate, f => f.Date.Recent())
        .RuleFor(s => s.CustomerId, _ => Guid.NewGuid())
        .RuleFor(s => s.CustomerName, f => f.Person.FullName)
        .RuleFor(s => s.BranchId, _ => Guid.NewGuid())
        .RuleFor(s => s.BranchName, f => f.Company.CompanyName())
        .RuleFor(s => s.Items, _ => ItemFaker.Generate(2));

    public static CreateSaleCommand GenerateValidCommand() => Faker.Generate();
}
