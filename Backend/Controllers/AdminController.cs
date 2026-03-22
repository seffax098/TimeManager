using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Backend.Contracts;
using Backend.Services;

namespace Backend.Controllers;

[ApiController]
[Authorize(Roles = "admin")]
[Route("api/admin")]
public sealed class AdminController(ReportService reportService) : ControllerBase
{
    [HttpGet("employees")]
    public async Task<ActionResult<AdminEmployeesResponse>> GetEmployees(
        [FromQuery] DateOnly? date,
        [FromQuery(Name = "sort_by")] string? sortBy,
        [FromQuery] string? order,
        CancellationToken cancellationToken)
    {
        var reportDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var response = await reportService.BuildAdminEmployeesAsync(reportDate, sortBy, order, cancellationToken);
        return Ok(response);
    }

    [HttpGet("employees/{id:guid}")]
    public async Task<ActionResult<AdminEmployeeDetailsResponse>> GetEmployeeDetails(Guid id, [FromQuery] DateOnly? date, CancellationToken cancellationToken)
    {
        var reportDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var response = await reportService.BuildAdminEmployeeDetailsAsync(id, reportDate, cancellationToken);
        return response is null ? NotFound() : Ok(response);
    }
}
