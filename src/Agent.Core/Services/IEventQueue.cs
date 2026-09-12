using Agent.Core.Enums;
using Agent.Core.Models;

namespace Agent.Core.Services;

public interface IEventQueue
{
    List<AgentEvent> GetByStatus(DeliveryStatus status);

    void UpdateStatus(
        Guid eventId,
        DeliveryStatus status);
}