using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Unit.ORM.Fixtures;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.ORM;

public class DefaultContextTests
{
    [Fact(DisplayName = "OnModelCreating applies every IEntityTypeConfiguration in the assembly")]
    public void OnModelCreating_AppliesAllConfigurations()
    {
        using var fixture = new SqliteDbContextFixture();

        var saleEntity = fixture.Context.Model.FindEntityType(typeof(Sale))!;
        var itemEntity = fixture.Context.Model.FindEntityType(typeof(SaleItem))!;
        var userEntity = fixture.Context.Model.FindEntityType(typeof(User))!;

        saleEntity.GetTableName().Should().Be("Sales");
        itemEntity.GetTableName().Should().Be("SaleItems");
        userEntity.GetTableName().Should().Be("Users");
    }

    [Fact(DisplayName = "DbSet properties are initialised")]
    public void DbSets_AreInitialised()
    {
        using var fixture = new SqliteDbContextFixture();

        fixture.Context.Sales.Should().NotBeNull();
        fixture.Context.SaleItems.Should().NotBeNull();
        fixture.Context.Users.Should().NotBeNull();
    }
}
