using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Contracts;
using Backend.Data;
using Backend.Infrastructure;
using Backend.Models;

namespace Backend.Controllers;

[ApiController]
[Authorize]
[Route("api/violations")]
public sealed class ViolationsController(AppDbContext dbContext, IWebHostEnvironment environment) : ControllerBase
{
    [HttpPost]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<ActionResult<ViolationResponse>> Create([FromForm] CreateViolationRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetRequiredUserId();
        var activity = await dbContext.ActivityRecords
            .Include(x => x.Session)
            .FirstOrDefaultAsync(x => x.ActivityId == request.ActivityId && x.Session.UserId == userId, cancellationToken);

        if (activity is null)
        {
            return NotFound(new { message = "Активность не найдена." });
        }

        var exists = await dbContext.Violations.AnyAsync(x => x.ActivityId == request.ActivityId, cancellationToken);
        if (exists)
        {
            return Conflict(new { message = "Нарушение для этой активности уже создано." });
        }

        var todayFolder = DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd");
        var uploadsFolder = Path.Combine(environment.ContentRootPath, "uploads", "violations", todayFolder);
        Directory.CreateDirectory(uploadsFolder);

        var extension = Path.GetExtension(request.Screenshot.FileName).ToLowerInvariant();
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        await using (var stream = System.IO.File.Create(filePath))
        {
            await request.Screenshot.CopyToAsync(stream, cancellationToken);
        }

        var relativePath = $"/uploads/violations/{todayFolder}/{fileName}";

        var violation = new Violation
        {
            ViolationId = Guid.NewGuid(),
            ActivityId = activity.ActivityId,
            Reason = request.Reason.Trim(),
            ScreenshotPath = relativePath,
            IsDisputed = false,
            CreatedAt = DateTimeOffset.UtcNow
        };

        dbContext.Violations.Add(violation);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new ViolationResponse(
            violation.ViolationId,
            violation.ActivityId,
            violation.Reason,
            violation.ScreenshotPath,
            violation.IsDisputed,
            violation.CreatedAt));
    }
}
