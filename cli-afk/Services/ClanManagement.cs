using Microsoft.Data.Sqlite;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;

namespace Services
{
    class ClanManagement
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
                ContributePoints(player);
            };
            channel.BasicConsume(queue: "hello",
                 autoAck: true,
                 consumer: consumer);
        }
        public void ContributePoints(string playerName)
        {
            using var getPlayerClanIdCommand = dbConnection.CreateCommand();
            getPlayerClanIdCommand.CommandText = $"SELECT clan_id FROM player where name = '{playerName}';";
            using var reader = getPlayerClanIdCommand.ExecuteReader();
            string? clanId = null;
            while (reader.Read())
            {
                if (!reader.IsDBNull(0))
                {
                    clanId = reader.GetString(0);
                    using var updatePointsCommand = dbConnection.CreateCommand();
                    updatePointsCommand.CommandText = $"INSERT INTO player_contribution(player_id, points, clan_id) values ('{playerName}', 1, '{clanId}');";
                    updatePointsCommand.ExecuteReader();
                }
            }
        }
    }
}