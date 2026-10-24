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
    [Fact]
    public void GetByStatus_ShouldReturnEmptyListWhenNoEventsMatch()
    {
        var databasePath = Path.Combine(
            Path.GetTempPath(),
            $"event-empty-status-{Guid.NewGuid()}.db");

        try
        {
            var database = new Database(databasePath);
            new DatabaseInitializer(database).Initialize();

            var repository = new EventRepository(database);

            var result = repository.GetByStatus(DeliveryStatus.Failed);

            Assert.Empty(result);
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
    public void UpdateStatus_ShouldNotModifyEventsWhenEventIdDoesNotExist()
    {
        var databasePath = Path.Combine(
            Path.GetTempPath(),
            $"event-missing-status-{Guid.NewGuid()}.db");

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
                Guid.NewGuid(),
                DeliveryStatus.Processed);

            var storedEvent = Assert.Single(repository.GetAll());

            Assert.Equal(agentEvent.EventId, storedEvent.EventId);
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
[Fact]
public void GetAll_ShouldReturnEventsInTimestampOrder()
{
    var databasePath = Path.Combine(
        Path.GetTempPath(),
        $"event-order-{Guid.NewGuid()}.db");

    try
    {
        var database = new Database(databasePath);
        new DatabaseInitializer(database).Initialize();

        var repository = new EventRepository(database);
        var baseTime = DateTime.UtcNow;

        var laterEvent = new AgentEvent
        {
            EventId = Guid.NewGuid(),
            TimestampUtc = baseTime.AddMinutes(2),
            DeviceId = "test-device",
            UserId = "test-user",
            EventType = EventType.AgentError,
            Source = "EventRepositoryTests",
            Data = "{}",
            DeliveryStatus = DeliveryStatus.Local
        };

        var earlierEvent = new AgentEvent
        {
            EventId = Guid.NewGuid(),
            TimestampUtc = baseTime,
            DeviceId = "test-device",
            UserId = "test-user",
            EventType = EventType.AgentStarted,
            Source = "EventRepositoryTests",
            Data = "{}",
            DeliveryStatus = DeliveryStatus.Local
        };

        repository.Save(laterEvent);
        repository.Save(earlierEvent);

        var result = repository.GetAll();

        Assert.Equal(2, result.Count);
        Assert.Equal(earlierEvent.EventId, result[0].EventId);
        Assert.Equal(laterEvent.EventId, result[1].EventId);
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
