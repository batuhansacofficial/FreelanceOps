using FreelanceOps.Domain.TimeTracking;

namespace FreelanceOps.Application.TimeTracking.GetTimeEntries;

public sealed record TimeEntryResponse(
    Guid Id,
    Guid WorkspaceId,
    Guid ProjectId,
    Guid TaskId,
    Guid UserId,
    DateTime StartedAtUtc,
    DateTime? EndedAtUtc,
    int? DurationMinutes,
    string? Description,
    TimeEntrySource Source,
    bool IsRunning,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);
