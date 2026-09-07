using Agent.Collectors;
using Agent.Core.Configuration;
using Microsoft.Extensions.Options;

namespace Agent.Service;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly AgentConfiguration _configuration;
    private readonly ICollector _collector;

    public Worker(
        ILogger<Worker> logger,
        IOptions<AgentConfiguration> configuration,
        ICollector collector)
    {
        _logger = logger;
        _configuration = configuration.Value;
        _collector = collector;
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

            var events = await _collector.CollectAsync(stoppingToken);

            foreach (var agentEvent in events)
            {
                _logger.LogInformation(
                    "Collected event: {EventType} from {Source}",
                    agentEvent.EventType,
                    agentEvent.Source);
            }

            await Task.Delay(
                TimeSpan.FromSeconds(_configuration.CollectionIntervalSeconds),
                stoppingToken);
        }
    }
}