namespace Ambev.DeveloperEvaluation.Domain.Events;

public class SaleModified
{
    public Guid SaleId { get; set; }
    public string SaleNumber { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
    public decimal TotalAmount { get; set; }
}
