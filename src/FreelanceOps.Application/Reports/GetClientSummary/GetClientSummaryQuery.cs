namespace FreelanceOps.Application.Reports.GetClientSummary;

public sealed record GetClientSummaryQuery(
    Guid WorkspaceId,
    DateOnly? From,
    DateOnly? To);
