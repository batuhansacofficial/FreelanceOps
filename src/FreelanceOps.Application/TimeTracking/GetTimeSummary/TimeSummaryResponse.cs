namespace FreelanceOps.Application.TimeTracking.GetTimeSummary;

public sealed record TimeSummaryResponse(
    int TotalMinutes,
    double TotalHours,
    int EntriesCount,
    IReadOnlyCollection<ProjectTimeSummaryResponse> ByProject,
    IReadOnlyCollection<UserTimeSummaryResponse> ByUser);

public sealed record ProjectTimeSummaryResponse(
    Guid ProjectId,
    string ProjectName,
    int TotalMinutes,
    double TotalHours);

public sealed record UserTimeSummaryResponse(
    Guid UserId,
    string FullName,
    int TotalMinutes,
    double TotalHours);
