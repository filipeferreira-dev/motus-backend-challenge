using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.ORM.Repositories;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using Ambev.DeveloperEvaluation.Unit.ORM.Fixtures;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.ORM.Repositories;

public class SaleRepositoryTests
{
    private static async Task<Sale> SeedAsync(SaleRepository repo, int itemCount = 1, int quantityPerItem = 2)
    {
        var sale = SaleTestData.GenerateValidSale(itemCount, quantityPerItem);
        await repo.CreateAsync(sale);
        return sale;
    }

    [Fact(DisplayName = "CreateAsync persists sale and its items")]
    public async Task CreateAsync_PersistsSaleWithItems()
    {
        using var fixture = new SqliteDbContextFixture();
        var repo = new SaleRepository(fixture.Context);
        var sale = await SeedAsync(repo, itemCount: 2);

        using var verify = fixture.NewContext();
        var loaded = await verify.Sales.Include(s => s.Items).FirstAsync(s => s.Id == sale.Id);
        loaded.SaleNumber.Should().Be(sale.SaleNumber);
        loaded.Items.Should().HaveCount(2);
        loaded.Items.Should().AllSatisfy(i => i.IsCancelled.Should().BeFalse());
    }

    [Fact(DisplayName = "GetByIdAsync returns sale with items eagerly loaded; null when missing")]
    public async Task GetByIdAsync_HandlesPresentAndMissing()
    {
        using var fixture = new SqliteDbContextFixture();
        var repo = new SaleRepository(fixture.Context);
        var sale = await SeedAsync(repo, itemCount: 3);

        var found = await repo.GetByIdAsync(sale.Id);
        found.Should().NotBeNull();
        found!.Items.Should().HaveCount(3);

        (await repo.GetByIdAsync(Guid.NewGuid())).Should().BeNull();
    }

    [Fact(DisplayName = "SaleNumberExistsAsync is true for existing, false for missing")]
    public async Task SaleNumberExistsAsync_TrueForExisting_FalseForMissing()
    {
        using var fixture = new SqliteDbContextFixture();
        var repo = new SaleRepository(fixture.Context);
        var sale = await SeedAsync(repo);

        (await repo.SaleNumberExistsAsync(sale.SaleNumber)).Should().BeTrue();
        (await repo.SaleNumberExistsAsync("S-DOES-NOT-EXIST")).Should().BeFalse();
    }

    [Fact(DisplayName = "UpdateAsync wipes existing items and inserts the new ones")]
    public async Task UpdateAsync_ReplacesItems()
    {
        using var fixture = new SqliteDbContextFixture();
        var repo = new SaleRepository(fixture.Context);
        var sale = await SeedAsync(repo, itemCount: 3);
        var originalItemIds = sale.Items.Select(i => i.Id).ToHashSet();

        var loaded = (await repo.GetByIdAsync(sale.Id))!;
        loaded.ReplaceItems(new[]
        {
            (Guid.NewGuid(), "Widget Z", 5, 12m),
        });

        await repo.UpdateAsync(loaded);

        using var verify = fixture.NewContext();
        var refreshed = await verify.Sales.Include(s => s.Items).FirstAsync(s => s.Id == sale.Id);
        refreshed.Items.Should().HaveCount(1);
        refreshed.Items.Should().NotContain(i => originalItemIds.Contains(i.Id));
        refreshed.Items.Single().ProductName.Should().Be("Widget Z");
    }

    [Fact(DisplayName = "UpdateAsync persists scalar changes when items are unchanged (cancel-path shape)")]
    public async Task UpdateAsync_UnchangedItems_PersistsScalarChanges()
    {
        using var fixture = new SqliteDbContextFixture();
        var repo = new SaleRepository(fixture.Context);
        var sale = await SeedAsync(repo, itemCount: 2);

        var loaded = (await repo.GetByIdAsync(sale.Id))!;
        loaded.Cancel();

        await repo.UpdateAsync(loaded);

        using var verify = fixture.NewContext();
        var refreshed = await verify.Sales.Include(s => s.Items).FirstAsync(s => s.Id == sale.Id);
        refreshed.Status.Should().Be(SaleStatus.Cancelled);
        refreshed.Items.Should().OnlyContain(i => i.IsCancelled);
    }

