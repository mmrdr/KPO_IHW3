using System.Collections;
using Microsoft.AspNetCore.Mvc;
using OrdersService.Models;
using OrdersService.Services;

namespace OrdersController.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrdersService _ordersService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IOrdersService ordersService, ILogger<OrdersController> logger)
    {
        _ordersService = ordersService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<OrderResponse>> CreateOrder([FromBody] CreateOrderRequest request)
    {
        try
        {
            _logger.LogInformation("Я добрался до orders controller");
            var result = await _ordersService.CreateOrder(request);
            _logger.LogInformation("ПОЛУЧИЛОСЬ");
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order");
            return StatusCode(500, "Internal server error in order creating");
        }
    }

    [HttpGet("all/{accountId}")]
    public async Task<ActionResult<IEnumerable<OrderResponse>>> GetAllOrders(Guid accountId)
    {
        try
        {
            var result = await _ordersService.GetAllOrders(accountId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching orders");
            return StatusCode(500, "Internal server error in orders fetching");
        }
    }

    [HttpGet("{orderId}")]
    public async Task<ActionResult<StatusResponse>> GetOrderStatus(Guid orderId)
    {
        try
        {
            var result = await _ordersService.GetOrderStatus(orderId);
            if (result is null)
            {
                return NotFound($"Order with OrderId {orderId} not found");
            }
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching order");
            return StatusCode(500, "Interval server error in order fetching");
        }
    }

    [HttpGet("health")]
    public IActionResult HealthCheck()
    {
        return Ok(new 
        { 
            Status = "Healthy", 
            Timestamp = DateTime.UtcNow,
            Service = "Payment Service",
            Version = "1.0.0"
        });
    }
}
