using System.Runtime.InteropServices;
using System.Text.Json;
using Agent.Core.Enums;
using Agent.Core.Models;

namespace Agent.Collectors;

public class SystemInfoCollector : ICollector
{
    public Task<IEnumerable<AgentEvent>> CollectAsync(
        CancellationToken cancellationToken = default)
    {
        var systemInfo = new
        {
            MachineName = Environment.MachineName,
            OperatingSystem = Environment.OSVersion.Platform.ToString(),
            OSVersion = Environment.OSVersion.VersionString,
            Architecture = RuntimeInformation.OSArchitecture.ToString()
        };

        var agentEvent = new AgentEvent
        {
            EventId = Guid.NewGuid(),
            TimestampUtc = DateTime.UtcNow,
            DeviceId = Environment.MachineName,
            UserId = Environment.UserName,
            EventType = EventType.SystemInfo,
            Source = "SystemInfoCollector",
            Data = JsonSerializer.Serialize(systemInfo)
        };

        return Task.FromResult<IEnumerable<AgentEvent>>(
            new[] { agentEvent });
    }
}