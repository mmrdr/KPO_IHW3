using System.ComponentModel.DataAnnotations;
namespace ApiGateway.Models;

public class CreateOrderRequest
{
    [Required]
    public Guid AccountId { get; set; }
    public string OrderName { get; set; } = string.Empty;

    public decimal Price { get; set; }
}