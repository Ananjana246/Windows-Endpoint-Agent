using Agent.Core.Services;
using Agent.Diagnostics;
using Agent.Storage.Data;
using Agent.Storage.Repositories;

namespace Agent.Diagnostics.Tests;

public class HealthServiceTests
{
    [Fact]
    public void GetHealth_ShouldReturnStale_WhenLastCollectionIsTooOld()
    {
        var database = CreateDatabase();
        var deviceIdentity = new DeviceIdentityService(Path.Combine(Path.GetTempPath(), $"agent-device-{Guid.NewGuid():N}.id"));
        var eventRepository = new EventRepository(database);
        var diagnosticsRepository = new DiagnosticsRepository(database);
        var collectorHealthService = new CollectorHealthService(
            new CollectorHealthRepository(database));

        diagnosticsRepository.UpdateLastCollection(DateTime.UtcNow.AddMinutes(-2));

        var service = new HealthService(
            deviceIdentity,
            eventRepository,
            diagnosticsRepository,
            collectorHealthService,
            database);

        var health = service.GetHealth();

        Assert.Equal("STALE", health.Status);
        Assert.True(health.DatabaseHealthy);
    }

    [Fact]
    public void GetHealth_ShouldReturnRunning_WhenLastCollectionIsRecent()
    {
        var database = CreateDatabase();
        var deviceIdentity = new DeviceIdentityService(Path.Combine(Path.GetTempPath(), $"agent-device-{Guid.NewGuid():N}.id"));
        var eventRepository = new EventRepository(database);
        var diagnosticsRepository = new DiagnosticsRepository(database);
        var collectorHealthService = new CollectorHealthService(
            new CollectorHealthRepository(database));

        diagnosticsRepository.UpdateLastCollection(DateTime.UtcNow);

        var service = new HealthService(
            deviceIdentity,
            eventRepository,
            diagnosticsRepository,
            collectorHealthService,
            database);

        var health = service.GetHealth();

        Assert.Equal("RUNNING", health.Status);
        Assert.True(health.DatabaseHealthy);
    }

    private static Database CreateDatabase()
    {
        var databasePath = Path.Combine(
            Path.GetTempPath(),
            $"agent-health-{Guid.NewGuid():N}.db");

        var database = new Database(databasePath);
        var initializer = new DatabaseInitializer(database);
        initializer.Initialize();

        return database;
    }
}
