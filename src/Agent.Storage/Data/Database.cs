using Microsoft.Data.Sqlite;
namespace Agent.Storage.Data;

public class Database
{
    private readonly string _connectionString;
    public Database(string databasePath)
    {
        _connectionString = $"Data Source={databasePath}";
    }
    public SqliteConnection CreateConnection()
    {
        return new SqliteConnection(_connectionString);
    }

    public bool CanConnect()
    {
        try
        {
            using var connection = CreateConnection();
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1;";
            command.ExecuteScalar();
            return true;
        }
        catch
        {
            return false;
        }
    }

}