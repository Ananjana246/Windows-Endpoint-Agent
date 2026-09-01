using Agent.Core.Configuration;
using Agent.Service;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<AgentConfiguration>(
    builder.Configuration.GetSection("Agent"));

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
