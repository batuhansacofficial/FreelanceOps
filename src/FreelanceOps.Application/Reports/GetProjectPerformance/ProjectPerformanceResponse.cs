using FreelanceOps.Domain.Projects;

namespace FreelanceOps.Application.Reports.GetProjectPerformance;

public sealed record ProjectPerformanceResponse(
    DateOnly From,
    DateOnly To,
    IReadOnlyCollection<ProjectPerformanceItemResponse> Items);

public sealed record ProjectPerformanceItemResponse(
    Guid ProjectId,
    string ProjectName,
    Guid ClientId,
    string ClientName,
    ProjectStatus Status,
    int TrackedMinutes,
    double TrackedHours,
    decimal InvoiceTotal,
    decimal PaidAmount,
    decimal OutstandingAmount,
    decimal RevenuePerTrackedHour);
