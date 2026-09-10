using System.Security.Cryptography;
using System.Text;
using Agent.Core.Models;

namespace Agent.Core.Services;

public class EventNormalizer
{
    public AgentEvent Normalize(AgentEvent agentEvent)
    {
        if (agentEvent.EventId == Guid.Empty)
        {
            agentEvent.EventId = Guid.NewGuid();
        }

        agentEvent.DeviceId ??= string.Empty;
        agentEvent.UserId ??= string.Empty;
        agentEvent.Source ??= string.Empty;
        agentEvent.Data ??= string.Empty;

        agentEvent.TimestampUtc = agentEvent.TimestampUtc.ToUniversalTime();

        return agentEvent;
    }

    public string CreateDeduplicationKey(AgentEvent agentEvent)
    {
        var normalizedData = agentEvent.Data.Trim();

        var rawKey = string.Join(
            "|",
            agentEvent.DeviceId,
            agentEvent.UserId,
            agentEvent.EventType,
            agentEvent.Source,
            normalizedData);

        using var sha256 = SHA256.Create();

        var hash = sha256.ComputeHash(
            Encoding.UTF8.GetBytes(rawKey));

        return Convert.ToHexString(hash);
    }
}