    [Fact(DisplayName = "DeleteAsync removes the sale and cascades to its items")]
    public async Task DeleteAsync_Existing_RemovesSaleAndCascadesItems()
    {
        using var fixture = new SqliteDbContextFixture();
        var repo = new SaleRepository(fixture.Context);
        var sale = await SeedAsync(repo, itemCount: 2);

        var deleted = await repo.DeleteAsync(sale.Id);
        deleted.Should().BeTrue();

        using var verify = fixture.NewContext();
        (await verify.Sales.AnyAsync(s => s.Id == sale.Id)).Should().BeFalse();
        (await verify.SaleItems.AnyAsync(i => i.SaleId == sale.Id)).Should().BeFalse();
    }

    [Fact(DisplayName = "DeleteAsync returns false for a missing sale")]
    public async Task DeleteAsync_Missing_ReturnsFalse()
    {
        using var fixture = new SqliteDbContextFixture();
        var repo = new SaleRepository(fixture.Context);

        (await repo.DeleteAsync(Guid.NewGuid())).Should().BeFalse();
    }

    [Fact(DisplayName = "ListAsync filters by CustomerId / BranchId / Status / SaleNumber")]
    public async Task ListAsync_FiltersBySimpleEqualities()
    {
        using var fixture = new SqliteDbContextFixture();
        var repo = new SaleRepository(fixture.Context);

        var customerA = Guid.NewGuid();
        var branchA = Guid.NewGuid();

        var matching = SaleTestData.GenerateValidSale(itemCount: 1);
        typeof(Sale).GetProperty(nameof(Sale.CustomerId))!.SetValue(matching, customerA);
        typeof(Sale).GetProperty(nameof(Sale.BranchId))!.SetValue(matching, branchA);
        typeof(Sale).GetProperty(nameof(Sale.SaleNumber))!.SetValue(matching, "S-FILTER-1");
        await repo.CreateAsync(matching);

        // Two more sales unrelated to any filter
        await SeedAsync(repo);
        await SeedAsync(repo);

        (await repo.ListAsync(new SaleListFilters { CustomerId = customerA })).Items.Should().ContainSingle(s => s.Id == matching.Id);
        (await repo.ListAsync(new SaleListFilters { BranchId = branchA })).Items.Should().ContainSingle(s => s.Id == matching.Id);
        (await repo.ListAsync(new SaleListFilters { Status = SaleStatus.Active })).Items.Should().Contain(s => s.Id == matching.Id);
        (await repo.ListAsync(new SaleListFilters { SaleNumber = "S-FILTER-1" })).Items.Should().ContainSingle(s => s.Id == matching.Id);
    }

