namespace FreelanceOps.Application.TimeTracking.CreateManualTimeEntry;

public sealed record CreateManualTimeEntryCommand(
    Guid WorkspaceId,
    Guid TaskId,
    DateTime StartedAtUtc,
    int DurationMinutes,
    string? Description);
