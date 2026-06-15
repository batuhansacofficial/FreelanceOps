using FreelanceOps.Application.Workspaces.CreateWorkspace;
using FreelanceOps.Application.Workspaces.DeleteWorkspace;
using FreelanceOps.Application.Workspaces.GetMyWorkspaces;
using FreelanceOps.Application.Workspaces.GetWorkspaceById;
using FreelanceOps.Application.Workspaces.Members;
using FreelanceOps.Application.Workspaces.Members.AddWorkspaceMember;
using FreelanceOps.Application.Workspaces.Members.ChangeWorkspaceMemberRole;
using FreelanceOps.Application.Workspaces.Members.GetWorkspaceMembers;
using FreelanceOps.Application.Workspaces.Members.RemoveWorkspaceMember;
using FreelanceOps.Application.Workspaces.RenameWorkspace;
using FreelanceOps.Domain.Workspaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FreelanceOps.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/workspaces")]
public sealed class WorkspacesController(
    CreateWorkspaceHandler createWorkspaceHandler,
    GetMyWorkspacesHandler getMyWorkspacesHandler,
    GetWorkspaceByIdHandler getWorkspaceByIdHandler,
    RenameWorkspaceHandler renameWorkspaceHandler,
    DeleteWorkspaceHandler deleteWorkspaceHandler,
    GetWorkspaceMembersHandler getWorkspaceMembersHandler,
    AddWorkspaceMemberHandler addWorkspaceMemberHandler,
    ChangeWorkspaceMemberRoleHandler changeWorkspaceMemberRoleHandler,
    RemoveWorkspaceMemberHandler removeWorkspaceMemberHandler) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<CreateWorkspaceResponse>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(
        CreateWorkspaceCommand command,
        CancellationToken cancellationToken)
    {
        var response = await createWorkspaceHandler.Handle(command, cancellationToken);

        return Created($"/api/workspaces/{response.Id}", response);
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyCollection<WorkspaceSummaryResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
    {
        var response = await getMyWorkspacesHandler.Handle(new GetMyWorkspacesQuery(), cancellationToken);

        return Ok(response);
    }

    [HttpGet("{workspaceId:guid}")]
    [ProducesResponseType<WorkspaceDetailResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(
        Guid workspaceId,
        CancellationToken cancellationToken)
    {
        var response = await getWorkspaceByIdHandler.Handle(
            new GetWorkspaceByIdQuery(workspaceId),
            cancellationToken);

        return Ok(response);
    }

    [HttpPut("{workspaceId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Rename(
        Guid workspaceId,
        RenameWorkspaceRequest request,
        CancellationToken cancellationToken)
    {
        await renameWorkspaceHandler.Handle(
            new RenameWorkspaceCommand(workspaceId, request.Name),
            cancellationToken);

        return NoContent();
    }

    [HttpDelete("{workspaceId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(
        Guid workspaceId,
        CancellationToken cancellationToken)
    {
        await deleteWorkspaceHandler.Handle(
            new DeleteWorkspaceCommand(workspaceId),
            cancellationToken);

        return NoContent();
    }

    [HttpGet("{workspaceId:guid}/members")]
    [ProducesResponseType<IReadOnlyCollection<WorkspaceMemberResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMembers(
        Guid workspaceId,
        CancellationToken cancellationToken)
    {
        var response = await getWorkspaceMembersHandler.Handle(
            new GetWorkspaceMembersQuery(workspaceId),
            cancellationToken);

        return Ok(response);
    }

    [HttpPost("{workspaceId:guid}/members")]
    [ProducesResponseType<WorkspaceMemberResponse>(StatusCodes.Status201Created)]
    public async Task<IActionResult> AddMember(
        Guid workspaceId,
        AddWorkspaceMemberRequest request,
        CancellationToken cancellationToken)
    {
        var response = await addWorkspaceMemberHandler.Handle(
            new AddWorkspaceMemberCommand(workspaceId, request.Email, request.Role),
            cancellationToken);

        return Created($"/api/workspaces/{workspaceId}/members/{response.Id}", response);
    }

    [HttpPatch("{workspaceId:guid}/members/{memberId:guid}/role")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ChangeMemberRole(
        Guid workspaceId,
        Guid memberId,
        ChangeWorkspaceMemberRoleRequest request,
        CancellationToken cancellationToken)
    {
        await changeWorkspaceMemberRoleHandler.Handle(
            new ChangeWorkspaceMemberRoleCommand(workspaceId, memberId, request.Role),
            cancellationToken);

        return NoContent();
    }

    [HttpDelete("{workspaceId:guid}/members/{memberId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveMember(
        Guid workspaceId,
        Guid memberId,
        CancellationToken cancellationToken)
    {
        await removeWorkspaceMemberHandler.Handle(
            new RemoveWorkspaceMemberCommand(workspaceId, memberId),
            cancellationToken);

        return NoContent();
    }
}

public sealed record RenameWorkspaceRequest(string Name);

public sealed record AddWorkspaceMemberRequest(string Email, WorkspaceRole Role);

public sealed record ChangeWorkspaceMemberRoleRequest(WorkspaceRole Role);
