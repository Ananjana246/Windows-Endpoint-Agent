using Agent.Core.Models;
using Agent.Core.Services;
using Agent.Storage.Repositories;

namespace Agent.Diagnostics;

public class HealthService
{
    private readonly DeviceIdentityService _deviceIdentityService;
    private readonly EventRepository _eventRepository;
    private readonly DiagnosticsRepository _diagnosticsRepository;
    private readonly CollectorHealthService _collectorHealthService;
    private readonly Agent.Storage.Data.Database _database;

    public HealthService(
        DeviceIdentityService deviceIdentityService,
        EventRepository eventRepository,
        DiagnosticsRepository diagnosticsRepository,
        CollectorHealthService collectorHealthService,
        Agent.Storage.Data.Database database)
    {
        _deviceIdentityService = deviceIdentityService;
        _eventRepository = eventRepository;
        _diagnosticsRepository = diagnosticsRepository;
        _collectorHealthService = collectorHealthService;
        _database = database;
    }

    public HealthStatus GetHealth()
    {
        var deviceId = _deviceIdentityService.GetDeviceId();

        var allEvents = _eventRepository.GetAll();

        var readyEvents =
            _eventRepository.GetByStatus(
                Agent.Core.Enums.DeliveryStatus.Ready);

        var lastCollection =
            _diagnosticsRepository.GetLastCollection();

        var databaseHealthy = _database.CanConnect();
        var staleThreshold = TimeSpan.FromSeconds(60);
        var collectionHealthy = lastCollection.HasValue && DateTime.UtcNow - lastCollection.Value <= staleThreshold;

        return new HealthStatus
        {
            Status = (!databaseHealthy ? "DEGRADED" : collectionHealthy ? "RUNNING" : "STALE"),
            DeviceId = deviceId,
            TotalEvents = allEvents.Count,
            ReadyEvents = readyEvents.Count,
            LastCollectionUtc =
                lastCollection ?? DateTime.MinValue,
            CollectorCount = _collectorHealthService.GetAll().Count,
            DatabaseHealthy = databaseHealthy,
              Collectors = _collectorHealthService.GetAll()
        };
    }
}