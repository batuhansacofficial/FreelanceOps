using FreelanceOps.Application.Reports.GetDashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FreelanceOps.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/workspaces/{workspaceId:guid}/reports")]
public sealed class ReportsController(
    GetDashboardHandler getDashboardHandler) : ControllerBase
{
    [HttpGet("dashboard")]
    [ProducesResponseType<DashboardResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboard(
        Guid workspaceId,
        CancellationToken cancellationToken)
    {
        var response = await getDashboardHandler.Handle(
            new GetDashboardQuery(workspaceId),
            cancellationToken);

        return Ok(response);
    }
}
