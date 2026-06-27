using FreelanceOps.Domain.Common;

namespace FreelanceOps.Domain.TimeTracking;

public sealed class TimeEntry
{
    private TimeEntry()
    {
    }

    private TimeEntry(
        Guid workspaceId,
        Guid projectId,
        Guid taskId,
        Guid userId,
        DateTime startedAtUtc,
        DateTime? endedAtUtc,
        int? durationMinutes,
        string? description,
        TimeEntrySource source)
    {
        Id = Guid.NewGuid();
        WorkspaceId = workspaceId;
        ProjectId = projectId;
        TaskId = taskId;
        UserId = userId;
        StartedAtUtc = startedAtUtc;
        EndedAtUtc = endedAtUtc;
        DurationMinutes = durationMinutes;
        Description = NormalizeOptional(description);
        Source = source;
        CreatedAtUtc = DateTime.UtcNow;
        IsDeleted = false;
    }

    public Guid Id { get; private set; }
    public Guid WorkspaceId { get; private set; }
    public Guid ProjectId { get; private set; }
    public Guid TaskId { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime StartedAtUtc { get; private set; }
    public DateTime? EndedAtUtc { get; private set; }
    public int? DurationMinutes { get; private set; }
    public string? Description { get; private set; }
    public TimeEntrySource Source { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }
    public bool IsDeleted { get; private set; }

    public bool IsRunning => EndedAtUtc is null && DurationMinutes is null;

    public static TimeEntry StartTimer(
        Guid workspaceId,
        Guid projectId,
        Guid taskId,
        Guid userId,
        DateTime startedAtUtc,
        string? description)
    {
        return new TimeEntry(
            workspaceId,
            projectId,
            taskId,
            userId,
            startedAtUtc,
            endedAtUtc: null,
            durationMinutes: null,
            description,
            TimeEntrySource.Timer);
    }

    public static TimeEntry CreateManual(
        Guid workspaceId,
        Guid projectId,
        Guid taskId,
        Guid userId,
        DateTime startedAtUtc,
        int durationMinutes,
        string? description)
    {
        if (durationMinutes <= 0)
        {
            throw new DomainException("Duration must be greater than zero.");
        }

        return new TimeEntry(
            workspaceId,
            projectId,
            taskId,
            userId,
            startedAtUtc,
            endedAtUtc: startedAtUtc.AddMinutes(durationMinutes),
            durationMinutes,
            description,
            TimeEntrySource.Manual);
    }

    public void Stop(DateTime endedAtUtc)
    {
        if (!IsRunning)
        {
            throw new DomainException("Time entry is not running.");
        }

        if (endedAtUtc <= StartedAtUtc)
        {
            throw new DomainException("End time must be after start time.");
        }

        EndedAtUtc = endedAtUtc;
        DurationMinutes = (int)Math.Ceiling((endedAtUtc - StartedAtUtc).TotalMinutes);
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void UpdateManualEntry(
        DateTime startedAtUtc,
        int durationMinutes,
        string? description)
    {
        if (Source != TimeEntrySource.Manual)
        {
            throw new DomainException("Only manual time entries can be edited.");
        }

        if (durationMinutes <= 0)
        {
            throw new DomainException("Duration must be greater than zero.");
        }

        StartedAtUtc = startedAtUtc;
        EndedAtUtc = startedAtUtc.AddMinutes(durationMinutes);
        DurationMinutes = durationMinutes;
        Description = NormalizeOptional(description);
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void SoftDelete()
    {
        if (IsDeleted)
        {
            return;
        }

        IsDeleted = true;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}
