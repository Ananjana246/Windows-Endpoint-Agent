var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/", () => "Windows Endpoint Agent Dashboard");

app.MapGet("/health", () => new
{
    status = "Healthy",
    timestamp = DateTimeOffset.Now
});

app.Run();
