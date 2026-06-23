using FreelanceOps.Application.Reports.GetClientSummary;
using FreelanceOps.Application.Reports.GetDashboard;
using FreelanceOps.Application.Reports.GetProjectPerformance;
using FreelanceOps.Application.Reports.GetRevenueReport;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FreelanceOps.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/workspaces/{workspaceId:guid}/reports")]
public sealed class ReportsController(
    GetClientSummaryHandler getClientSummaryHandler,
    GetDashboardHandler getDashboardHandler,
    GetProjectPerformanceHandler getProjectPerformanceHandler,
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

    [HttpGet("client-summary")]
    [ProducesResponseType<ClientSummaryResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetClientSummary(
        Guid workspaceId,
        [FromQuery] DateOnly? from = null,
        [FromQuery] DateOnly? to = null,
        CancellationToken cancellationToken = default)
    {
        var response = await getClientSummaryHandler.Handle(
            new GetClientSummaryQuery(workspaceId, from, to),
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("project-performance")]
    [ProducesResponseType<ProjectPerformanceResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProjectPerformance(
        Guid workspaceId,
        [FromQuery] DateOnly? from = null,
        [FromQuery] DateOnly? to = null,
        CancellationToken cancellationToken = default)
    {
        var response = await getProjectPerformanceHandler.Handle(
            new GetProjectPerformanceQuery(workspaceId, from, to),
            cancellationToken);

        return Ok(response);
    }
}
