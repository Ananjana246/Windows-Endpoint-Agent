// using Agent.Core.Configuration;
// using Agent.Service;

// var builder = Host.CreateApplicationBuilder(args);

// builder.Services.Configure<AgentConfiguration>(
//     builder.Configuration.GetSection("Agent"));

// builder.Services.AddHostedService<Worker>();

// var host = builder.Build();
// host.Run();

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/", () => "Windows Endpoint Agent Dashboard");

app.MapGet("/health", () => new
{
    status = "Healthy",
    timestamp = DateTimeOffset.Now
});

app.Run();