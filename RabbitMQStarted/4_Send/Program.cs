using System.Text;
using RabbitMQ.Client;

var factory = new ConnectionFactory() { HostName = "localhost" };

using var connection = await factory.CreateConnectionAsync();

using var channel = await connection.CreateChannelAsync();

//await channel.QueueDeclareAsync("hello", false, false, false, null);
var exchangeName = "weather_direct";
await channel.ExchangeDeclareAsync(exchangeName, ExchangeType.Direct);
string? message = null;

do
{
    Console.WriteLine("Please enter your message");
    message = Console.ReadLine();

    Console.WriteLine("Please enter your routing key");
    var routingKey = Console.ReadLine();

    if (!string.IsNullOrEmpty(message))
    {
        SendMessage(channel, message, routingKey ?? string.Empty);
    }

} while (!string.IsNullOrEmpty(message));

async void SendMessage(IChannel channel, string message, string routingKey)
{
    var body = Encoding.UTF8.GetBytes(message);
    await channel.BasicPublishAsync(exchangeName, routingKey, body);
    Console.WriteLine($"[x] Send {message}");
}
