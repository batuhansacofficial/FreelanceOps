namespace FreelanceOps.Application.Clients.GetClientById;

public sealed record ClientDetailResponse(
    Guid Id,
    Guid WorkspaceId,
    string Name,
    string? Email,
    string? CompanyName,
    string? Notes,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);
