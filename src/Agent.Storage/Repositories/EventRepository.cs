using Agent.Core.Models;
using Agent.Storage.Data;
using Microsoft.Data.Sqlite;

namespace Agent.Storage.Repositories;

public class EventRepository
{
    private readonly Database _database;
    public EventRepository(Database database)
    {
        _database = database;
    }
    public void Save(AgentEvent agentEvent)
    {
        using var connection = _database.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO events
            (
                event_id,
                timestamp_utc,
                device_id,
                user_id,
                event_type,
                source,
                data
            )
            VALUES
            (
                $event_id,
                $timestamp_utc,
                $device_id,
                $user_id,
                $event_type,
                $source,
                $data
            );
            """;
        command.Parameters.AddWithValue(
            "$event_id",
            agentEvent.EventId.ToString());
        command.Parameters.AddWithValue(
            "$timestamp_utc",
            agentEvent.TimestampUtc.ToString("O"));
        command.Parameters.AddWithValue(
            "$device_id",
            agentEvent.DeviceId);
        command.Parameters.AddWithValue(
            "$user_id",
            agentEvent.UserId);
        command.Parameters.AddWithValue(
            "$event_type",
            agentEvent.EventType.ToString());
        command.Parameters.AddWithValue(
            "$source",
            agentEvent.Source);
        command.Parameters.AddWithValue(
            "$data",
            agentEvent.Data);
        command.ExecuteNonQuery();
    }
    public List<AgentEvent> GetAll()
{
    var events = new List<AgentEvent>();
    using var connection = _database.CreateConnection();
    connection.Open();
    using var command = connection.CreateCommand();
    command.CommandText = """
        SELECT
            event_id,
            timestamp_utc,
            device_id,
            user_id,
            event_type,
            source,
            data
        FROM events
        ORDER BY timestamp_utc;
        """;
    using var reader = command.ExecuteReader();
    while (reader.Read())
    {
        events.Add(new AgentEvent
        {
            EventId = Guid.Parse(reader.GetString(0)),
            TimestampUtc = DateTime.Parse(reader.GetString(1)),
            DeviceId = reader.GetString(2),
            UserId = reader.GetString(3),
            EventType = Enum.Parse<Agent.Core.Enums.EventType>(
                reader.GetString(4)),
            Source = reader.GetString(5),
            Data = reader.GetString(6)
        });
    }

    return events;
}
}