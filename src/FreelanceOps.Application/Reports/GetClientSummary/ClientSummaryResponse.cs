namespace FreelanceOps.Application.Reports.GetClientSummary;

public sealed record ClientSummaryResponse(
    DateOnly From,
    DateOnly To,
    IReadOnlyCollection<ClientSummaryItemResponse> Items);

public sealed record ClientSummaryItemResponse(
    Guid ClientId,
    string ClientName,
    int ProjectCount,
    int InvoiceCount,
    decimal PaidAmount,
    decimal OutstandingAmount,
    int TrackedMinutes,
    double TrackedHours);
