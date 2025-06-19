using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OrderController.Controllers;
using OrdersService.Services;
using OrdersService.Models;

public class OrdersControllerTests
{
    private readonly Mock<IOrdersService> _ordersServiceMock = new();
    private readonly ILogger<OrdersController> _logger = new LoggerFactory().CreateLogger<OrdersController>();
    private OrdersController CreateController() =>
        new OrdersController(_ordersServiceMock.Object, _logger);

    [Fact]
    public async Task CreateOrder_ReturnsOk()
    {
        var request = new CreateOrderRequest { AccountId = Guid.NewGuid(), OrderName = "Test", Price = 100 };
        var response = new OrderResponse
        {
            OrderID = Guid.NewGuid(),
            AccountID = request.AccountId,
            OrderName = request.OrderName,
            Price = request.Price,
            OrderStatus = OrderStatus.Waiting,
            CreatedAt = DateTime.UtcNow
        };
        _ordersServiceMock.Setup(x => x.CreateOrder(request)).ReturnsAsync(response);

        var controller = CreateController();
        var result = await controller.CreateOrder(request);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var value = Assert.IsType<OrderResponse>(okResult.Value);
        Assert.Equal(response.OrderID, value.OrderID);
        Assert.Equal(response.AccountID, value.AccountID);
        Assert.Equal(response.OrderName, value.OrderName);
        Assert.Equal(response.Price, value.Price);
        Assert.Equal(response.OrderStatus, value.OrderStatus);
    }

    [Fact]
    public async Task GetAllOrders_ReturnsOk()
    {
        var accountId = Guid.NewGuid();
        var orders = new List<OrderResponse>
        {
            new OrderResponse
            {
                OrderID = Guid.NewGuid(),
                AccountID = accountId,
                OrderName = "Test",
                Price = 10,
                OrderStatus = OrderStatus.Paid,
                CreatedAt = DateTime.UtcNow
            }
        };
        _ordersServiceMock.Setup(x => x.GetAllOrders(accountId)).ReturnsAsync(orders);

        var controller = CreateController();
        var result = await controller.GetAllOrders(accountId);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var value = Assert.IsType<List<OrderResponse>>(okResult.Value);
        Assert.Single(value);
        Assert.Equal(orders[0].OrderID, value[0].OrderID);
    }

    [Fact]
    public async Task GetOrderStatus_ReturnsNotFound_WhenOrderIsNull()
    {
        var orderId = Guid.NewGuid();
        _ordersServiceMock.Setup(x => x.GetOrderStatus(orderId)).ReturnsAsync((StatusResponse)null);

        var controller = CreateController();
        var result = await controller.GetOrderStatus(orderId);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetOrderStatus_ReturnsOk_WhenOrderExists()
    {
        var orderId = Guid.NewGuid();
        var status = new StatusResponse { OrderStatus = OrderStatus.Paid };
        _ordersServiceMock.Setup(x => x.GetOrderStatus(orderId)).ReturnsAsync(status);

        var controller = CreateController();
        var result = await controller.GetOrderStatus(orderId);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var value = Assert.IsType<StatusResponse>(okResult.Value);
        Assert.Equal(OrderStatus.Paid, value.OrderStatus);
    }
}