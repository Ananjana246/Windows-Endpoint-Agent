using Agent.Core.Models;
using Agent.Diagnostics;
using Agent.Storage.Data;
using Agent.Storage.Repositories;

namespace Agent.Diagnostics.Tests;

public class CollectorHealthServiceTests
{
    [Fact]
    public void MarkSuccess_ShouldStoreRunningHealth()
    {
        var database = CreateDatabase();
        var repository = new CollectorHealthRepository(database);
        var service = new CollectorHealthService(repository);

        service.MarkSuccess("ProcessCollector");

        var health = service.GetAll().Single();

        Assert.Equal("ProcessCollector", health.Name);
        Assert.True(health.Enabled);
        Assert.Equal("RUNNING", health.Status);
        Assert.NotNull(health.LastSuccessUtc);
    }

    [Fact]
    public void MarkFailure_ShouldPreserveLastSuccess()
    {
        var database = CreateDatabase();
        var repository = new CollectorHealthRepository(database);
        var service = new CollectorHealthService(repository);

        service.MarkSuccess("ProcessCollector");
        var previousSuccess = service.GetAll().Single().LastSuccessUtc;

        service.MarkFailure("ProcessCollector");

        var health = service.GetAll().Single();

        Assert.True(health.Enabled);
        Assert.Equal("FAILED", health.Status);
        Assert.Equal(previousSuccess, health.LastSuccessUtc);
    }

    [Fact]
    public void MarkDisabled_ShouldStoreDisabledHealth()
    {
        var database = CreateDatabase();
        var repository = new CollectorHealthRepository(database);
        var service = new CollectorHealthService(repository);

        service.MarkDisabled("ProcessCollector");

        var health = service.GetAll().Single();

        Assert.Equal("ProcessCollector", health.Name);
        Assert.False(health.Enabled);
        Assert.Equal("DISABLED", health.Status);
        Assert.Null(health.LastSuccessUtc);
    }

    private static Database CreateDatabase()
    {
        var databasePath = Path.Combine(
            Path.GetTempPath(),
            $"agent-diagnostics-{Guid.NewGuid():N}.db");

        var database = new Database(databasePath);
        var initializer = new DatabaseInitializer(database);
        initializer.Initialize();

        return database;
    }
}
