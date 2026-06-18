namespace FreelanceOps.Application.TimeTracking.GetTimeSummary;

public sealed record GetTimeSummaryQuery(
    Guid WorkspaceId,
    DateOnly? From = null,
    DateOnly? To = null,
    Guid? ProjectId = null,
    Guid? TaskId = null);
