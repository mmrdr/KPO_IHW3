namespace PaymentService.RabbitMq;

public interface IRabbitMqService
{
    Task SendMessageAsync(object obj);
    Task SendMessageAsync(string message);
}