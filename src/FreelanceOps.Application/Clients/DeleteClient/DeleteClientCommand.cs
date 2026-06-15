namespace FreelanceOps.Application.Clients.DeleteClient;

public sealed record DeleteClientCommand(
    Guid WorkspaceId,
    Guid ClientId);
