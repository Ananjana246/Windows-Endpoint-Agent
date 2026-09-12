using Agent.Collectors;
using Agent.Core.Configuration;
using Agent.Core.Services;
using Microsoft.Extensions.Options;
using Agent.Storage.Repositories;
using Agent.Core.Enums;

namespace Agent.Service;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly AgentConfiguration _configuration;
    private readonly IEnumerable<ICollector> _collectors;
    private readonly EventNormalizer _normalizer;
    private readonly EventRepository _eventRepository;
    private readonly EventQueueProcessor _queueProcessor;private readonly DeviceIdentityService _deviceIdentityService;
    

    private readonly Dictionary<string, DateTime> _seenEvents = new();

    public Worker(
        ILogger<Worker> logger,
        IOptions<AgentConfiguration> configuration,
        IEnumerable<ICollector> collectors,
        EventNormalizer normalizer,
        EventRepository eventRepository,
        EventQueueProcessor queueProcessor,
        DeviceIdentityService deviceIdentityService)
    {
        _logger = logger;
        _configuration = configuration.Value;
        _collectors = collectors;
        _normalizer = normalizer;
        _eventRepository = eventRepository;   
        _queueProcessor = queueProcessor;
        _deviceIdentityService = deviceIdentityService;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Agent configuration loaded. Database: {DatabasePath}, Collection interval: {Interval} seconds",
            _configuration.DatabasePath,
            _configuration.CollectionIntervalSeconds);
        var deviceId = _deviceIdentityService.GetDeviceId();

        _logger.LogInformation(
            "Endpoint Device ID: {DeviceId}",
            deviceId);

        _queueProcessor.PrepareLocalEvents();

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation(
                "Endpoint Agent worker running at: {time}",
                DateTimeOffset.Now);

            foreach (var collector in _collectors)
            {
                try
                {
                    var events = await collector.CollectAsync(stoppingToken);

                    foreach (var agentEvent in events)
                    {
                        var normalizedEvent =
                            _normalizer.Normalize(agentEvent);
                        normalizedEvent.DeviceId = deviceId;

                        var deduplicationKey =
                            _normalizer.CreateDeduplicationKey(
                                normalizedEvent);

                        var now = DateTime.UtcNow;

                        if (_seenEvents.TryGetValue(
                                deduplicationKey,
                                out var previousTime))
                        {
                            if ((now - previousTime).TotalSeconds < 60)
                            {
                                _logger.LogDebug(
                                    "Duplicate event ignored: {EventType} from {Source}",
                                    normalizedEvent.EventType,
                                    normalizedEvent.Source);

                                continue;
                            }
                        }

                        _seenEvents[deduplicationKey] = now;
                        _eventRepository.Save(normalizedEvent);

_eventRepository.UpdateStatus(
    normalizedEvent.EventId,
    DeliveryStatus.Ready);

_logger.LogInformation(
    "Saved event: {EventType} from {Source} with status READY",
    normalizedEvent.EventType,
    normalizedEvent.Source);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Error while running collector: {Collector}",
                        collector.GetType().Name);
                }
            }

            await Task.Delay(
                TimeSpan.FromSeconds(
                    _configuration.CollectionIntervalSeconds),
                stoppingToken);
        }
    }
}
