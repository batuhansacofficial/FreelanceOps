namespace FreelanceOps.Application.TimeTracking.StopTimer;

public sealed record StopTimerCommand(
    Guid WorkspaceId,
    Guid TimeEntryId);
