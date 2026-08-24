using Agent.Core.Enums;
namespace Agent.Core.Models;

public class AgentEvent
{
    public Guid EventId { get; set; }
    public DateTime TimestampUtc { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public EventType EventType { get; set; }
    public string Source { get; set; } = string.Empty;
    public string Data { get; set; } = string.Empty;
}