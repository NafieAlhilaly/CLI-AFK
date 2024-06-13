using Microsoft.Data.Sqlite;
using Services;


using var dbConnection = new SqliteConnection("Data Source=DBFile.db");
dbConnection.Open();
var initTablesCommand = dbConnection.CreateCommand();
initTablesCommand.CommandText = @"
CREATE table if not exists player(id INTEGER PRIMARY KEY AUTOINCREMENT, name text UNIQUE NOT NULL, clan_id text, FOREIGN KEY(clan_id) REFERENCES clan(name));
CREATE table if not exists clan(id INTEGER PRIMARY KEY AUTOINCREMENT, name text NOT NULL UNIQUE, description text);
CREATE table if not exists player_earn(id INTEGER PRIMARY KEY AUTOINCREMENT, player_id INTEGER NOT NULL,points INTEGER NOT NULL, FOREIGN KEY(player_id) REFERENCES player(name));
CREATE table if not exists player_contribution(
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    player_id text NOT NULL,
    points INTEGER NOT NULL,
    clan_id text NOT NULL,
    FOREIGN KEY(player_id) REFERENCES player(name),
    FOREIGN KEY(clan_id) REFERENCES clan(name));
INSERT OR IGNORE INTO clan (name) values ('Knights');
INSERT OR IGNORE INTO clan (name) values ('Samurai');
INSERT OR IGNORE INTO clan (name) values ('Vikings');
";
initTablesCommand.ExecuteReader().Read();


Console.WriteLine("Service is running and waiting for messages to consume!");
ClanManagement clanManagement = new();
PlayerManagement playerManagement = new();
while (true)
{
    playerManagement.Consume();
    clanManagement.Consume();
    Thread.Sleep(1000);
}