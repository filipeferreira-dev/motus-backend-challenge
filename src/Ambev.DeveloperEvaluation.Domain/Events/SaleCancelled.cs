namespace Ambev.DeveloperEvaluation.Domain.Events;

public class SaleCancelled
{
    public Guid SaleId { get; set; }
    public string SaleNumber { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
}
