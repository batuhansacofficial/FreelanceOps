using FreelanceOps.Domain.TimeTracking;

namespace FreelanceOps.Application.TimeTracking.StartTimer;

public sealed record StartTimerResponse(
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
    bool IsRunning);
