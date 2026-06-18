namespace FreelanceOps.Application.TimeTracking.StartTimer;

public sealed record StartTimerCommand(
    Guid WorkspaceId,
    Guid TaskId,
    string? Description);
