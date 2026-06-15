namespace FreelanceOps.Application.Clients.CreateClient;

public sealed record CreateClientResponse(
    Guid Id,
    Guid WorkspaceId,
    string Name,
    string? Email,
    string? CompanyName,
    string? Notes,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);
