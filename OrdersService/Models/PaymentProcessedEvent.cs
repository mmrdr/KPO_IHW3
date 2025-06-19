namespace OrdersService.Models;

public class PaymentProcessedEvent
{
    public Guid OrderId { get; set; }
    public Guid AccountId { get; set; }
    public decimal Price { get; set; }
    public bool Success { get; set; }
    public string? Reason { get; set; }
}