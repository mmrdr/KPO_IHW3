namespace OrdersService.Models;

public class OrderResponse
{
    public Guid OrderID { get; set; }

    public Guid AccountID { get; set; }

    public string OrderName { get; set; } = string.Empty;

    public OrderStatus OrderStatus { get; set; }

    public decimal Price { get; set; }

    public DateTime CreatedAt { get; set; }
}