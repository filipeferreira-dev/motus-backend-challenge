using Ambev.DeveloperEvaluation.Domain.Services;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Services;

public class DiscountTierCalculatorTests
{
    [Theory(DisplayName = "PercentFor returns the correct tier at every boundary")]
    [InlineData(1, 0.00)]
    [InlineData(3, 0.00)]
    [InlineData(4, 0.10)]
    [InlineData(9, 0.10)]
    [InlineData(10, 0.20)]
    [InlineData(20, 0.20)]
    public void PercentFor_Boundaries(int qty, decimal expected)
    {
        DiscountTierCalculator.PercentFor(qty).Should().Be(expected);
    }

    [Theory(DisplayName = "PercentFor throws on invalid quantities")]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(21)]
    [InlineData(100)]
    public void PercentFor_Invalid_Throws(int qty)
    {
        Action act = () => DiscountTierCalculator.PercentFor(qty);
        act.Should().Throw<DomainException>();
    }
}
