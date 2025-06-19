using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrdersService.Models;

public class Order
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid OrderID { get; set; }
    
    [Required]
    public Guid AccountID { get; set; }

    [Required]
    public string OrderName { get; set; } = string.Empty;

    [Required]
    public OrderStatus OrderStatus { get; set; }

    [Required]
    public decimal Price { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public DateTime CreatedAt { get; set; }
}

public enum OrderStatus
{
    Waiting,
    Paid,
    Failed
}