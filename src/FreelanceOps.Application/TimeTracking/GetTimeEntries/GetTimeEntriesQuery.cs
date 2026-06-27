namespace FreelanceOps.Application.TimeTracking.GetTimeEntries;

public sealed record GetTimeEntriesQuery(
    Guid WorkspaceId,
    int Page = 1,
    int PageSize = 20,
    Guid? UserId = null,
    Guid? ProjectId = null,
    Guid? TaskId = null,
    DateOnly? From = null,
    DateOnly? To = null);
