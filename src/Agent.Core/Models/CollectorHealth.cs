namespace Agent.Core.Models;

public class CollectorHealth
{
    public string Name { get; set; } = string.Empty;

    public bool Enabled { get; set; }

    public string Status { get; set; } = "UNKNOWN";

    public DateTime? LastSuccessUtc { get; set; }
}
