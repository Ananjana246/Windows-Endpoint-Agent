using Agent.Core.Models;
using Agent.Core.Services;
using Agent.Storage.Repositories;

namespace Agent.Diagnostics;

public class HealthService
{
    private readonly DeviceIdentityService _deviceIdentityService;
    private readonly EventRepository _eventRepository;
    private readonly DiagnosticsRepository _diagnosticsRepository;

    public HealthService(
        DeviceIdentityService deviceIdentityService,
        EventRepository eventRepository,
        DiagnosticsRepository diagnosticsRepository)
    {
        _deviceIdentityService = deviceIdentityService;
        _eventRepository = eventRepository;
        _diagnosticsRepository = diagnosticsRepository;
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

        return new HealthStatus
        {
            Status = "RUNNING",
            DeviceId = deviceId,
            TotalEvents = allEvents.Count,
            ReadyEvents = readyEvents.Count,
            LastCollectionUtc =
                lastCollection ?? DateTime.MinValue,
            CollectorCount = 4,
            DatabaseHealthy = true
        };
    }
}