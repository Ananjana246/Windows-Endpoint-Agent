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
}