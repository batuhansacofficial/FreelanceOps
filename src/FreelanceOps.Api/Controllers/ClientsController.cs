using FreelanceOps.Application.Clients.CreateClient;
using FreelanceOps.Application.Clients.DeleteClient;
using FreelanceOps.Application.Clients.GetClientById;
using FreelanceOps.Application.Clients.GetClients;
using FreelanceOps.Application.Clients.UpdateClient;
using FreelanceOps.Application.Common.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FreelanceOps.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/workspaces/{workspaceId:guid}/clients")]
public sealed class ClientsController(
    CreateClientHandler createClientHandler,
    GetClientsHandler getClientsHandler,
    GetClientByIdHandler getClientByIdHandler,
    UpdateClientHandler updateClientHandler,
    DeleteClientHandler deleteClientHandler) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<CreateClientResponse>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(
        Guid workspaceId,
        CreateClientRequest request,
        CancellationToken cancellationToken)
    {
        var response = await createClientHandler.Handle(
            new CreateClientCommand(
                workspaceId,
                request.Name,
                request.Email,
                request.CompanyName,
                request.Notes),
            cancellationToken);

        return Created($"/api/workspaces/{workspaceId}/clients/{response.Id}", response);
    }

    [HttpGet]
    [ProducesResponseType<PagedResult<ClientSummaryResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(
        Guid workspaceId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var response = await getClientsHandler.Handle(
            new GetClientsQuery(workspaceId, page, pageSize, search),
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("{clientId:guid}")]
    [ProducesResponseType<ClientDetailResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(
        Guid workspaceId,
        Guid clientId,
        CancellationToken cancellationToken)
    {
        var response = await getClientByIdHandler.Handle(
            new GetClientByIdQuery(workspaceId, clientId),
            cancellationToken);

        return Ok(response);
    }

    [HttpPut("{clientId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Update(
        Guid workspaceId,
        Guid clientId,
        UpdateClientRequest request,
        CancellationToken cancellationToken)
    {
        await updateClientHandler.Handle(
            new UpdateClientCommand(
                workspaceId,
                clientId,
                request.Name,
                request.Email,
                request.CompanyName,
                request.Notes),
            cancellationToken);

        return NoContent();
    }

    [HttpDelete("{clientId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(
        Guid workspaceId,
        Guid clientId,
        CancellationToken cancellationToken)
    {
        await deleteClientHandler.Handle(
            new DeleteClientCommand(workspaceId, clientId),
            cancellationToken);

        return NoContent();
    }
}

public sealed record CreateClientRequest(
    string Name,
    string? Email,
    string? CompanyName,
    string? Notes);

public sealed record UpdateClientRequest(
    string Name,
    string? Email,
    string? CompanyName,
    string? Notes);
