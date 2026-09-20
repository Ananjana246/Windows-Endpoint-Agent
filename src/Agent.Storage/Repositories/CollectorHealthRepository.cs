using Agent.Core.Models;
using Agent.Storage.Data;

namespace Agent.Storage.Repositories;

public class CollectorHealthRepository
{
    private readonly Database _database;

    public CollectorHealthRepository(Database database)
    {
        _database = database;
    }

    public void Upsert(CollectorHealth health)
    {
        using var connection = _database.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();

        command.CommandText = """
            INSERT INTO collector_health
            (
                name,
                enabled,
                status,
                last_success_utc
            )
            VALUES
            (
                $name,
                $enabled,
                $status,
                $last_success_utc
            )
            ON CONFLICT(name) DO UPDATE SET
                enabled = excluded.enabled,
                status = excluded.status,
                last_success_utc = excluded.last_success_utc;
            """;

        command.Parameters.AddWithValue("$name", health.Name);
        command.Parameters.AddWithValue("$enabled", health.Enabled ? 1 : 0);
        command.Parameters.AddWithValue("$status", health.Status);
        command.Parameters.AddWithValue(
            "$last_success_utc",
            health.LastSuccessUtc?.ToString("O") ?? (object)DBNull.Value);

        command.ExecuteNonQuery();
    }

    public List<CollectorHealth> GetAll()
    {
        using var connection = _database.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();

        command.CommandText = """
            SELECT
                name,
                enabled,
                status,
                last_success_utc
            FROM collector_health
            ORDER BY name;
            """;

        using var reader = command.ExecuteReader();

        var results = new List<CollectorHealth>();

        while (reader.Read())
        {
            DateTime? lastSuccess = null;

            if (!reader.IsDBNull(3) &&
                DateTime.TryParse(
                    reader.GetString(3),
                    null,
                    System.Globalization.DateTimeStyles.RoundtripKind,
                    out var parsed))
            {
                lastSuccess = parsed;
            }

            results.Add(new CollectorHealth
            {
                Name = reader.GetString(0),
                Enabled = reader.GetInt32(1) == 1,
                Status = reader.GetString(2),
                LastSuccessUtc = lastSuccess
            });
        }

        return results;
    }
}
