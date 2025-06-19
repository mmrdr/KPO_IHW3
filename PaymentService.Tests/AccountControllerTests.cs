using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using PaymentService.Controllers;
using PaymentService.Services;
using PaymentService.Models;

public class AccountControllerTests
{
    private readonly Mock<IPaymentService> _paymentServiceMock = new();
    private readonly ILogger<AccountController> _logger = new LoggerFactory().CreateLogger<AccountController>();
    private AccountController CreateController() =>
        new AccountController(_paymentServiceMock.Object, _logger);

    [Fact]
    public async Task CreateAccount_ReturnsOk()
    {
        var response = new AccountResponse { UserId = Guid.NewGuid(), Balance = 0, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        _paymentServiceMock.Setup(x => x.CreateAccountAsync()).ReturnsAsync(response);

        var controller = CreateController();
        var result = await controller.CreateAccount();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var value = Assert.IsType<AccountResponse>(okResult.Value);
        Assert.Equal(response.UserId, value.UserId);
        Assert.Equal(response.Balance, value.Balance);
    }

    [Fact]
    public async Task Deposit_ReturnsNotFound_WhenAccountIsNull()
    {
        var request = new DepositRequest { UserId = Guid.NewGuid(), Amount = 100 };
        _paymentServiceMock.Setup(x => x.DepositAsync(request)).ReturnsAsync((AccountResponse)null);

        var controller = CreateController();
        var result = await controller.Deposit(request);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetBalance_ReturnsOk()
    {
        var userId = Guid.NewGuid();
        var balanceResponse = new BalanceResponse { UserId = userId, Balance = 500 };
        _paymentServiceMock.Setup(x => x.GetBalanceAsync(userId)).ReturnsAsync(balanceResponse);

        var controller = CreateController();
        var result = await controller.GetAccountBalance(userId);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var value = Assert.IsType<BalanceResponse>(okResult.Value);
        Assert.Equal(balanceResponse.Balance, value.Balance);
    }
}
