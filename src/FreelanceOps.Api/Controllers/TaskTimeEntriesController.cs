using FreelanceOps.Application.Common.Pagination;
using FreelanceOps.Application.TimeTracking.CreateManualTimeEntry;
using FreelanceOps.Application.TimeTracking.GetTimeEntries;
using FreelanceOps.Application.TimeTracking.StartTimer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FreelanceOps.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/workspaces/{workspaceId:guid}/tasks/{taskId:guid}/time-entries")]
public sealed class TaskTimeEntriesController(
    StartTimerHandler startTimerHandler,
    CreateManualTimeEntryHandler createManualTimeEntryHandler,
    GetTimeEntriesHandler getTimeEntriesHandler) : ControllerBase
{
    [HttpPost("start")]
    [ProducesResponseType<StartTimerResponse>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Start(
        Guid workspaceId,
        Guid taskId,
        StartTimerRequest request,
        CancellationToken cancellationToken)
    {
        var response = await startTimerHandler.Handle(
            new StartTimerCommand(workspaceId, taskId, request.Description),
            cancellationToken);

        return Created(
            $"/api/workspaces/{workspaceId}/time-entries/{response.Id}",
            response);
    }

    [HttpPost("manual")]
    [ProducesResponseType<TimeEntryResponse>(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateManual(
        Guid workspaceId,
        Guid taskId,
        CreateManualTimeEntryRequest request,
        CancellationToken cancellationToken)
    {
        var response = await createManualTimeEntryHandler.Handle(
            new CreateManualTimeEntryCommand(
                workspaceId,
                taskId,
                request.StartedAtUtc,
                request.DurationMinutes,
                request.Description),
            cancellationToken);

        return Created(
            $"/api/workspaces/{workspaceId}/time-entries/{response.Id}",
            response);
    }

    [HttpGet]
    [ProducesResponseType<PagedResult<TimeEntryResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(
        Guid workspaceId,
        Guid taskId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? userId = null,
        [FromQuery] DateOnly? from = null,
        [FromQuery] DateOnly? to = null,
        CancellationToken cancellationToken = default)
    {
        var response = await getTimeEntriesHandler.Handle(
            new GetTimeEntriesQuery(
                workspaceId,
                page,
                pageSize,
                userId,
                TaskId: taskId,
                From: from,
                To: to),
            cancellationToken);

        return Ok(response);
    }
}

public sealed record StartTimerRequest(string? Description);

public sealed record CreateManualTimeEntryRequest(
    DateTime StartedAtUtc,
    int DurationMinutes,
    string? Description);
