using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

var factory = new ConnectionFactory() { HostName = "localhost" };

using var connection = await factory.CreateConnectionAsync();

using var channel = await connection.CreateChannelAsync();

await channel.QueueDeclareAsync("hello", false, false, false, null);

Console.WriteLine("[] Waiting for messages...");

var consumer = new AsyncEventingBasicConsumer(channel);
consumer.ReceivedAsync += async (model, ea) =>
{
    var body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);
    Console.WriteLine($"[x] Receive message {message}");

    if (message.Contains("exception"))
    {
        Console.WriteLine("Error in processing");
        await channel.BasicRejectAsync(ea.DeliveryTag, false);
        throw new Exception("Error in processing");
    }

    //Additional processing for this message
    Console.WriteLine($"Processed message {message}");
    await channel.BasicAckAsync(ea.DeliveryTag, false);
};

await channel.BasicConsumeAsync("hello", autoAck: false, consumer);

Console.WriteLine("Press Enter for exit");
Console.Read();
