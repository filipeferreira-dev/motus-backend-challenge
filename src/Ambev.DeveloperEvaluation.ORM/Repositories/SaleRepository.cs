using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Ambev.DeveloperEvaluation.ORM.Repositories;

public class SaleRepository : ISaleRepository
{
    private readonly DefaultContext _context;

    public SaleRepository(DefaultContext context)
    {
        _context = context;
    }

    public async Task<Sale> CreateAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        await _context.Sales.AddAsync(sale, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return sale;
    }

    public async Task<Sale> UpdateAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        var trackedItems = _context.ChangeTracker.Entries<SaleItem>()
            .Where(e => e.Entity.SaleId == sale.Id)
            .ToList();
        foreach (var entry in trackedItems)
            entry.State = EntityState.Detached;

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        await _context.Set<SaleItem>()
            .Where(i => i.SaleId == sale.Id)
            .ExecuteDeleteAsync(cancellationToken);

        foreach (var item in sale.Items)
            _context.Set<SaleItem>().Add(item);

        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return sale;
    }

    public Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _context.Sales
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var sale = await _context.Sales
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        if (sale == null) return false;

        _context.Sales.Remove(sale);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> SaleNumberExistsAsync(string saleNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Sales.AnyAsync(s => s.SaleNumber == saleNumber, cancellationToken);
    }

    public async Task<PagedResult<Sale>> ListAsync(SaleListFilters filters, CancellationToken cancellationToken = default)
    {
        var query = _context.Sales.AsQueryable();

        if (filters.CustomerId.HasValue)
            query = query.Where(s => s.CustomerId == filters.CustomerId.Value);
        if (filters.BranchId.HasValue)
            query = query.Where(s => s.BranchId == filters.BranchId.Value);
        if (filters.Status.HasValue)
            query = query.Where(s => s.Status == filters.Status.Value);
        if (!string.IsNullOrWhiteSpace(filters.SaleNumber))
            query = query.Where(s => s.SaleNumber == filters.SaleNumber);
        if (filters.MinSaleDate.HasValue)
            query = query.Where(s => s.SaleDate >= filters.MinSaleDate.Value);
        if (filters.MaxSaleDate.HasValue)
            query = query.Where(s => s.SaleDate <= filters.MaxSaleDate.Value);
        if (filters.MinTotalAmount.HasValue)
            query = query.Where(s => s.TotalAmount >= filters.MinTotalAmount.Value);
        if (filters.MaxTotalAmount.HasValue)
            query = query.Where(s => s.TotalAmount <= filters.MaxTotalAmount.Value);

        var total = await query.CountAsync(cancellationToken);

        query = ApplyOrder(query, filters.Order);

        var page = filters.Page < 1 ? 1 : filters.Page;
        var size = filters.Size < 1 ? 10 : Math.Min(filters.Size, 100);

        var items = await query
            .Include(s => s.Items)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(cancellationToken);

        return new PagedResult<Sale>(items, total, page, size);
    }

    private static IQueryable<Sale> ApplyOrder(IQueryable<Sale> query, string? order)
    {
        if (string.IsNullOrWhiteSpace(order))
            return query.OrderByDescending(s => s.SaleDate);

        IOrderedQueryable<Sale>? ordered = null;
        var clauses = order.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var clause in clauses)
        {
            var parts = clause.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            var field = parts[0];
            var desc = parts.Length > 1 && parts[1].Equals("desc", StringComparison.OrdinalIgnoreCase);

            ordered = ordered == null
                ? ApplyFirstSort(query, field, desc)
                : ApplyThenSort(ordered, field, desc);
        }

        return ordered ?? query.OrderByDescending(s => s.SaleDate);
    }

    private static IOrderedQueryable<Sale> ApplyFirstSort(IQueryable<Sale> query, string field, bool desc)
        => field.ToLowerInvariant() switch
        {
            "salenumber" => desc ? query.OrderByDescending(s => s.SaleNumber) : query.OrderBy(s => s.SaleNumber),
            "saledate" => desc ? query.OrderByDescending(s => s.SaleDate) : query.OrderBy(s => s.SaleDate),
            "customerid" => desc ? query.OrderByDescending(s => s.CustomerId) : query.OrderBy(s => s.CustomerId),
            "branchid" => desc ? query.OrderByDescending(s => s.BranchId) : query.OrderBy(s => s.BranchId),
            "totalamount" => desc ? query.OrderByDescending(s => s.TotalAmount) : query.OrderBy(s => s.TotalAmount),
            "status" => desc ? query.OrderByDescending(s => s.Status) : query.OrderBy(s => s.Status),
            "createdat" => desc ? query.OrderByDescending(s => s.CreatedAt) : query.OrderBy(s => s.CreatedAt),
            _ => query.OrderByDescending(s => s.SaleDate)
        };

    private static IOrderedQueryable<Sale> ApplyThenSort(IOrderedQueryable<Sale> query, string field, bool desc)
        => field.ToLowerInvariant() switch
        {
            "salenumber" => desc ? query.ThenByDescending(s => s.SaleNumber) : query.ThenBy(s => s.SaleNumber),
            "saledate" => desc ? query.ThenByDescending(s => s.SaleDate) : query.ThenBy(s => s.SaleDate),
            "customerid" => desc ? query.ThenByDescending(s => s.CustomerId) : query.ThenBy(s => s.CustomerId),
            "branchid" => desc ? query.ThenByDescending(s => s.BranchId) : query.ThenBy(s => s.BranchId),
            "totalamount" => desc ? query.ThenByDescending(s => s.TotalAmount) : query.ThenBy(s => s.TotalAmount),
            "status" => desc ? query.ThenByDescending(s => s.Status) : query.ThenBy(s => s.Status),
            "createdat" => desc ? query.ThenByDescending(s => s.CreatedAt) : query.ThenBy(s => s.CreatedAt),
            _ => query
        };
}