    [Fact(DisplayName = "ListAsync filters by SaleDate range and TotalAmount range")]
    public async Task ListAsync_FiltersByRanges()
    {
        using var fixture = new SqliteDbContextFixture();
        var repo = new SaleRepository(fixture.Context);

        var older = SaleTestData.GenerateValidSale(itemCount: 1);
        typeof(Sale).GetProperty(nameof(Sale.SaleDate))!.SetValue(older, new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        typeof(Sale).GetProperty(nameof(Sale.TotalAmount))!.SetValue(older, 50m);
        await repo.CreateAsync(older);

        var newer = SaleTestData.GenerateValidSale(itemCount: 1);
        typeof(Sale).GetProperty(nameof(Sale.SaleDate))!.SetValue(newer, new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc));
        typeof(Sale).GetProperty(nameof(Sale.TotalAmount))!.SetValue(newer, 500m);
        await repo.CreateAsync(newer);

        var dateFiltered = await repo.ListAsync(new SaleListFilters
        {
            MinSaleDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            MaxSaleDate = new DateTime(2027, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        });
        dateFiltered.Items.Should().ContainSingle(s => s.Id == newer.Id);

        var amountFiltered = await repo.ListAsync(new SaleListFilters
        {
            MinTotalAmount = 100m,
            MaxTotalAmount = 1000m,
        });
        amountFiltered.Items.Should().ContainSingle(s => s.Id == newer.Id);
    }

    [Fact(DisplayName = "ListAsync paging respects Page and Size and reports TotalCount")]
    public async Task ListAsync_PagingHonorsPageAndSize()
    {
        using var fixture = new SqliteDbContextFixture();
        var repo = new SaleRepository(fixture.Context);

        for (var i = 0; i < 8; i++)
            await SeedAsync(repo);

        var page2 = await repo.ListAsync(new SaleListFilters { Page = 2, Size = 3 });

        page2.Items.Should().HaveCount(3);
        page2.TotalCount.Should().Be(8);
        page2.Page.Should().Be(2);
        page2.Size.Should().Be(3);
    }

    [Theory(DisplayName = "ListAsync ordering runs every SQLite-translatable arm of ApplyFirstSort")]
    [InlineData("saleNumber asc")]
    [InlineData("saleNumber desc")]
    [InlineData("saleDate asc")]
    [InlineData("saleDate desc")]
    [InlineData("customerId asc")]
    [InlineData("branchId desc")]
    [InlineData("status desc")]
    [InlineData("createdAt asc")]
    [InlineData("unknownField asc")]
    public async Task ListAsync_OrderingHandlesEveryField(string order)
    {
        using var fixture = new SqliteDbContextFixture();
        var repo = new SaleRepository(fixture.Context);
        await SeedAsync(repo);
        await SeedAsync(repo);

        var result = await repo.ListAsync(new SaleListFilters { Order = order });

        result.Items.Should().NotBeEmpty();
    }

    [Theory(DisplayName = "ListAsync multi-field ordering runs every SQLite-translatable arm of ApplyThenSort")]
    [InlineData("createdAt asc, saleNumber asc")]
    [InlineData("createdAt asc, saleNumber desc")]
    [InlineData("createdAt asc, saleDate asc")]
    [InlineData("createdAt asc, saleDate desc")]
    [InlineData("createdAt asc, customerId asc")]
    [InlineData("createdAt asc, customerId desc")]
    [InlineData("createdAt asc, branchId asc")]
    [InlineData("createdAt asc, branchId desc")]
    [InlineData("createdAt asc, status asc")]
    [InlineData("createdAt asc, status desc")]
    [InlineData("customerId asc, saleDate desc")]
    [InlineData("saleDate desc, createdAt asc")]
    [InlineData("saleDate desc, unknownField asc")]
    public async Task ListAsync_MultiFieldOrdering_HitsEveryArm(string order)
    {
        using var fixture = new SqliteDbContextFixture();
        var repo = new SaleRepository(fixture.Context);
        await SeedAsync(repo);
        await SeedAsync(repo);
        await SeedAsync(repo);

        var result = await repo.ListAsync(new SaleListFilters { Order = order });

        result.Items.Should().HaveCount(3);
    }

    [Fact(DisplayName = "ListAsync clamps Page<1 to 1 and Size<1 to 10")]
    public async Task ListAsync_ClampsPagingDefaults()
    {
        using var fixture = new SqliteDbContextFixture();
        var repo = new SaleRepository(fixture.Context);
        for (var i = 0; i < 3; i++)
            await SeedAsync(repo);

        var result = await repo.ListAsync(new SaleListFilters { Page = 0, Size = 0 });

        result.Items.Should().HaveCount(3);
        result.TotalCount.Should().Be(3);
    }

    [Fact(DisplayName = "ListAsync clamps Size above 100 down to 100")]
    public async Task ListAsync_ClampsSizeAboveCeiling()
    {
        using var fixture = new SqliteDbContextFixture();
        var repo = new SaleRepository(fixture.Context);
        await SeedAsync(repo);

        var result = await repo.ListAsync(new SaleListFilters { Size = 5000 });

        result.Size.Should().Be(100);
    }

    [Fact(DisplayName = "ListAsync with null Order falls back to default OrderByDescending(SaleDate)")]
    public async Task ListAsync_NullOrder_UsesDefault()
    {
        using var fixture = new SqliteDbContextFixture();
        var repo = new SaleRepository(fixture.Context);
        var a = SaleTestData.GenerateValidSale(itemCount: 1);
        a.SaleDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var b = SaleTestData.GenerateValidSale(itemCount: 1);
        b.SaleDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        await repo.CreateAsync(a);
        await repo.CreateAsync(b);

        var result = await repo.ListAsync(new SaleListFilters());

        result.Items.First().Id.Should().Be(b.Id);
        result.Items.Last().Id.Should().Be(a.Id);
    }
}
