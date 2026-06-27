using FreelanceOps.Application.Common.Pagination;
using FreelanceOps.Application.Proposals;
using FreelanceOps.Application.Proposals.AcceptProposal;
using FreelanceOps.Application.Proposals.CancelProposal;
using FreelanceOps.Application.Proposals.ConvertProposalToProject;
using FreelanceOps.Application.Proposals.CreateProposal;
using FreelanceOps.Application.Proposals.DeleteProposal;
using FreelanceOps.Application.Proposals.GetProposalById;
using FreelanceOps.Application.Proposals.GetProposals;
using FreelanceOps.Application.Proposals.RejectProposal;
using FreelanceOps.Application.Proposals.SendProposal;
using FreelanceOps.Application.Proposals.UpdateProposal;
using FreelanceOps.Domain.Proposals;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FreelanceOps.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/workspaces/{workspaceId:guid}/proposals")]
public sealed class ProposalsController(
    CreateProposalHandler createProposalHandler,
    GetProposalsHandler getProposalsHandler,
    GetProposalByIdHandler getProposalByIdHandler,
    UpdateProposalHandler updateProposalHandler,
    DeleteProposalHandler deleteProposalHandler,
    SendProposalHandler sendProposalHandler,
    AcceptProposalHandler acceptProposalHandler,
    RejectProposalHandler rejectProposalHandler,
    CancelProposalHandler cancelProposalHandler,
    ConvertProposalToProjectHandler convertProposalToProjectHandler) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<CreateProposalResponse>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(
        Guid workspaceId,
        CreateProposalRequest request,
        CancellationToken cancellationToken)
    {
        var response = await createProposalHandler.Handle(
            new CreateProposalCommand(
                workspaceId,
                request.ClientId,
                request.Title,
                request.Scope,
                request.ValidUntil,
                request.Currency,
                request.Items?.Select(ToInput).ToArray() ?? []),
            cancellationToken);

        return Created(
            $"/api/workspaces/{workspaceId}/proposals/{response.Id}",
            response);
    }

    [HttpGet]
    [ProducesResponseType<PagedResult<ProposalSummaryResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(
        Guid workspaceId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] ProposalStatus? status = null,
        [FromQuery] Guid? clientId = null,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var response = await getProposalsHandler.Handle(
            new GetProposalsQuery(
                workspaceId,
                page,
                pageSize,
                status,
                clientId,
                search),
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("{proposalId:guid}")]
    [ProducesResponseType<ProposalDetailResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(
        Guid workspaceId,
        Guid proposalId,
        CancellationToken cancellationToken)
    {
        var response = await getProposalByIdHandler.Handle(
            new GetProposalByIdQuery(workspaceId, proposalId),
            cancellationToken);

        return Ok(response);
    }

    [HttpPut("{proposalId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Update(
        Guid workspaceId,
        Guid proposalId,
        UpdateProposalRequest request,
        CancellationToken cancellationToken)
    {
        await updateProposalHandler.Handle(
            new UpdateProposalCommand(
                workspaceId,
                proposalId,
                request.Title,
                request.Scope,
                request.ValidUntil,
                request.Currency,
                request.Items?.Select(ToInput).ToArray() ?? []),
            cancellationToken);

        return NoContent();
    }

    [HttpDelete("{proposalId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(
        Guid workspaceId,
        Guid proposalId,
        CancellationToken cancellationToken)
    {
        await deleteProposalHandler.Handle(
            new DeleteProposalCommand(workspaceId, proposalId),
            cancellationToken);

        return NoContent();
    }

    [HttpPatch("{proposalId:guid}/send")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Send(
        Guid workspaceId,
        Guid proposalId,
        CancellationToken cancellationToken)
    {
        await sendProposalHandler.Handle(
            new SendProposalCommand(workspaceId, proposalId),
            cancellationToken);

        return NoContent();
    }

    [HttpPatch("{proposalId:guid}/accept")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Accept(
        Guid workspaceId,
        Guid proposalId,
        CancellationToken cancellationToken)
    {
        await acceptProposalHandler.Handle(
            new AcceptProposalCommand(workspaceId, proposalId),
            cancellationToken);

        return NoContent();
    }

    [HttpPatch("{proposalId:guid}/reject")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Reject(
        Guid workspaceId,
        Guid proposalId,
        CancellationToken cancellationToken)
    {
        await rejectProposalHandler.Handle(
            new RejectProposalCommand(workspaceId, proposalId),
            cancellationToken);

        return NoContent();
    }

    [HttpPatch("{proposalId:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Cancel(
        Guid workspaceId,
        Guid proposalId,
        CancellationToken cancellationToken)
    {
        await cancelProposalHandler.Handle(
            new CancelProposalCommand(workspaceId, proposalId),
            cancellationToken);

        return NoContent();
    }

    [HttpPost("{proposalId:guid}/convert-to-project")]
    [ProducesResponseType<ConvertProposalToProjectResponse>(StatusCodes.Status201Created)]
    public async Task<IActionResult> ConvertToProject(
        Guid workspaceId,
        Guid proposalId,
        ConvertProposalToProjectRequest request,
        CancellationToken cancellationToken)
    {
        var response = await convertProposalToProjectHandler.Handle(
            new ConvertProposalToProjectCommand(
                workspaceId,
                proposalId,
                request.StartDate,
                request.Deadline),
            cancellationToken);

        return Created(
            $"/api/workspaces/{workspaceId}/projects/{response.ProjectId}",
            response);
    }

    private static ProposalItemInput ToInput(ProposalItemRequest item)
    {
        return new ProposalItemInput(
            item.Description,
            item.Quantity,
            item.UnitPrice,
            item.TaxRate);
    }
}

public sealed record CreateProposalRequest(
    Guid ClientId,
    string Title,
    string Scope,
    DateOnly ValidUntil,
    string Currency,
    IReadOnlyCollection<ProposalItemRequest> Items);

public sealed record UpdateProposalRequest(
    string Title,
    string Scope,
    DateOnly ValidUntil,
    string Currency,
    IReadOnlyCollection<ProposalItemRequest> Items);

public sealed record ProposalItemRequest(
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal TaxRate);

public sealed record ConvertProposalToProjectRequest(
    DateOnly? StartDate,
    DateOnly? Deadline);
