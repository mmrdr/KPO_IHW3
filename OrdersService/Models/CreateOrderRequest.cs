namespace OrdersService.Models;

public class CreateOrderRequest
{
    public Guid AccountId { get; set; }
    
    public string OrderName { get; set; } = string.Empty;

    public decimal Price { get; set; }
}