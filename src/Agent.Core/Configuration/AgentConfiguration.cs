namespace Agent.Core.Configuration;

public class AgentConfiguration
{
    public string DatabasePath { get; set; } = "agent.db";

    public int CollectionIntervalSeconds { get; set; } = 30;

    public string LogLevel { get; set; } = "Information";
}