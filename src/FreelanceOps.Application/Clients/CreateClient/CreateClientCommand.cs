namespace FreelanceOps.Application.Clients.CreateClient;

public sealed record CreateClientCommand(
    Guid WorkspaceId,
    string Name,
    string? Email,
    string? CompanyName,
    string? Notes);
