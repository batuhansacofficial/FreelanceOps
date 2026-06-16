using FreelanceOps.Application.ProjectTasks.ChangeProjectTaskStatus;
using FreelanceOps.Application.ProjectTasks.DeleteProjectTask;
using FreelanceOps.Application.ProjectTasks.GetProjectTaskById;
using FreelanceOps.Application.ProjectTasks.UpdateProjectTask;
using FreelanceOps.Domain.Projects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FreelanceOps.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/workspaces/{workspaceId:guid}/tasks")]
public sealed class ProjectTasksController(
    GetProjectTaskByIdHandler getProjectTaskByIdHandler,
    UpdateProjectTaskHandler updateProjectTaskHandler,
    ChangeProjectTaskStatusHandler changeProjectTaskStatusHandler,
    DeleteProjectTaskHandler deleteProjectTaskHandler) : ControllerBase
{
    [HttpGet("{taskId:guid}")]
    [ProducesResponseType<ProjectTaskDetailResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(
        Guid workspaceId,
        Guid taskId,
        CancellationToken cancellationToken)
    {
        var response = await getProjectTaskByIdHandler.Handle(
            new GetProjectTaskByIdQuery(workspaceId, taskId),
            cancellationToken);

        return Ok(response);
    }

    [HttpPut("{taskId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Update(
        Guid workspaceId,
        Guid taskId,
        UpdateProjectTaskRequest request,
        CancellationToken cancellationToken)
    {
        await updateProjectTaskHandler.Handle(
            new UpdateProjectTaskCommand(
                workspaceId,
                taskId,
                request.Title,
                request.Description,
                request.Priority,
                request.DueDate,
                request.AssignedToUserId),
            cancellationToken);

        return NoContent();
    }

    [HttpPatch("{taskId:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ChangeStatus(
        Guid workspaceId,
        Guid taskId,
        ChangeProjectTaskStatusRequest request,
        CancellationToken cancellationToken)
    {
        await changeProjectTaskStatusHandler.Handle(
            new ChangeProjectTaskStatusCommand(workspaceId, taskId, request.Status),
            cancellationToken);

        return NoContent();
    }

    [HttpDelete("{taskId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(
        Guid workspaceId,
        Guid taskId,
        CancellationToken cancellationToken)
    {
        await deleteProjectTaskHandler.Handle(
            new DeleteProjectTaskCommand(workspaceId, taskId),
            cancellationToken);

        return NoContent();
    }
}

public sealed record UpdateProjectTaskRequest(
    string Title,
    string? Description,
    ProjectTaskPriority Priority,
    DateOnly? DueDate,
    Guid? AssignedToUserId);

public sealed record ChangeProjectTaskStatusRequest(ProjectTaskStatus Status);
