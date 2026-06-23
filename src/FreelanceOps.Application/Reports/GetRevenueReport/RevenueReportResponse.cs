namespace FreelanceOps.Application.Reports.GetRevenueReport;

public sealed record RevenueReportResponse(
    DateOnly From,
    DateOnly To,
    string GroupBy,
    IReadOnlyCollection<RevenueByCurrencyResponse> ItemsByCurrency);

public sealed record RevenueByCurrencyResponse(
    string Currency,
    decimal TotalPaidRevenue,
    IReadOnlyCollection<RevenuePeriodResponse> Items);

public sealed record RevenuePeriodResponse(
    string Period,
    decimal Amount);
