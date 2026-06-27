namespace FreelanceOps.Application.Clients.GetClients;

public sealed record ClientSummaryResponse(
    Guid Id,
    Guid WorkspaceId,
    string Name,
    string? Email,
    string? CompanyName,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);
