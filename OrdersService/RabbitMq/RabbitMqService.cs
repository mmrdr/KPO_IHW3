using System.Text.Json;
using System.Text;
using RabbitMQ.Client;

namespace OrdersService.RabbitMq;

public class RabbitMqService : IRabbitMqService
{
    public async Task SendMessageAsync(object obj)
    {
        var message = JsonSerializer.Serialize(obj);
        await SendMessageAsync(message);
    }

    public async Task SendMessageAsync(string message)
    {
        var factory = new ConnectionFactory() { HostName = "rabbit-mq", Password = "secret", UserName = "rabbitmq" };
        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();
        await channel.QueueDeclareAsync(queue: "order-to-payment-queue",
                       durable: false,
                       exclusive: false,
                       autoDelete: false,
                       arguments: null);

        var body = Encoding.UTF8.GetBytes(message);

        await channel.BasicPublishAsync<BasicProperties>(exchange: "",
                       routingKey: "order-to-payment-queue",
                       mandatory: false,
                       basicProperties: new BasicProperties(),
                       body: body);
    }
}