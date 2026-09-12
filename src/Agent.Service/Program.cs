using Agent.Collectors;
using Agent.Core.Services;
using Agent.Service;
using Agent.Storage.Data;
using Agent.Storage.Repositories;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "Windows Endpoint Agent";
});

// Register the system information collector
builder.Services.AddSingleton<ICollector, SystemInfoCollector>();
builder.Services.AddSingleton<ICollector, ProcessCollector>();
builder.Services.AddSingleton<ICollector, FileCollector>();
builder.Services.AddSingleton<EventNormalizer>();
// Register SQLite storage
builder.Services.AddSingleton<Database>(sp =>
{
    var configuration = sp.GetRequiredService<
        Microsoft.Extensions.Options.IOptions<Agent.Core.Configuration.AgentConfiguration>>();
    return new Database(configuration.Value.DatabasePath);
});
builder.Services.AddSingleton<DatabaseInitializer>();
builder.Services.AddSingleton<EventRepository>();
builder.Services.AddSingleton<IEventQueue>(
    sp => sp.GetRequiredService<EventRepository>());
builder.Services.AddSingleton<DiagnosticsRepository>();
builder.Services.AddSingleton<EventQueueProcessor>();
builder.Services.AddSingleton<Agent.Diagnostics.HealthService>();
builder.Services.AddSingleton<DeviceIdentityService>(sp =>
{
    var identityPath = Path.Combine(
        AppContext.BaseDirectory,
        "device.id");

    return new DeviceIdentityService(identityPath);
});
// Register the background worker
builder.Services.AddHostedService<Worker>();
var host = builder.Build();
// Initialize the SQLite database
var databaseInitializer =
    host.Services.GetRequiredService<DatabaseInitializer>();
databaseInitializer.Initialize();
host.Run();
