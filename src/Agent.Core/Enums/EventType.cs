namespace Agent.Core.Enums;

public enum EventType
{
    AgentStarted,
    AgentStopped,
    AgentError,
    UserLogin,
    UserLogout,
    WorkstationLock,
    WorkstationUnlock,
    ProcessStarted,
    ProcessStopped,
    FileCreated,
    FileModified,
    FileRenamed,
    FileDeleted
}