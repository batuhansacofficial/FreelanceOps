using FreelanceOps.Application.Reports.GetDashboard;
using FreelanceOps.Application.Reports.GetRevenueReport;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FreelanceOps.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/workspaces/{workspaceId:guid}/reports")]
public sealed class ReportsController(
    GetDashboardHandler getDashboardHandler,
    GetRevenueReportHandler getRevenueReportHandler) : ControllerBase
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

    [HttpGet("revenue")]
    [ProducesResponseType<RevenueReportResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRevenue(
        Guid workspaceId,
        [FromQuery] DateOnly? from = null,
        [FromQuery] DateOnly? to = null,
        [FromQuery] string groupBy = "month",
        CancellationToken cancellationToken = default)
    {
        var response = await getRevenueReportHandler.Handle(
            new GetRevenueReportQuery(workspaceId, from, to, groupBy),
            cancellationToken);

        return Ok(response);
    }
}
