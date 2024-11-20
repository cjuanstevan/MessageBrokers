using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

var factory = new ConnectionFactory() { HostName = "localhost" };

using var connection = await factory.CreateConnectionAsync();

using var channel = await connection.CreateChannelAsync();

//Definimos el nombre de la cola
Console.WriteLine("Please enter queue name");
var queueName = Console.ReadLine();
await channel.QueueDeclareAsync(queueName, false, false, false, null);

//Definimos las claves de ruta
Console.WriteLine("Please enter routin keys");
var routingKey = Console.ReadLine();
string[] routingKeys = routingKey.Split(",", StringSplitOptions.RemoveEmptyEntries);

//Defino el mismo nombre del intercambio del remitente
var exchangeName = "weather_direct";
if (routingKeys.Any())
{
    foreach (var key in routingKeys)
    {
        await channel.QueueBindAsync(queueName, exchangeName, key);
    }
}
else
{
    await channel.QueueBindAsync(queueName, exchangeName, string.Empty);
}


//Se define que solamente obtenga de a 1 mensaje y que si está ocupado no reciba más
await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false);

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

    if (int.TryParse(message, out var delayTime))
    {
        Thread.Sleep(delayTime * 1000);
    }

    //Additional processing for this message
    Console.WriteLine($"Processed message {message}");
    await channel.BasicAckAsync(ea.DeliveryTag, false);
};

await channel.BasicConsumeAsync(queueName, autoAck: false, consumer);

Console.WriteLine("Press Enter for exit");
Console.Read();