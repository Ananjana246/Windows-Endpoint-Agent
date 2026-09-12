using System.Text.Json;
using Agent.Core.Enums;
using Agent.Core.Models;

namespace Agent.Collectors;

public class FileCollector : ICollector
{
    private readonly Dictionary<string, FileState> _previousFiles = new();
    private bool _hasInitialSnapshot;

    public Task<IEnumerable<AgentEvent>> CollectAsync(
        CancellationToken cancellationToken = default)
    {
        var events = new List<AgentEvent>();
        var currentFiles = new Dictionary<string, FileState>();

        var folders = new[]
        {
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
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

                    try
                    {
                        var fileInfo = new FileInfo(file);

                        var state = new FileState(
                            fileInfo.Length,
                            fileInfo.LastWriteTimeUtc);

                        currentFiles[fileInfo.FullName] = state;

                        if (!_previousFiles.ContainsKey(fileInfo.FullName))
                        {
                            events.Add(CreateEvent(
                                EventType.FileCreated,
                                fileInfo));
                        }
                        else if (_previousFiles[fileInfo.FullName] != state)
                        {
                            events.Add(CreateEvent(
                                EventType.FileModified,
                                fileInfo));
                        }
                    }
                    catch (UnauthorizedAccessException)
                    {
                    }
                    catch (IOException)
                    {
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
            }
            catch (IOException)
            {
            }
        }

        // First scan establishes the baseline.
        // Existing files should not be reported as newly created.
        if (!_hasInitialSnapshot)
        {
            _previousFiles.Clear();

            foreach (var file in currentFiles)
            {
                _previousFiles[file.Key] = file.Value;
            }

            _hasInitialSnapshot = true;

            return Task.FromResult<IEnumerable<AgentEvent>>(new List<AgentEvent>());
        }

        foreach (var previousFile in _previousFiles)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!currentFiles.ContainsKey(previousFile.Key))
            {
                var fileData = new
                {
                    FilePath = previousFile.Key
                };

                events.Add(new AgentEvent
                {
                    EventId = Guid.NewGuid(),
                    TimestampUtc = DateTime.UtcNow,
                    DeviceId = Environment.MachineName,
                    UserId = Environment.UserName,
                    EventType = EventType.FileDeleted,
                    Source = "FileCollector",
                    Data = JsonSerializer.Serialize(fileData)
                });
            }
        }

        _previousFiles.Clear();

        foreach (var file in currentFiles)
        {
            _previousFiles[file.Key] = file.Value;
        }

        return Task.FromResult<IEnumerable<AgentEvent>>(events);
    }

    private static AgentEvent CreateEvent(
        EventType eventType,
        FileInfo fileInfo)
    {
        var fileData = new
        {
            FileName = fileInfo.Name,
            FilePath = fileInfo.FullName,
            Extension = fileInfo.Extension,
            Size = fileInfo.Length,
            LastModifiedUtc = fileInfo.LastWriteTimeUtc
        };

        return new AgentEvent
        {
            EventId = Guid.NewGuid(),
            TimestampUtc = DateTime.UtcNow,
            DeviceId = Environment.MachineName,
            UserId = Environment.UserName,
            EventType = eventType,
            Source = "FileCollector",
            Data = JsonSerializer.Serialize(fileData)
        };
    }

    private readonly record struct FileState(
        long Size,
        DateTime LastModifiedUtc);
}