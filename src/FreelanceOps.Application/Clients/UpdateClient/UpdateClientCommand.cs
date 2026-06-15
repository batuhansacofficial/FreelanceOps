namespace FreelanceOps.Application.Clients.UpdateClient;

public sealed record UpdateClientCommand(
    Guid WorkspaceId,
    Guid ClientId,
    string Name,
    string? Email,
    string? CompanyName,
    string? Notes);
