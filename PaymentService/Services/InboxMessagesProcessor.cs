using PaymentService.Data;
using PaymentService.Models;
using System.Text.Json;

namespace PaymentService.Services;

public class InboxProcessor : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<InboxProcessor> _logger;

    public InboxProcessor(IServiceScopeFactory scopeFactory, ILogger<InboxProcessor> logger)
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

                try
                {
                    var messages = dbContext.InboxMessages
                        .Where(x => !x.Processed)
                        .OrderBy(x => x.ReceivedOnUtc)
                        .Take(10)
                        .ToList();

                    if (messages.Any())
                    {
                        _logger.LogInformation("Найдено {Count} сообщений для обработки в Inbox.", messages.Count);
                    }

                    foreach (var msg in messages)
                    {
                        var evt = JsonSerializer.Deserialize<OrderCreatedEvent>(msg.Payload);
                        if (evt is null)
                        {
                            _logger.LogWarning("Не удалось десериализовать событие для InboxMessage {Id}", msg.Id);
                            continue;
                        }

                        var account = dbContext.Accounts.FirstOrDefault(a => a.UserId == evt.AccountId);
                        bool success = false;
                        string? reason = null;

                        if (account == null)
                        {
                            reason = "Account not found";
                        }
                        else if (account.Balance < evt.Price)
                        {
                            reason = "Insufficient funds";
                        }
                        else
                        {
                            account.Balance -= evt.Price;
                            account.UpdatedAt = DateTime.UtcNow;
                            success = true;
                        }

                        var outboxMsg = new OutboxMessage
                        {
                            EventType = "PaymentProcessed",
                            Payload = JsonSerializer.Serialize(new PaymentProcessedEvent
                            {
                                OrderId = evt.OrderId,
                                AccountId = evt.AccountId,
                                Price = evt.Price,
                                Success = success,
                                Reason = reason
                            }),
                            OccurredOnUtc = DateTime.UtcNow
                        };
                        dbContext.OutboxMessages.Add(outboxMsg);

                        msg.Processed = true;
                        msg.ProcessedOnUtc = DateTime.UtcNow;
                    }
                    await dbContext.SaveChangesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Произошла ошибка в InboxProcessor.");
                }
            }
            await Task.Delay(1000, stoppingToken);
        }
    }
}
