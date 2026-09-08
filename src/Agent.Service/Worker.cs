using Agent.Collectors;
using Agent.Core.Configuration;
using Microsoft.Extensions.Options;

namespace Agent.Service;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly AgentConfiguration _configuration;
    private readonly IEnumerable<ICollector> _collectors;

    public Worker(
        ILogger<Worker> logger,
        IOptions<AgentConfiguration> configuration,
        IEnumerable<ICollector> collectors)
    {
        _logger = logger;
        _configuration = configuration.Value;
        _collectors = collectors;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Agent configuration loaded. Database: {DatabasePath}, Collection interval: {Interval} seconds",
            _configuration.DatabasePath,
            _configuration.CollectionIntervalSeconds);

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
                        _logger.LogInformation(
                            "Collected event: {EventType} from {Source}",
                            agentEvent.EventType,
                            agentEvent.Source);
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