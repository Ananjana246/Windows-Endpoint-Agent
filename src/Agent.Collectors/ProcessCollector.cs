using System.Diagnostics;
using System.Text.Json;
using Agent.Core.Enums;
using Agent.Core.Models;

namespace Agent.Collectors;

public class ProcessCollector : ICollector
{
    public Task<IEnumerable<AgentEvent>> CollectAsync(
        CancellationToken cancellationToken = default)
    {
        var events = new List<AgentEvent>();

        foreach (var process in Process.GetProcesses())
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var processInfo = new
                {
                    ProcessId = process.Id,
                    ProcessName = process.ProcessName
                };

                var agentEvent = new AgentEvent
                {
                    EventId = Guid.NewGuid(),
                    TimestampUtc = DateTime.UtcNow,
                    DeviceId = Environment.MachineName,
                    UserId = Environment.UserName,
                    EventType = EventType.ProcessStarted,
                    Source = "ProcessCollector",
                    Data = JsonSerializer.Serialize(processInfo)
                };

                events.Add(agentEvent);
            }
            catch
            {
                // Some system processes may not be accessible.
                // Skip them and continue collecting other processes.
            }
            finally
            {
                process.Dispose();
            }
        }

        return Task.FromResult<IEnumerable<AgentEvent>>(events);
    }
}
