using Microsoft.Data.Sqlite;

namespace Agent.Storage.Data;

public class DatabaseInitializer
{
    private readonly Database _database;

    public DatabaseInitializer(Database database)
    {
        _database = database;
    }

    public void Initialize()
    {
        using var connection = _database.CreateConnection();

        connection.Open();

        using var command = connection.CreateCommand();

        command.CommandText = """
            CREATE TABLE IF NOT EXISTS events
            (
                event_id TEXT PRIMARY KEY,
                timestamp_utc TEXT NOT NULL,
                device_id TEXT NOT NULL,
                user_id TEXT NOT NULL,
                event_type TEXT NOT NULL,
                source TEXT NOT NULL,
                data TEXT NOT NULL,
                delivery_status TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS diagnostics
            (
                id INTEGER PRIMARY KEY CHECK (id = 1),
                last_collection_utc TEXT
            );

            INSERT OR IGNORE INTO diagnostics
            (
                id,
                last_collection_utc
            )
            VALUES
            (
                1,
                NULL
            );
            """;

        command.ExecuteNonQuery();
    }
}