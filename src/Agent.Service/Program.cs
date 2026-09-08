using Agent.Collectors;
using Agent.Service;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "Windows Endpoint Agent";
});

// Register the system information collector
builder.Services.AddSingleton<ICollector, SystemInfoCollector>();
builder.Services.AddSingleton<ICollector, ProcessCollector>();
// Register the background worker
builder.Services.AddHostedService<Worker>();

var host = builder.Build();

host.Run();