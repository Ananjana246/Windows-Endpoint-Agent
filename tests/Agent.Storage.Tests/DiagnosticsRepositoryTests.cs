using Agent.Storage.Data;
using Agent.Storage.Repositories;

namespace Agent.Storage.Tests;

public class DiagnosticsRepositoryTests
{
    [Fact]
    public void GetLastCollection_ShouldReturnNullWhenNoCollectionExists()
    {
        var databasePath = Path.Combine(
            Path.GetTempPath(),
            $"diagnostics-{Guid.NewGuid():N}.db");

        try
        {
            var database = new Database(databasePath);
            new DatabaseInitializer(database).Initialize();

            var repository = new DiagnosticsRepository(database);

            var result = repository.GetLastCollection();

            Assert.Null(result);
        }
        finally
        {
            if (File.Exists(databasePath))
            {
                File.Delete(databasePath);
            }
        }
    }
    [Fact]
public void GetLastCollection_ShouldReturnSavedTimestamp()
{
    var databasePath = Path.Combine(
        Path.GetTempPath(),
        $"diagnostics-{Guid.NewGuid():N}.db");

    try
    {
        var database = new Database(databasePath);
        new DatabaseInitializer(database).Initialize();

        var repository = new DiagnosticsRepository(database);
        var expectedTimestamp = new DateTime(
            2026, 10, 26, 10, 30, 0, DateTimeKind.Utc);

        repository.UpdateLastCollection(expectedTimestamp);

        var result = repository.GetLastCollection();

        Assert.NotNull(result);
        Assert.Equal(expectedTimestamp, result.Value);
    }
    finally
    {
        if (File.Exists(databasePath))
        {
            File.Delete(databasePath);
        }
    }
}
}