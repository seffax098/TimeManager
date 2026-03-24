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
[Route("api/profile")]
public sealed class ProfileController(AppDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ProfileResponse>> GetProfile(CancellationToken cancellationToken)
    {
        var userId = User.GetRequiredUserId();

        var user = await dbContext.Users
            .AsNoTracking()
            .Include(x => x.TechStackItems)
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

        if (user is null)
        {
            return NotFound();
        }

        return Ok(MapProfile(user));
    }

    [HttpPut]
    public async Task<ActionResult<UpdateProfileResponse>> UpdateProfile(
        [FromBody] UpdateProfileRequest request,
        CancellationToken cancellationToken)
    {
        var fullName = string.Equals(request.FullName, "string", StringComparison.OrdinalIgnoreCase) ? null : request.FullName;
        var settings = request.Settings;
        if (settings is not null)
        {
            if (string.Equals(settings.WorkTime, "string", StringComparison.OrdinalIgnoreCase))
            {
                settings = settings with { WorkTime = null };
            }

            if (string.Equals(settings.Theme, "string", StringComparison.OrdinalIgnoreCase))
            {
                settings = settings with { Theme = null };
            }
        }

        request = request with { FullName = fullName, Settings = settings };

        var userId = User.GetRequiredUserId();

        var user = await dbContext.Users
            .Include(x => x.TechStackItems)
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

        if (user is null)
        {
            return NotFound();
        }

        if (!string.IsNullOrWhiteSpace(request.FullName))
        {
            user.FullName = request.FullName.Trim();
        }

        if (request.Settings is not null)
        {
            user.Settings ??= new UserSettings();

            if (!string.IsNullOrWhiteSpace(request.Settings.WorkTime)
                && TimeSpan.TryParse(request.Settings.WorkTime, out var workTime))
            {
                user.Settings.WorkTime = workTime;
            }

            if (!string.IsNullOrWhiteSpace(request.Settings.Theme))
            {
                user.Settings.Theme = request.Settings.Theme.Trim().ToLowerInvariant();
            }
        }

        await using var tx = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        if (request.TechStack is not null)
        {
            var normalized = request.TechStack
                .Where(x => !string.IsNullOrWhiteSpace(x.Name))
                .Select(x => x.Name.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x) && !string.Equals(x, "string", StringComparison.OrdinalIgnoreCase))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Select((name, idx) => new { Name = name, Position = idx })
                .ToList();

            if (normalized.Count == 0)
            {
                return BadRequest(new { message = "techStack must contain at least one non-empty item." });
            }

            await dbContext.TechStackItems
                .Where(x => x.UserId == user.UserId)
                .ExecuteDeleteAsync(cancellationToken);

            dbContext.TechStackItems.AddRange(normalized.Select(x => new TechStackItem
            {
                ItemId = Guid.NewGuid(),
                UserId = user.UserId,
                Name = x.Name,
                Position = x.Position,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            }));
        }

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await tx.RollbackAsync(cancellationToken);
            return BadRequest(new { message = "Concurrency error while updating profile.", detail = ex.InnerException?.Message ?? ex.Message });
        }
        catch (DbUpdateException ex)
        {
            await tx.RollbackAsync(cancellationToken);
            return BadRequest(new { message = "Failed to update profile.", detail = ex.InnerException?.Message ?? ex.Message });
        }

        return Ok(new UpdateProfileResponse(
            "Профиль обновлён",
            MapProfile(user)));
    }

    private static ProfileResponse MapProfile(User user)
    {
        var settings = user.Settings ?? new UserSettings();

        return new ProfileResponse(
            user.UserId,
            user.Login,
            user.FullName,
            user.Role,
            user.TechStackItems
                .OrderBy(x => x.Position)
                .Select(x => new TechStackItemDto(x.ItemId, x.Name, x.Position))
                .ToList(),
            new SettingsDto(
                settings.WorkTime.ToString(@"hh\:mm\:ss"),
                settings.Theme),
            user.CreatedAt);
    }
}
