using FreelanceOps.Application.Common.Pagination;
using FreelanceOps.Application.TimeTracking.GetTimeEntries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FreelanceOps.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/workspaces/{workspaceId:guid}/projects/{projectId:guid}/time-entries")]
public sealed class ProjectTimeEntriesController(
    GetTimeEntriesHandler getTimeEntriesHandler) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PagedResult<TimeEntryResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(
        Guid workspaceId,
        Guid projectId,
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
                ProjectId: projectId,
                From: from,
                To: to),
            cancellationToken);

        return Ok(response);
    }
}
