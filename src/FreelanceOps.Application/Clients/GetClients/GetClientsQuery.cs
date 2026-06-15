namespace FreelanceOps.Application.Clients.GetClients;

public sealed record GetClientsQuery(
    Guid WorkspaceId,
    int Page = 1,
    int PageSize = 20,
    string? Search = null);
