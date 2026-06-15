namespace FreelanceOps.Application.Clients.GetClientById;

public sealed record GetClientByIdQuery(
    Guid WorkspaceId,
    Guid ClientId);
