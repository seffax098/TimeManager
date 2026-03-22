using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Backend.Contracts;
using Backend.Infrastructure;
using Backend.Services;

namespace Backend.Controllers;

[ApiController]
[Authorize]
[Route("api/reports")]
public sealed class ReportsController(ReportService reportService) : ControllerBase
{
    [HttpGet("day")]
    public async Task<ActionResult<DayReportResponse>> GetDay([FromQuery] DateOnly? date, CancellationToken cancellationToken)
    {
        var userId = User.GetRequiredUserId();
        var reportDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var report = await reportService.BuildDayReportAsync(userId, reportDate, cancellationToken);

        return report is null ? NotFound() : Ok(report);
    }
}
