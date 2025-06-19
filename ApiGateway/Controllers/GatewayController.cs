using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using ApiGateway.Models;

namespace ApiGateway.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GatewayController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly ServiceUrls _serviceUrls;
    private readonly ILogger<GatewayController> _logger;

    public GatewayController(
        HttpClient httpClient, 
        IOptions<ServiceUrls> serviceUrls, 
        ILogger<GatewayController> logger)
    {
        _httpClient = httpClient;
        _serviceUrls = serviceUrls.Value;
        _logger = logger;
    }

    [HttpPost("account")]
    public async Task<IActionResult> CreateAccount()
    {
        try
        {
            _logger.LogInformation("Trying to create account");

            var response = await _httpClient.PostAsync($"{_serviceUrls.PaymentService}/api/account", null)
            .ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Payment service returned {response.StatusCode}");
                return StatusCode((int)response.StatusCode, new { Error = "Error during account creating" });
            }
            var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            return Ok(jsonResult);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Connectiong error with Payment Service");
            return StatusCode(503, new { Error = "Payment Service cry" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during account creating");
            return StatusCode(500, new { Error = "Internal error" });
        }
    }

    [HttpGet("account/{userId}")]
    public async Task<IActionResult> GetAccount(Guid userId)
    {
        try
        {
            if (userId == Guid.Empty)
            {
                return BadRequest(new { Error = "Необходимо указать корректный UserId" });
            }
            _logger.LogInformation("Getting account {UserId} via Payment Service", userId);

            var response = await _httpClient.GetAsync($"{_serviceUrls.PaymentService}/api/account/{userId}")
                    .ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Payment service returned {response.StatusCode} for account {userId}");
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return NotFound(new { Error = "Account not found" });
                }
                return StatusCode((int)response.StatusCode, new { Error = "Error during account finding" });
            }

            var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            return Ok(jsonResult);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Connectiong error with Payment Service");
            return StatusCode(503, new { Error = "Payment Service cry" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during account findinf");
            return StatusCode(500, new { Error = "Internal error" });
        }
    }

    [HttpGet("account/{userId}/balance")]
    public async Task<IActionResult> GetBalance(Guid userId)
    {
        try
            {
                if (userId == Guid.Empty)
                    return BadRequest(new { Error = "URL must has a UserId" });

                _logger.LogInformation("Getting balance for account {UserId} via Payment Service", userId);

                var response = await _httpClient.GetAsync($"{_serviceUrls.PaymentService}/api/account/{userId}/balance")
                    .ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Payment service returned {response.StatusCode} for balance {userId}");
                    
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                        return NotFound(new { Error = "Account not found" });
                    
                    return StatusCode((int)response.StatusCode, new { Error = "Error during getting balance" });
                }

                var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
                
                return Ok(jsonResult);
            }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Connectiong error with Payment Service");
            return StatusCode(503, new { Error = "Payment Service cry" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during account findinf");
            return StatusCode(500, new { Error = "Internal error" });
        }
    }

    [HttpPost("account/deposit")]
    public async Task<IActionResult> Deposit([FromBody] DepositRequest request)
    {
           try
            {
                if (request.UserId == Guid.Empty)
                    return BadRequest(new { Error = "URL must has a UserId" });

                if (request.Amount <= 0)
                    return BadRequest(new { Error = "Deposit must be more than 0" });

                _logger.LogInformation("Processing deposit for account {UserId}, amount {Amount} via Payment Service", 
                    request.UserId, request.Amount);

                var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_serviceUrls.PaymentService}/api/account/deposit", content)
                    .ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Payment service returned {response.StatusCode} for deposit {request.UserId}");
                    
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                        return NotFound(new { Error = "Account not found" });
                    
                    return StatusCode((int)response.StatusCode, new { Error = "Error during account depositing" });
                }

                var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
                
                return Ok(jsonResult);
            }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Connectiong error with Payment Service");
            return StatusCode(503, new { Error = "Payment Service cry" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during account findinf");
            return StatusCode(500, new { Error = "Internal error" });
        }
    }

    [HttpPost("order")]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        try
        {
            if (request.AccountId == Guid.Empty)
                return BadRequest(new { Error = "URL must has a UserId" });
                
            if (request.OrderName == string.Empty)
                return BadRequest(new { Error = "Order name must be non empty" });

            if (request.Price <= 0)
                return BadRequest(new { Error = "Order price must be more than 0" });

            _logger.LogInformation("Trying to create order");

            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_serviceUrls.OrdersService}/api/orders", content)
            .ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Orders service returned {response.StatusCode}");
                return StatusCode((int)response.StatusCode, new { Error = "Error during order creating" });
            }
            var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            return Ok(jsonResult);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Connectiong error with Orders Service");
            return StatusCode(503, new { Error = "Order Service cry" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during order creating");
            return StatusCode(500, new { Error = "Internal error" });
        }
    }

    [HttpGet("orders/{accountId}")]
    public async Task<IActionResult> GetAllOrders(Guid accountId)
    {
        try
        {
            if (accountId == Guid.Empty)
                return BadRequest("Path must contain account id");
            _logger.LogInformation("Getting orders via Orders Service");

            var response = await _httpClient.GetAsync($"{_serviceUrls.OrdersService}/api/orders/all/{accountId}")
                    .ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Orders service returned {response.StatusCode} for orders fetching");
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return NotFound(new { Error = "Orders not found" });
                }
                return StatusCode((int)response.StatusCode, new { Error = "Error during orders finding" });
            }

            var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            return Ok(jsonResult);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Connectiong error with Orders Service");
            return StatusCode(503, new { Error = "Orders Service cry" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during order finding");
            return StatusCode(500, new { Error = "Internal error" });
        }
    }

    [HttpGet("order/{orderId}")]
    public async Task<IActionResult> GetOrderStatus(Guid orderId)
    {
        try
        {
            if (orderId == Guid.Empty)
            {
                return BadRequest(new { Error = "Must type correct UUID" });
            }
            _logger.LogInformation("Getting order status {OrderId} via Orders Service", orderId);

            var response = await _httpClient.GetAsync($"{_serviceUrls.OrdersService}/api/orders/{orderId}")
                    .ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Orders service returned {response.StatusCode} for order {orderId}");
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return NotFound(new { Error = "Order not found" });
                }
                return StatusCode((int)response.StatusCode, new { Error = "Error during order finding" });
            }

            var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            return Ok(jsonResult);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Connectiong error with Orders Service");
            return StatusCode(503, new { Error = "Orders Service cry" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during order finding");
            return StatusCode(500, new { Error = "Internal error" });
        }
    }


    [HttpGet("health")]
    public IActionResult HealthCheck()
    {
        return Ok(new 
        { 
            Status = "Healthy", 
            Timestamp = DateTime.UtcNow,
            Service = "API Gateway",
            Version = "1.0.0"
        });
    }
}