using Agent.Core.Models;
using Agent.Storage.Repositories;

namespace Agent.Diagnostics;

public class CollectorHealthService
{
    private readonly CollectorHealthRepository _repository;

    public CollectorHealthService(CollectorHealthRepository repository)
    {
        _repository = repository;
    }

    public void MarkSuccess(string collectorName)
    {
        _repository.Upsert(new CollectorHealth
        {
            Name = collectorName,
            Enabled = true,
            Status = "RUNNING",
            LastSuccessUtc = DateTime.UtcNow
        });
    }

    public void MarkFailure(string collectorName)
    {
        var existing = _repository.GetAll()
            .FirstOrDefault(x => x.Name == collectorName);

        _repository.Upsert(new CollectorHealth
        {
            Name = collectorName,
            Enabled = true,
            Status = "FAILED",
            LastSuccessUtc = existing?.LastSuccessUtc
        });
    }

    public void MarkDisabled(string collectorName)
    {
        _repository.Upsert(new CollectorHealth
        {
            Name = collectorName,
            Enabled = false,
            Status = "DISABLED",
            LastSuccessUtc = null
        });
    }

    public List<CollectorHealth> GetAll()
    {
        return _repository.GetAll();
    }
}
