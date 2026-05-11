using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Specifications;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Specifications;

public class MaxItemsPerProductSpecificationTests
{
    [Theory(DisplayName = "Specification accepts quantities up to 20 and rejects nothing the entity itself rejects first")]
    [InlineData(1, true)]
    [InlineData(20, true)]
    public void IsSatisfiedBy_KnownGoodQuantities(int qty, bool expected)
    {
        var sale = Sale.Create("S-1", DateTime.UtcNow, Guid.NewGuid(), "C", Guid.NewGuid(), "B");
        var item = sale.AddItem(Guid.NewGuid(), "P", qty, 10m);

        new MaxItemsPerProductSpecification().IsSatisfiedBy(item).Should().Be(expected);
    }
}
