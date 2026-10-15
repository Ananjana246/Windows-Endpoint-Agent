using Agent.Core.Enums;
using Agent.Core.Models;
using Agent.Storage.Data;
using Agent.Storage.Repositories;

namespace Agent.Storage.Tests;

public class EventRepositoryUpdateStatusTests
{
    [Fact]
    public void UpdateStatus_ShouldUpdateStoredEventDeliveryStatus()
    {
        var databasePath = Path.Combine(
            Path.GetTempPath(),
            $"event-repository-{Guid.NewGuid()}.db");

        try
        {
            var database = new Database(databasePath);
            new DatabaseInitializer(database).Initialize();

            var repository = new EventRepository(database);

            var agentEvent = new AgentEvent
            {
                EventId = Guid.NewGuid(),
                TimestampUtc = DateTime.UtcNow,
                DeviceId = "test-device",
                UserId = "test-user",
                EventType = EventType.AgentStarted,
                Source = "EventRepositoryTests",
                Data = "{}",
                DeliveryStatus = DeliveryStatus.Local
            };

            repository.Save(agentEvent);
            repository.UpdateStatus(
                agentEvent.EventId,
                DeliveryStatus.Processed);

            var result = repository.GetByStatus(
                DeliveryStatus.Processed);

            var storedEvent = Assert.Single(result);
            Assert.Equal(agentEvent.EventId, storedEvent.EventId);
            Assert.Equal(
                DeliveryStatus.Processed,
                storedEvent.DeliveryStatus);
        }
        finally
        {
            File.Delete(databasePath);
        }
    }

    [Fact]
    public void GetByStatus_ShouldReturnOnlyMatchingEvents()
    {
        var databasePath = Path.Combine(
            Path.GetTempPath(),
            $"event-status-{Guid.NewGuid()}.db");

        try
        {
            var database = new Database(databasePath);
            new DatabaseInitializer(database).Initialize();

            var repository = new EventRepository(database);

            var localEvent = new AgentEvent
            {
                EventId = Guid.NewGuid(),
                TimestampUtc = DateTime.UtcNow,
                DeviceId = "test-device",
                UserId = "test-user",
                EventType = EventType.AgentStarted,
                Source = "EventRepositoryTests",
                Data = "{}",
                DeliveryStatus = DeliveryStatus.Local
            };

            var failedEvent = new AgentEvent
            {
                EventId = Guid.NewGuid(),
                TimestampUtc = DateTime.UtcNow.AddSeconds(1),
                DeviceId = "test-device",
                UserId = "test-user",
                EventType = EventType.AgentError,
                Source = "EventRepositoryTests",
                Data = "{}",
                DeliveryStatus = DeliveryStatus.Failed
            };

            repository.Save(localEvent);
            repository.Save(failedEvent);

            var result = repository.GetByStatus(DeliveryStatus.Local);

            var storedEvent = Assert.Single(result);
            Assert.Equal(localEvent.EventId, storedEvent.EventId);
            Assert.Equal(DeliveryStatus.Local, storedEvent.DeliveryStatus);
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
