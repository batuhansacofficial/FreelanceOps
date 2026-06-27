namespace FreelanceOps.Application.Reports.GetProjectPerformance;

public sealed record GetProjectPerformanceQuery(
    Guid WorkspaceId,
    DateOnly? From,
    DateOnly? To);
