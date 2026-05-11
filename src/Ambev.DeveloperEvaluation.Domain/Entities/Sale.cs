using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Enums;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

public class Sale : BaseEntity
{
    public string SaleNumber { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public SaleStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    private readonly List<SaleItem> _items = new();
    public IReadOnlyCollection<SaleItem> Items => _items.AsReadOnly();

    private Sale() { }

    public static Sale Create(
        string saleNumber,
        DateTime saleDate,
        Guid customerId,
        string customerName,
        Guid branchId,
        string branchName)
    {
        if (string.IsNullOrWhiteSpace(saleNumber))
            throw new DomainException("SaleNumber is required");
        if (customerId == Guid.Empty)
            throw new DomainException("CustomerId is required");
        if (string.IsNullOrWhiteSpace(customerName))
            throw new DomainException("CustomerName is required");
        if (branchId == Guid.Empty)
            throw new DomainException("BranchId is required");
        if (string.IsNullOrWhiteSpace(branchName))
            throw new DomainException("BranchName is required");

        return new Sale
        {
            Id = Guid.NewGuid(),
            SaleNumber = saleNumber,
            SaleDate = saleDate,
            CustomerId = customerId,
            CustomerName = customerName,
            BranchId = branchId,
            BranchName = branchName,
            Status = SaleStatus.Active,
            TotalAmount = 0m,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(
        DateTime saleDate,
        Guid customerId,
        string customerName,
        Guid branchId,
        string branchName)
    {
        EnsureActive();

        if (customerId == Guid.Empty)
            throw new DomainException("CustomerId is required");
        if (string.IsNullOrWhiteSpace(customerName))
            throw new DomainException("CustomerName is required");
        if (branchId == Guid.Empty)
            throw new DomainException("BranchId is required");
        if (string.IsNullOrWhiteSpace(branchName))
            throw new DomainException("BranchName is required");

        SaleDate = saleDate;
        CustomerId = customerId;
        CustomerName = customerName;
        BranchId = branchId;
        BranchName = branchName;
        Touch();
    }

    public SaleItem AddItem(Guid productId, string productName, int quantity, decimal unitPrice)
    {
        EnsureActive();
        var item = SaleItem.Create(productId, productName, quantity, unitPrice);
        item.SaleId = Id;
        _items.Add(item);
        RecalculateTotal();
        Touch();
        return item;
    }

    public void CancelItem(Guid itemId)
    {
        EnsureActive();
        var item = _items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new DomainException($"SaleItem {itemId} not found");
        if (item.IsCancelled) return;
        item.Cancel();
        RecalculateTotal();
        Touch();
    }

    public void Cancel()
    {
        if (Status == SaleStatus.Cancelled) return;
        foreach (var item in _items)
            item.Cancel();
        Status = SaleStatus.Cancelled;
        RecalculateTotal();
        Touch();
    }

    public void ReplaceItems(IEnumerable<(Guid ProductId, string ProductName, int Quantity, decimal UnitPrice)> newItems)
    {
        EnsureActive();
        _items.Clear();
        foreach (var i in newItems)
        {
            var item = SaleItem.Create(i.ProductId, i.ProductName, i.Quantity, i.UnitPrice);
            item.SaleId = Id;
            _items.Add(item);
        }
        RecalculateTotal();
        Touch();
    }

    private void RecalculateTotal()
    {
        TotalAmount = _items.Sum(i => i.Contribution);
    }

    private void Touch()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    private void EnsureActive()
    {
        if (Status == SaleStatus.Cancelled)
            throw new DomainException("Cannot modify a cancelled sale");
    }
}
