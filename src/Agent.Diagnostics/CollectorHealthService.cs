using Agent.Core.Models;

namespace Agent.Diagnostics;

public class CollectorHealthService
{
    private readonly Dictionary<string, CollectorHealth> _health = new();

    public void MarkSuccess(string collectorName)
    {
        _health[collectorName] = new CollectorHealth
        {
            Name = collectorName,
            Enabled = true,
            Status = "RUNNING",
            LastSuccessUtc = DateTime.UtcNow
        };
    }

    public void MarkFailure(string collectorName)
    {
        if (!_health.TryGetValue(collectorName, out var health))
        {
            health = new CollectorHealth
            {
                Name = collectorName,
                Enabled = true
            };
        }

        health.Status = "FAILED";
        _health[collectorName] = health;
    }

    public void MarkDisabled(string collectorName)
    {
        _health[collectorName] = new CollectorHealth
        {
            Name = collectorName,
            Enabled = false,
            Status = "DISABLED",
            LastSuccessUtc = null
        };
    }

    public List<CollectorHealth> GetAll()
    {
        return _health.Values
            .OrderBy(x => x.Name)
            .ToList();
    }
}
