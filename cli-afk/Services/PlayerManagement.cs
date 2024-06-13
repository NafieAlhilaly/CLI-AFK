using Microsoft.Data.Sqlite;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;
namespace Services
{
    class PlayerManagement
    {
        private readonly SqliteConnection dbConnection = new("Data Source=DBFile.db");
        public void Consume()
        {
            dbConnection.Open();
            var factory = new ConnectionFactory { HostName = "localhost" };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(queue: "actions",
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);
            channel.QueueBind(queue: "actions",
                              exchange: "logs",
                              routingKey: string.Empty);

            EventingBasicConsumer consumer = new(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                string message = Encoding.UTF8.GetString(body);
                Dictionary<string, object> data = JsonSerializer.Deserialize<Dictionary<string, object>>(message);
                string player = data["player"].ToString();
                Earn(player);

            };
            channel.BasicConsume(queue: "hello",
                 autoAck: true,
                 consumer: consumer);
        }
        public void CreatePlayerIfNotExist(string playerName)
        {
            using var insertCommand = dbConnection.CreateCommand();
            insertCommand.CommandText = $"INSERT OR IGNORE into player(name) values ('{playerName}');";
            insertCommand.ExecuteReader();
        }
        public void Earn(string playerName)
        {
            CreatePlayerIfNotExist(playerName);

            using var updatePointsCommand = dbConnection.CreateCommand();
            updatePointsCommand.CommandText = $"INSERT INTO player_earn(player_id, points) values ('{playerName}', 1);";
            updatePointsCommand.ExecuteReader();
        }
    }
}