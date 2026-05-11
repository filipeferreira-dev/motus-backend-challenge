namespace Ambev.DeveloperEvaluation.Domain.Events;

public class SaleCreated
{
    public Guid SaleId { get; set; }
    public string SaleNumber { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
    public decimal TotalAmount { get; set; }
    public int ItemCount { get; set; }
}
