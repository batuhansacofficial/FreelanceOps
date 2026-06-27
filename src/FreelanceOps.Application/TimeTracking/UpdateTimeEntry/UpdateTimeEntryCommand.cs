namespace FreelanceOps.Application.TimeTracking.UpdateTimeEntry;

public sealed record UpdateTimeEntryCommand(
    Guid WorkspaceId,
    Guid TimeEntryId,
    DateTime StartedAtUtc,
    int DurationMinutes,
    string? Description);
