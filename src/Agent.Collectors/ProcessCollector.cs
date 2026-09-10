using System.Diagnostics;
using System.Text.Json;
using Agent.Core.Enums;
using Agent.Core.Models;

namespace Agent.Collectors;

public class ProcessCollector : ICollector
{
    private readonly Dictionary<int, ProcessState> _previousProcesses = new();

    public Task<IEnumerable<AgentEvent>> CollectAsync(
        CancellationToken cancellationToken = default)
    {
        var events = new List<AgentEvent>();
        var currentProcesses = new Dictionary<int, ProcessState>();

        foreach (var process in Process.GetProcesses())
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var processState = GetProcessState(process);

                currentProcesses[process.Id] = processState;

                // New process detected
                if (!_previousProcesses.ContainsKey(process.Id))
                {
                    events.Add(CreateProcessEvent(
                        EventType.ProcessStarted,
                        processState));
                }
            }
            catch
            {
                // Some system processes may not be accessible.
                // Skip them and continue.
            }
            finally
            {
                process.Dispose();
            }
        }

        // Processes that disappeared
        foreach (var previousProcess in _previousProcesses)
        {
            if (!currentProcesses.ContainsKey(previousProcess.Key))
            {
                events.Add(CreateProcessEvent(
                    EventType.ProcessStopped,
                    previousProcess.Value));
            }
        }

        _previousProcesses.Clear();

        foreach (var process in currentProcesses)
        {
            _previousProcesses[process.Key] = process.Value;
        }

        return Task.FromResult<IEnumerable<AgentEvent>>(events);
    }

    private static ProcessState GetProcessState(Process process)
    {
        string? processPath = null;
        DateTime? processStartTimeUtc = null;

        try
        {
            processPath = process.MainModule?.FileName;
        }
        catch
        {
            // Access to some processes may be denied.
        }

        try
        {
            processStartTimeUtc =
                process.StartTime.ToUniversalTime();
        }
        catch
        {
            // Start time may not be available.
        }

        return new ProcessState(
            process.Id,
            process.ProcessName,
            processPath,
            processStartTimeUtc);
    }

    private static AgentEvent CreateProcessEvent(
        EventType eventType,
        ProcessState process)
    {
        var processData = new
        {
            ProcessId = process.ProcessId,
            ProcessName = process.ProcessName,
            ProcessPath = process.ProcessPath,
            ProcessStartTimeUtc = process.ProcessStartTimeUtc,
            UserId = Environment.UserName,
            TimestampUtc = DateTime.UtcNow
        };

        return new AgentEvent
        {
            EventId = Guid.NewGuid(),
            TimestampUtc = DateTime.UtcNow,
            DeviceId = Environment.MachineName,
            UserId = Environment.UserName,
            EventType = eventType,
            Source = "ProcessCollector",
            Data = JsonSerializer.Serialize(processData)
        };
    }

    private readonly record struct ProcessState(
        int ProcessId,
        string ProcessName,
        string? ProcessPath,
        DateTime? ProcessStartTimeUtc);
}