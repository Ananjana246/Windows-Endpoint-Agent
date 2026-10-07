using Agent.Storage.Identity;
using Agent.Core.Enums;
using Agent.Core.Models;
using Agent.Storage.Data;
using Agent.Storage.Repositories;

namespace Agent.Storage.Tests;

public class AgentIdentityTests
{
    [Fact]
    public void GetDeviceId_ShouldReturnSameIdAcrossCalls()
    {
        var identityFilePath = Path.Combine(
            Path.GetTempPath(),
            $"agent-identity-{Guid.NewGuid()}.txt");

        var identity = new AgentIdentity(identityFilePath);

        var firstId = identity.GetDeviceId();
        var secondId = identity.GetDeviceId();

        Assert.False(string.IsNullOrWhiteSpace(firstId));
        Assert.Equal(firstId, secondId);

        File.Delete(identityFilePath);
    }
}

public class EventRepositoryTests
{
    [Fact]
    public void Save_ShouldPersistEventAndReturnItFromGetAll()
    {
        var databasePath = Path.Combine(
            Path.GetTempPath(),
            $"agent-events-{Guid.NewGuid()}.db");

        try
        {
            var database = new Database(databasePath);
            var initializer = new DatabaseInitializer(database);
            initializer.Initialize();

            var repository = new EventRepository(database);

            var expectedEvent = new AgentEvent
            {
                EventId = Guid.NewGuid(),
                TimestampUtc = DateTime.UtcNow,
                DeviceId = "test-device",
                UserId = "test-user",
                EventType = EventType.AgentStarted,
                Source = "Test",
                Data = "Test lifecycle event",
                DeliveryStatus = DeliveryStatus.Ready
            };

            repository.Save(expectedEvent);

            var events = repository.GetAll();

            var savedEvent = Assert.Single(events);

            Assert.Equal(expectedEvent.EventId, savedEvent.EventId);
            Assert.Equal(expectedEvent.DeviceId, savedEvent.DeviceId);
            Assert.Equal(expectedEvent.UserId, savedEvent.UserId);
            Assert.Equal(expectedEvent.EventType, savedEvent.EventType);
            Assert.Equal(expectedEvent.Source, savedEvent.Source);
            Assert.Equal(expectedEvent.Data, savedEvent.Data);
            Assert.Equal(expectedEvent.DeliveryStatus, savedEvent.DeliveryStatus);
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
