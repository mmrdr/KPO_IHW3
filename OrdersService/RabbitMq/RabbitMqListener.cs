using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using OrdersService.Models;
using OrdersService.Data;

public class RabbitMqListener : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RabbitMqListener> _logger;
    private IConnection _connection;
    private IChannel _channel;

    public RabbitMqListener(IServiceScopeFactory scopeFactory, ILogger<RabbitMqListener> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory()
        {
            HostName = "rabbit-mq",
            UserName = "rabbitmq",
            Password = "secret"
        };
        _connection = await factory.CreateConnectionAsync();
        _channel = await _connection.CreateChannelAsync();

        await _channel.QueueDeclareAsync(queue: "payment-to-order-queue",
                              durable: false,
                              exclusive: false,
                              autoDelete: false,
                              arguments: null);

        await base.StartAsync(cancellationToken);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (ch, ea) =>
        {
            try
            {
                var content = Encoding.UTF8.GetString(ea.Body.ToArray());
                _logger.LogInformation($"Получено сообщение из payment-to-order-queue: {content}");

                var evt = JsonSerializer.Deserialize<PaymentProcessedEvent>(content);

                using var scope = _scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();

                var order = await dbContext.Orders.FindAsync(evt.OrderId);
                if (order != null)
                {
                    order.OrderStatus = evt.Success ? OrderStatus.Paid : OrderStatus.Failed;
                    await dbContext.SaveChangesAsync();
                }

                await _channel.BasicAckAsync(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обработке сообщения в OrdersService Listener");
            }
        };

        _channel.BasicConsumeAsync(queue: "payment-to-order-queue", autoAck: false, consumer: consumer);
        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel?.CloseAsync();
        _connection?.CloseAsync();
        base.Dispose();
    }
}
