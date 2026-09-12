namespace Agent.Core.Models;

public class HealthStatus
{
    public string Status { get; set; } = "RUNNING";

    public string DeviceId { get; set; } = string.Empty;

    public int TotalEvents { get; set; }

    public int ReadyEvents { get; set; }

    public DateTime LastCollectionUtc { get; set; }

    public int CollectorCount { get; set; }

    public bool DatabaseHealthy { get; set; }
}