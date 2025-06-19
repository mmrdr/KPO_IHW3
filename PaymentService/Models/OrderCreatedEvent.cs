namespace PaymentService.Models;
public class OrderCreatedEvent
{
    public Guid OrderId { get; set; }
    public Guid AccountId { get; set; }
    public decimal Price { get; set; }
    public string OrderName { get; set; } = string.Empty;
}