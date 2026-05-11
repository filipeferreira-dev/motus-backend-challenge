namespace Ambev.DeveloperEvaluation.Domain.Events;

public class ItemCancelled
{
    public Guid SaleId { get; set; }
    public string SaleNumber { get; set; } = string.Empty;
    public Guid SaleItemId { get; set; }
    public Guid ProductId { get; set; }
    public DateTime OccurredAt { get; set; }
}
