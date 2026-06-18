using FreelanceOps.Application.TimeTracking.GetTimeSummary;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FreelanceOps.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/workspaces/{workspaceId:guid}/reports")]
public sealed class TimeReportsController(
    GetTimeSummaryHandler getTimeSummaryHandler) : ControllerBase
{
    [HttpGet("time-summary")]
    [ProducesResponseType<TimeSummaryResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTimeSummary(
        Guid workspaceId,
        [FromQuery] DateOnly? from = null,
        [FromQuery] DateOnly? to = null,
        [FromQuery] Guid? projectId = null,
        [FromQuery] Guid? taskId = null,
        CancellationToken cancellationToken = default)
    {
        var response = await getTimeSummaryHandler.Handle(
            new GetTimeSummaryQuery(workspaceId, from, to, projectId, taskId),
            cancellationToken);

        return Ok(response);
    }
}
