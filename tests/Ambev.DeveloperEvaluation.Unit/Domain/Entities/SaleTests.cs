using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities;

public class SaleTests
{
    [Fact(DisplayName = "Create sets Active status, generates Id, defaults TotalAmount to zero")]
    public void Create_WithValidData_InitializesAsActive()
    {
        var sale = Sale.Create("S-1", DateTime.UtcNow, Guid.NewGuid(), "Cust", Guid.NewGuid(), "Br");

        sale.Status.Should().Be(SaleStatus.Active);
        sale.Id.Should().NotBe(Guid.Empty);
        sale.TotalAmount.Should().Be(0m);
        sale.Items.Should().BeEmpty();
    }

    [Theory(DisplayName = "Create rejects empty / blank required fields")]
    [InlineData("", "C", "B")]
    [InlineData("S", "", "B")]
    [InlineData("S", "C", "")]
    public void Create_WithMissingRequiredString_Throws(string number, string customer, string branch)
    {
        Action act = () => Sale.Create(number, DateTime.UtcNow, Guid.NewGuid(), customer, Guid.NewGuid(), branch);
        act.Should().Throw<DomainException>();
    }

    [Fact(DisplayName = "AddItem recalculates TotalAmount to sum of item contributions")]
    public void AddItem_RecalculatesTotal()
    {
        var sale = Sale.Create("S-1", DateTime.UtcNow, Guid.NewGuid(), "Cust", Guid.NewGuid(), "Br");

        sale.AddItem(Guid.NewGuid(), "P1", 2, 10m);   // 20.00
        sale.AddItem(Guid.NewGuid(), "P2", 5, 20m);   // 100 - 10% = 90.00
        sale.AddItem(Guid.NewGuid(), "P3", 15, 8m);   // 120 - 20% = 96.00

        sale.TotalAmount.Should().Be(206m);
        sale.Items.Should().HaveCount(3);
    }

    [Fact(DisplayName = "AddItem propagates DomainException when quantity exceeds 20")]
    public void AddItem_QuantityOver20_Throws()
    {
        var sale = SaleTestData.GenerateValidSale();
        Action act = () => sale.AddItem(Guid.NewGuid(), "P", 21, 10m);
        act.Should().Throw<DomainException>();
    }

    [Fact(DisplayName = "CancelItem zeroes the line and recalculates the total")]
    public void CancelItem_ZeroesLine_AndRecalculates()
    {
        var sale = Sale.Create("S-1", DateTime.UtcNow, Guid.NewGuid(), "Cust", Guid.NewGuid(), "Br");
        var first = sale.AddItem(Guid.NewGuid(), "P1", 2, 10m);  // 20.00
        sale.AddItem(Guid.NewGuid(), "P2", 5, 20m);              // 90.00
        sale.TotalAmount.Should().Be(110m);

        sale.CancelItem(first.Id);

        first.IsCancelled.Should().BeTrue();
        first.LineTotal.Should().Be(0m);
        sale.TotalAmount.Should().Be(90m);
    }

    [Fact(DisplayName = "CancelItem is idempotent")]
    public void CancelItem_Idempotent()
    {
        var sale = Sale.Create("S-1", DateTime.UtcNow, Guid.NewGuid(), "Cust", Guid.NewGuid(), "Br");
        var item = sale.AddItem(Guid.NewGuid(), "P1", 2, 10m);

        sale.CancelItem(item.Id);
        Action act = () => sale.CancelItem(item.Id);

        act.Should().NotThrow();
        item.IsCancelled.Should().BeTrue();
    }

    [Fact(DisplayName = "Cancel marks all items cancelled and zeroes total")]
    public void Cancel_CascadesToAllItems()
    {
        var sale = SaleTestData.GenerateValidSale(itemCount: 3, quantityPerItem: 2);
        var beforeTotal = sale.TotalAmount;
        beforeTotal.Should().BeGreaterThan(0m);

        sale.Cancel();

        sale.Status.Should().Be(SaleStatus.Cancelled);
        sale.Items.Should().OnlyContain(i => i.IsCancelled);
        sale.TotalAmount.Should().Be(0m);
    }

    [Fact(DisplayName = "Cancel on already-cancelled sale is a no-op")]
    public void Cancel_OnAlreadyCancelledSale_NoOp()
    {
        var sale = SaleTestData.GenerateValidSale();
        sale.Cancel();
        Action act = () => sale.Cancel();
        act.Should().NotThrow();
    }

    [Fact(DisplayName = "AddItem after Cancel throws because the sale is no longer active")]
    public void AddItem_OnCancelledSale_Throws()
    {
        var sale = SaleTestData.GenerateValidSale();
        sale.Cancel();

        Action act = () => sale.AddItem(Guid.NewGuid(), "P", 2, 5m);
        act.Should().Throw<DomainException>();
    }

    [Fact(DisplayName = "ReplaceItems rebuilds the item list and re-runs invariants")]
    public void ReplaceItems_RebuildsAndRecalculates()
    {
        var sale = SaleTestData.GenerateValidSale(itemCount: 2, quantityPerItem: 2);

        sale.ReplaceItems(new[]
        {
            (Guid.NewGuid(), "X", 4, 10m),  // 40 - 10% = 36
            (Guid.NewGuid(), "Y", 1, 5m)    // 5
        });

        sale.Items.Should().HaveCount(2);
        sale.TotalAmount.Should().Be(41m);
    }
}
