using Microsoft.AspNetCore.Mvc;
using PaymentService.Models;
using PaymentService.Services;

namespace PaymentService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<AccountController> _logger;

    public AccountController(IPaymentService paymentService, ILogger<AccountController> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<AccountResponse>> CreateAccount()
    {
        try
        {
            var result = await _paymentService.CreateAccountAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating account");
            return StatusCode(500, "Internal server error in account creating");
        }
    }

    [HttpGet("{userId}")]
    public async Task<ActionResult<AccountResponse>> GetAccount(Guid userId)
    {
        try
        {
            var result = await _paymentService.GetAccountAsync(userId);
            if (result is null)
            {
                return NotFound($"Account with UserId {userId} not found");
            }
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting account");
            return StatusCode(500, "Interval server error in account getting");
        }
    }

    [HttpPost("deposit")]
    public async Task<ActionResult<BalanceResponse>> Deposit([FromBody] DepositRequest request)
    {
        try
        {
            var result = await _paymentService.DepositAsync(request);
            if (result is null)
            {
                return NotFound($"Account with UserId {request.UserId} not found");
            }
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error depositing account");
            return StatusCode(500, "Interval server error in account depositing");
        }
    }

    [HttpGet("{id}/balance")]
    public async Task<ActionResult<BalanceResponse>> GetAccountBalance(Guid id)
    {
        try
        {
            var result = await _paymentService.GetBalanceAsync(id);
            if (result is null)
            {
                return NotFound($"Account with UserId {id} not found");
            }
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting balance account");
            return StatusCode(500, "Interval server error in account getting balance");
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
