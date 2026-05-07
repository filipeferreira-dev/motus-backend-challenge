namespace Ambev.DeveloperEvaluation.Application.Sales.Common;

public class SaleItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal LineDiscount { get; set; }
    public decimal LineTotal { get; set; }
    public bool IsCancelled { get; set; }
}
