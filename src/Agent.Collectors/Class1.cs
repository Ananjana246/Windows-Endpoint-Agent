using Agent.Core.Models;

namespace Agent.Collectors;

public interface ICollector
{
    Task<IEnumerable<AgentEvent>> CollectAsync(
        CancellationToken cancellationToken = default);
}

