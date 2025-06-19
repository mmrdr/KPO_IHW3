using Microsoft.AspNetCore.Mvc;
using OrdersService.RabbitMq;

namespace OrdersService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RabbitMqController : ControllerBase
{
    private readonly IRabbitMqService _rabbitMqService;

    private readonly ILogger<RabbitMqController> _logger;

    public RabbitMqController(IRabbitMqService rabbitMqService, ILogger<RabbitMqController> logger)
    {
        _rabbitMqService = rabbitMqService;
        _logger = logger;
    }

    [HttpGet]
    [Route("[action]/{message}")]
    public async Task<IActionResult> SendMessage(string message)
    {
        await _rabbitMqService.SendMessageAsync(message);
        return Ok("Message sent");
    } 
}