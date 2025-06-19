using PaymentService.Data;
using PaymentService.RabbitMq;

namespace PaymentService.Services;

public class OutboxProcessor : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxProcessor> _logger;

    public OutboxProcessor(
        IServiceScopeFactory scopeFactory,
        ILogger<OutboxProcessor> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
                var rabbitMqService = scope.ServiceProvider.GetRequiredService<IRabbitMqService>();

                var messages = dbContext.OutboxMessages
                    .Where(x => x.ProcessedOnUtc == null)
                    .OrderBy(x => x.OccurredOnUtc)
                    .Take(10)
                    .ToList();

                foreach (var msg in messages)
                {
                    try
                    {
                        await rabbitMqService.SendMessageAsync(msg.Payload);
                        msg.ProcessedOnUtc = DateTime.UtcNow;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed during sending message from outbox");
                    }
                }
                await dbContext.SaveChangesAsync(stoppingToken);
            }
            await Task.Delay(1000, stoppingToken);
        }
    }
}
