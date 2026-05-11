using Ambev.DeveloperEvaluation.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities;

public class SaleItemTests
{
    [Theory(DisplayName = "Quantity 1-3 yields 0% discount")]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void Create_QuantityBelow4_NoDiscount(int qty)
    {
        var sale = Sale.Create("S-1", DateTime.UtcNow, Guid.NewGuid(), "C", Guid.NewGuid(), "B");
        var item = sale.AddItem(Guid.NewGuid(), "P", qty, 10m);

        item.DiscountPercent.Should().Be(0m);
        item.LineDiscount.Should().Be(0m);
        item.LineTotal.Should().Be(qty * 10m);
    }

    [Theory(DisplayName = "Quantity 4-9 yields 10% discount")]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(9)]
    public void Create_Quantity4To9_TenPercent(int qty)
    {
        var sale = Sale.Create("S-1", DateTime.UtcNow, Guid.NewGuid(), "C", Guid.NewGuid(), "B");
        var item = sale.AddItem(Guid.NewGuid(), "P", qty, 10m);

        item.DiscountPercent.Should().Be(0.10m);
        item.LineDiscount.Should().Be(qty * 10m * 0.10m);
        item.LineTotal.Should().Be(qty * 10m * 0.90m);
    }

    [Theory(DisplayName = "Quantity 10-20 yields 20% discount")]
    [InlineData(10)]
    [InlineData(15)]
    [InlineData(20)]
    public void Create_Quantity10To20_TwentyPercent(int qty)
    {
        var sale = Sale.Create("S-1", DateTime.UtcNow, Guid.NewGuid(), "C", Guid.NewGuid(), "B");
        var item = sale.AddItem(Guid.NewGuid(), "P", qty, 10m);

        item.DiscountPercent.Should().Be(0.20m);
        item.LineDiscount.Should().Be(qty * 10m * 0.20m);
        item.LineTotal.Should().Be(qty * 10m * 0.80m);
    }

    [Fact(DisplayName = "Quantity 21 throws DomainException")]
    public void Create_QuantityOver20_Throws()
    {
        var sale = Sale.Create("S-1", DateTime.UtcNow, Guid.NewGuid(), "C", Guid.NewGuid(), "B");
        Action act = () => sale.AddItem(Guid.NewGuid(), "P", 21, 10m);
        act.Should().Throw<DomainException>().WithMessage("*more than 20*");
    }

    [Theory(DisplayName = "Quantity zero or negative throws")]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_QuantityZeroOrNegative_Throws(int qty)
    {
        var sale = Sale.Create("S-1", DateTime.UtcNow, Guid.NewGuid(), "C", Guid.NewGuid(), "B");
        Action act = () => sale.AddItem(Guid.NewGuid(), "P", qty, 10m);
        act.Should().Throw<DomainException>();
    }

    [Fact(DisplayName = "UnitPrice <= 0 throws")]
    public void Create_NonPositiveUnitPrice_Throws()
    {
        var sale = Sale.Create("S-1", DateTime.UtcNow, Guid.NewGuid(), "C", Guid.NewGuid(), "B");
        Action act = () => sale.AddItem(Guid.NewGuid(), "P", 2, 0m);
        act.Should().Throw<DomainException>();
    }

    [Fact(DisplayName = "Cancelled item contributes 0 to the sale total")]
    public void Cancelled_LineTotal_IsZero()
    {
        var sale = Sale.Create("S-1", DateTime.UtcNow, Guid.NewGuid(), "C", Guid.NewGuid(), "B");
        var item = sale.AddItem(Guid.NewGuid(), "P", 2, 10m);
        sale.CancelItem(item.Id);

        item.IsCancelled.Should().BeTrue();
        item.LineTotal.Should().Be(0m);
    }
}
