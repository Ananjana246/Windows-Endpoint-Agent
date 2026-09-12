using Agent.Core.Enums;
using Agent.Core.Services;

namespace Agent.Service;

public class EventQueueProcessor
{
    private readonly IEventQueue _eventQueue;
    private readonly ILogger<EventQueueProcessor> _logger;

    public EventQueueProcessor(
        IEventQueue eventQueue,
        ILogger<EventQueueProcessor> logger)
    {
        _eventQueue = eventQueue;
        _logger = logger;
    }

    public void PrepareLocalEvents()
    {
        var localEvents =
            _eventQueue.GetByStatus(DeliveryStatus.Local);

        foreach (var agentEvent in localEvents)
        {
            try
            {
                _eventQueue.UpdateStatus(
                    agentEvent.EventId,
                    DeliveryStatus.Ready);

                _logger.LogInformation(
                    "Event {EventId} moved from LOCAL to READY",
                    agentEvent.EventId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to prepare event {EventId}",
                    agentEvent.EventId);

                _eventQueue.UpdateStatus(
                    agentEvent.EventId,
                    DeliveryStatus.Failed);
            }
        }
    }
}