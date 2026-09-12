using Agent.Storage.Data;

namespace Agent.Storage.Repositories;

public class DiagnosticsRepository
{
    private readonly Database _database;

    public DiagnosticsRepository(Database database)
    {
        _database = database;
    }

    public void UpdateLastCollection(DateTime timestampUtc)
    {
        using var connection = _database.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();

        command.CommandText = """
            UPDATE diagnostics
            SET last_collection_utc = $timestamp
            WHERE id = 1;
            """;

        command.Parameters.AddWithValue(
            "$timestamp",
            timestampUtc.ToString("O"));

        command.ExecuteNonQuery();
    }

    public DateTime? GetLastCollection()
    {
        using var connection = _database.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();

        command.CommandText = """
            SELECT last_collection_utc
            FROM diagnostics
            WHERE id = 1;
            """;

        var result = command.ExecuteScalar();

        if (result == null || result == DBNull.Value)
        {
            return null;
        }

        if (DateTime.TryParse(
                result.ToString(),
                null,
                System.Globalization.DateTimeStyles.RoundtripKind,
                out var timestamp))
        {
            return timestamp;
        }

        return null;
    }
}
