using Agent.Core.Models;
using Agent.Storage.Data;
using Agent.Storage.Repositories;

namespace Agent.Storage.Tests;

public class CollectorHealthRepositoryTests
{
    [Fact]
    public void Upsert_ShouldStoreCollectorHealth()
    {
        var databasePath = Path.Combine(
            Path.GetTempPath(),
            $"collector-health-{Guid.NewGuid()}.db");

        try
        {
            var database = new Database(databasePath);
            new DatabaseInitializer(database).Initialize();

            var repository = new CollectorHealthRepository(database);

            var health = new CollectorHealth
            {
                Name = "ProcessCollector",
                Enabled = true,
                Status = "RUNNING",
                LastSuccessUtc = DateTime.UtcNow
            };

            repository.Upsert(health);

            var result = repository.GetAll();

            Assert.Single(result);
            Assert.Equal("ProcessCollector", result[0].Name);
            Assert.True(result[0].Enabled);
            Assert.Equal("RUNNING", result[0].Status);
            Assert.NotNull(result[0].LastSuccessUtc);
        }
        finally
        {
            File.Delete(databasePath);
        }
    }

    [Fact]
    public void Upsert_ShouldUpdateExistingCollector()
    {
        var databasePath = Path.Combine(
            Path.GetTempPath(),
            $"collector-health-{Guid.NewGuid()}.db");

        try
        {
            var database = new Database(databasePath);
            new DatabaseInitializer(database).Initialize();

            var repository = new CollectorHealthRepository(database);

            repository.Upsert(new CollectorHealth
            {
                Name = "FileCollector",
                Enabled = true,
                Status = "RUNNING",
                LastSuccessUtc = DateTime.UtcNow
            });

            repository.Upsert(new CollectorHealth
            {
                Name = "FileCollector",
                Enabled = false,
                Status = "DISABLED",
                LastSuccessUtc = null
            });

            var result = repository.GetAll();

            Assert.Single(result);
            Assert.Equal("FileCollector", result[0].Name);
            Assert.False(result[0].Enabled);
            Assert.Equal("DISABLED", result[0].Status);
            Assert.Null(result[0].LastSuccessUtc);
        }
        finally
        {
            File.Delete(databasePath);
        }
    }

    [Fact]
    public void GetAll_ShouldReturnStoredCollectors()
    {
        var databasePath = Path.Combine(
            Path.GetTempPath(),
            $"collector-health-{Guid.NewGuid()}.db");

        try
        {
            var database = new Database(databasePath);
            new DatabaseInitializer(database).Initialize();

            var repository = new CollectorHealthRepository(database);

            repository.Upsert(new CollectorHealth
            {
                Name = "FileCollector",
                Enabled = true,
                Status = "RUNNING"
            });

            repository.Upsert(new CollectorHealth
            {
                Name = "ProcessCollector",
                Enabled = true,
                Status = "RUNNING"
            });

            var result = repository.GetAll();

            Assert.Equal(2, result.Count);
            Assert.Equal(
                new[] { "FileCollector", "ProcessCollector" },
                result.Select(x => x.Name).ToArray());
        }
        finally
        {
            File.Delete(databasePath);
        }
    }
}
