using System.Text.Json;
using Agent.Core.Enums;
using Agent.Core.Models;

namespace Agent.Collectors;

public class FileCollector : ICollector
{
    public Task<IEnumerable<AgentEvent>> CollectAsync(
        CancellationToken cancellationToken = default)
    {
        var events = new List<AgentEvent>();

        var folders = new[]
        {
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
        };

        foreach (var folder in folders.Distinct())
        {
            if (string.IsNullOrWhiteSpace(folder) ||
                !Directory.Exists(folder))
            {
                continue;
            }

            try
            {
                foreach (var file in Directory.EnumerateFiles(
                    folder,
                    "*",
                    SearchOption.TopDirectoryOnly))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var fileInfo = new FileInfo(file);

                    var fileData = new
                    {
                        FileName = fileInfo.Name,
                        FilePath = fileInfo.FullName,
                        Extension = fileInfo.Extension,
                        Size = fileInfo.Length,
                        LastModifiedUtc = fileInfo.LastWriteTimeUtc
                    };

                    var agentEvent = new AgentEvent
                    {
                        EventId = Guid.NewGuid(),
                        TimestampUtc = DateTime.UtcNow,
                        DeviceId = Environment.MachineName,
                        UserId = Environment.UserName,
                        EventType = EventType.FileModified,
                        Source = "FileCollector",
                        Data = JsonSerializer.Serialize(fileData)
                    };

                    events.Add(agentEvent);
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Skip folders/files that cannot be accessed.
            }
            catch (IOException)
            {
                // Skip files that become unavailable during collection.
            }
        }

        return Task.FromResult<IEnumerable<AgentEvent>>(events);
    }
}