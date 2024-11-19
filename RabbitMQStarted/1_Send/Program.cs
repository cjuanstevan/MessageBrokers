using System.Text;
using RabbitMQ.Client;

var factory = new ConnectionFactory() { HostName = "localhost" };

using var connection = await factory.CreateConnectionAsync();

using var channel = await connection.CreateChannelAsync();

await channel.QueueDeclareAsync("hello", false, false, false, null);

string? message = null;

do
{
    Console.WriteLine("Please enter your message");
    message = Console.ReadLine();
    if (!string.IsNullOrEmpty(message))
    {
        SendMessage(channel, message);
    }

} while (!string.IsNullOrEmpty(message));

async void SendMessage(IChannel channel, string message)
{
    var body = Encoding.UTF8.GetBytes(message);
    await channel.BasicPublishAsync(string.Empty, "hello", body);
    Console.WriteLine($"[x] Send {message}");
}