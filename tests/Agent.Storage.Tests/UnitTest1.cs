using Agent.Core.Enums;
using Agent.Core.Models;
using Agent.Storage.Data;
using Agent.Storage.Repositories;

namespace Agent.Storage.Tests;
public class EventRepositoryTests
{
    [Fact]
    public void SaveAndGetAll_ShouldPersistEvent()
    {
        var databasePath = Path.Combine(
            Path.GetTempPath(),
            $"agent-test-{Guid.NewGuid()}.db");

        var database = new Database(databasePath);
        var initializer = new DatabaseInitializer(database);
        initializer.Initialize();
        var repository = new EventRepository(database);

        var originalEvent = new AgentEvent
        {
            EventId = Guid.NewGuid(),
            TimestampUtc = DateTime.UtcNow,
            DeviceId = "TEST-DEVICE",
            UserId = "TEST-USER",
            EventType = EventType.ProcessStarted,
            Source = "ProcessCollector",
            Data = """{"processName":"chrome.exe","pid":5420}"""
        };

        repository.Save(originalEvent);
        var events = repository.GetAll();
        Assert.Single(events);
        var savedEvent = events[0];
        Assert.Equal(originalEvent.EventId, savedEvent.EventId);
        Assert.Equal(originalEvent.DeviceId, savedEvent.DeviceId);
        Assert.Equal(originalEvent.UserId, savedEvent.UserId);
        Assert.Equal(originalEvent.EventType, savedEvent.EventType);
        Assert.Equal(originalEvent.Source, savedEvent.Source);
        Assert.Equal(originalEvent.Data, savedEvent.Data);

        File.Delete(databasePath);
    }
}