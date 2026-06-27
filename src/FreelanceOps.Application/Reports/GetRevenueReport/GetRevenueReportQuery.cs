namespace FreelanceOps.Application.Reports.GetRevenueReport;

public sealed record GetRevenueReportQuery(
    Guid WorkspaceId,
    DateOnly? From,
    DateOnly? To,
    string GroupBy);
