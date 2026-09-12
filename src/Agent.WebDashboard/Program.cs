using Agent.Core.Configuration;
using Agent.Core.Services;
using Agent.Diagnostics;
using Agent.Storage.Data;
using Agent.Storage.Repositories;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AgentConfiguration>(options =>
{
    options.DatabasePath = Path.GetFullPath(
        Path.Combine(
            builder.Environment.ContentRootPath,
            "../Agent.Service/agent.db"));
});

builder.Services.AddSingleton<DeviceIdentityService>(sp =>
{
    var identityPath = Path.GetFullPath(
        Path.Combine(
            builder.Environment.ContentRootPath,
            "../Agent.Service/bin/Debug/net8.0/device.id"));

    return new DeviceIdentityService(identityPath);
});

builder.Services.AddSingleton<Database>(sp =>
{
    var configuration = sp.GetRequiredService<
        IOptions<AgentConfiguration>>();

    return new Database(configuration.Value.DatabasePath);
});

builder.Services.AddSingleton<EventRepository>();
builder.Services.AddSingleton<DiagnosticsRepository>();

builder.Services.AddSingleton<HealthService>();

var app = builder.Build();

app.MapGet("/", () => "Windows Endpoint Agent Dashboard");

app.MapGet("/health", (HealthService healthService) =>
{
    var health = healthService.GetHealth();

    return Results.Ok(health);
});

app.Run();