using FreelanceOps.Application.Common.Pagination;
using FreelanceOps.Application.TimeTracking.DeleteTimeEntry;
using FreelanceOps.Application.TimeTracking.GetTimeEntries;
using FreelanceOps.Application.TimeTracking.StopTimer;
using FreelanceOps.Application.TimeTracking.UpdateTimeEntry;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FreelanceOps.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/workspaces/{workspaceId:guid}/time-entries")]
public sealed class TimeEntriesController(
    GetTimeEntriesHandler getTimeEntriesHandler,
    StopTimerHandler stopTimerHandler,
    UpdateTimeEntryHandler updateTimeEntryHandler,
    DeleteTimeEntryHandler deleteTimeEntryHandler) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PagedResult<TimeEntryResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(
        Guid workspaceId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? userId = null,
        [FromQuery] Guid? projectId = null,
        [FromQuery] Guid? taskId = null,
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
                projectId,
                taskId,
                from,
                to),
            cancellationToken);

        return Ok(response);
    }

    [HttpPost("{timeEntryId:guid}/stop")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Stop(
        Guid workspaceId,
        Guid timeEntryId,
        CancellationToken cancellationToken)
    {
        await stopTimerHandler.Handle(
            new StopTimerCommand(workspaceId, timeEntryId),
            cancellationToken);

        return NoContent();
    }

    [HttpPut("{timeEntryId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Update(
        Guid workspaceId,
        Guid timeEntryId,
        UpdateTimeEntryRequest request,
        CancellationToken cancellationToken)
    {
        await updateTimeEntryHandler.Handle(
            new UpdateTimeEntryCommand(
                workspaceId,
                timeEntryId,
                request.StartedAtUtc,
                request.DurationMinutes,
                request.Description),
            cancellationToken);

        return NoContent();
    }

    [HttpDelete("{timeEntryId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(
        Guid workspaceId,
        Guid timeEntryId,
        CancellationToken cancellationToken)
    {
        await deleteTimeEntryHandler.Handle(
            new DeleteTimeEntryCommand(workspaceId, timeEntryId),
            cancellationToken);

        return NoContent();
    }
}

public sealed record UpdateTimeEntryRequest(
    DateTime StartedAtUtc,
    int DurationMinutes,
    string? Description);
