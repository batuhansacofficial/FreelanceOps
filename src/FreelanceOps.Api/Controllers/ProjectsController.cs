using FreelanceOps.Application.Common.Pagination;
using FreelanceOps.Application.Projects.ChangeProjectStatus;
using FreelanceOps.Application.Projects.CreateProject;
using FreelanceOps.Application.Projects.DeleteProject;
using FreelanceOps.Application.Projects.GetProjectById;
using FreelanceOps.Application.Projects.GetProjects;
using FreelanceOps.Application.Projects.UpdateProject;
using FreelanceOps.Application.ProjectTasks.CreateProjectTask;
using FreelanceOps.Application.ProjectTasks.GetProjectTasks;
using FreelanceOps.Domain.Projects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FreelanceOps.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/workspaces/{workspaceId:guid}/projects")]
public sealed class ProjectsController(
    CreateProjectHandler createProjectHandler,
    GetProjectsHandler getProjectsHandler,
    GetProjectByIdHandler getProjectByIdHandler,
    UpdateProjectHandler updateProjectHandler,
    ChangeProjectStatusHandler changeProjectStatusHandler,
    DeleteProjectHandler deleteProjectHandler,
    CreateProjectTaskHandler createProjectTaskHandler,
    GetProjectTasksHandler getProjectTasksHandler) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<CreateProjectResponse>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(
        Guid workspaceId,
        CreateProjectRequest request,
        CancellationToken cancellationToken)
    {
        var response = await createProjectHandler.Handle(
            new CreateProjectCommand(
                workspaceId,
                request.ClientId,
                request.Name,
                request.Description,
                request.StartDate,
                request.Deadline),
            cancellationToken);

        return Created($"/api/workspaces/{workspaceId}/projects/{response.Id}", response);
    }

    [HttpGet]
    [ProducesResponseType<PagedResult<ProjectSummaryResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(
        Guid workspaceId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] ProjectStatus? status = null,
        [FromQuery] Guid? clientId = null,
        CancellationToken cancellationToken = default)
    {
        var response = await getProjectsHandler.Handle(
            new GetProjectsQuery(workspaceId, page, pageSize, search, status, clientId),
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("{projectId:guid}")]
    [ProducesResponseType<ProjectDetailResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(
        Guid workspaceId,
        Guid projectId,
        CancellationToken cancellationToken)
    {
        var response = await getProjectByIdHandler.Handle(
            new GetProjectByIdQuery(workspaceId, projectId),
            cancellationToken);

        return Ok(response);
    }

    [HttpPut("{projectId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Update(
        Guid workspaceId,
        Guid projectId,
        UpdateProjectRequest request,
        CancellationToken cancellationToken)
    {
        await updateProjectHandler.Handle(
            new UpdateProjectCommand(
                workspaceId,
                projectId,
                request.Name,
                request.Description,
                request.StartDate,
                request.Deadline),
            cancellationToken);

        return NoContent();
    }

    [HttpPatch("{projectId:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ChangeStatus(
        Guid workspaceId,
        Guid projectId,
        ChangeProjectStatusRequest request,
        CancellationToken cancellationToken)
    {
        await changeProjectStatusHandler.Handle(
            new ChangeProjectStatusCommand(workspaceId, projectId, request.Status),
            cancellationToken);

        return NoContent();
    }

    [HttpDelete("{projectId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(
        Guid workspaceId,
        Guid projectId,
        CancellationToken cancellationToken)
    {
        await deleteProjectHandler.Handle(
            new DeleteProjectCommand(workspaceId, projectId),
            cancellationToken);

        return NoContent();
    }

    [HttpPost("{projectId:guid}/tasks")]
    [ProducesResponseType<CreateProjectTaskResponse>(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateTask(
        Guid workspaceId,
        Guid projectId,
        CreateProjectTaskRequest request,
        CancellationToken cancellationToken)
    {
        var response = await createProjectTaskHandler.Handle(
            new CreateProjectTaskCommand(
                workspaceId,
                projectId,
                request.Title,
                request.Description,
                request.Priority,
                request.DueDate,
                request.AssignedToUserId),
            cancellationToken);

        return Created($"/api/workspaces/{workspaceId}/tasks/{response.Id}", response);
    }

    [HttpGet("{projectId:guid}/tasks")]
    [ProducesResponseType<PagedResult<ProjectTaskSummaryResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTasks(
        Guid workspaceId,
        Guid projectId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] ProjectTaskStatus? status = null,
        [FromQuery] Guid? assignedToUserId = null,
        CancellationToken cancellationToken = default)
    {
        var response = await getProjectTasksHandler.Handle(
            new GetProjectTasksQuery(workspaceId, projectId, page, pageSize, status, assignedToUserId),
            cancellationToken);

        return Ok(response);
    }
}

public sealed record CreateProjectRequest(
    Guid ClientId,
    string Name,
    string? Description,
    DateOnly? StartDate,
    DateOnly? Deadline);

public sealed record UpdateProjectRequest(
    string Name,
    string? Description,
    DateOnly? StartDate,
    DateOnly? Deadline);

public sealed record ChangeProjectStatusRequest(ProjectStatus Status);

public sealed record CreateProjectTaskRequest(
    string Title,
    string? Description,
    ProjectTaskPriority Priority,
    DateOnly? DueDate,
    Guid? AssignedToUserId);
