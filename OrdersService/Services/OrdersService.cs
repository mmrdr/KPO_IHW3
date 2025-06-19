using Microsoft.EntityFrameworkCore;
using OrdersService.Data;
using System.Text.Json;
using OrdersService.Models;

namespace OrdersService.Services;

public class OrderService : IOrdersService
{
    private readonly OrdersDbContext _context;
    private readonly ILogger<OrderService> _logger;

    public OrderService(OrdersDbContext context, ILogger<OrderService> logger)
    {
        _context = context;
        _logger = logger;
    }

public async Task<OrderResponse?> CreateOrder(CreateOrderRequest request)
{
    try
    {
        var strategy = _context.Database.CreateExecutionStrategy();

        var orderResponse = await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            _logger.LogInformation("Начинаем транзакцию для создания заказа");
            
            var order = new Order
            {
                OrderID = Guid.NewGuid(),
                AccountID = request.AccountId,
                OrderName = request.OrderName,
                Price = request.Price,
                OrderStatus = OrderStatus.Waiting,
                CreatedAt = DateTime.UtcNow,
            };
            _context.Orders.Add(order);

            var outboxMessage = new OutboxMessage {
                EventType = "OrderCreated",
                Payload = JsonSerializer.Serialize(new {
                    OrderId = order.OrderID,
                    AccountId = order.AccountID,
                    Price = order.Price,
                    OrderName = order.OrderName
                }),
                OccurredOnUtc = DateTime.UtcNow
            };
            _context.OutboxMessages.Add(outboxMessage);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Заказ {OrderId} успешно создан и закоммичен", order.OrderID);

            return new OrderResponse
            {
                OrderID = order.OrderID,
                AccountID = order.AccountID,
                OrderName = order.OrderName,
                Price = order.Price,
                OrderStatus = OrderStatus.Waiting,
                CreatedAt = order.CreatedAt,
            };
        });

        return orderResponse;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Ошибка при создании заказа после всех попыток");
        throw;
    }
}


    public async Task<IEnumerable<OrderResponse>?> GetAllOrders(Guid accountId)
    {
        var orders = await _context.Orders
            .Where(o => o.AccountID == accountId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
        return orders.Select(MapToOrderResponse);
    }

    public async Task<StatusResponse?> GetOrderStatus(Guid id)
    {
        var order = await _context.Orders
            .Where(o => o.OrderID == id)
            .FirstOrDefaultAsync();
        return order != null ? new StatusResponse { OrderStatus = order.OrderStatus } : null;
    }
    
    private static OrderResponse MapToOrderResponse(Order order)
    {
        return new OrderResponse
        {
            OrderID = order.OrderID,
            AccountID = order.AccountID,
            OrderName = order.OrderName,
            OrderStatus = order.OrderStatus,
            Price = order.Price,
            CreatedAt = order.CreatedAt
        };
    }
}