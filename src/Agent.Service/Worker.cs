using Agent.Core.Configuration;
using Microsoft.Extensions.Options;

namespace Agent.Service;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly AgentConfiguration _configuration;

    public Worker(
        ILogger<Worker> logger,
        IOptions<AgentConfiguration> configuration)
    {
        _logger = logger;
        _configuration = configuration.Value;
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
                "Worker running at: {time}",
                DateTimeOffset.Now);

            await Task.Delay(
                TimeSpan.FromSeconds(_configuration.CollectionIntervalSeconds),
                stoppingToken);
        }
    }
}