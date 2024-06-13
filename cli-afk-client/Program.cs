using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();
channel.ExchangeDeclare("logs", ExchangeType.Fanout);
channel.QueueDeclare(queue: "actions",
                     durable: false,
                     exclusive: false,
                     autoDelete: false,
                     arguments: null);
var basicProperties = channel.CreateBasicProperties();
basicProperties.ContentType = "Application/Json";


string? playerName;

Console.WriteLine(@"
----------- Welcome! -------
Enter your player name to login
");
playerName = Console.ReadLine();
int Points = 0;
string sessionId = Guid.NewGuid().ToString();
var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new Dictionary<string, object>{
    {"player", playerName},
}));
while (true)
{
    Console.WriteLine($@"
Welcome {playerName}
    Enter the following options to perform an action:
    1 - Start playing
    2 - List clans
    ");
    string command = Console.ReadLine();
    if (command == "1")
    {
        while (true)
        {
            Points += 1;
            Console.Clear();
            Console.WriteLine($"Welcome {playerName}");
            Console.WriteLine(Points.ToString());
            channel.BasicPublish(
                exchange: "logs",
                routingKey: "actions",
                basicProperties: basicProperties,
                body: body);
            Thread.Sleep(1000);
            Console.Clear();
        }
    }
    else if (command == "2")
    {
        Console.WriteLine(@"
1- Knights
2- Samurai
3- Vikings
        ");
    }
    else
    {
        Console.WriteLine("Unrecognized command");
    }

}