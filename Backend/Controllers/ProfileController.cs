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
        var userId = User.GetRequiredUserId();

        var user = await dbContext.Users
            .Include(x => x.TechStackItems)
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

        if (user is null)
        {
            return NotFound();
        }

        var now = DateTimeOffset.UtcNow;
        var hasChanges = false;

        if (!string.IsNullOrWhiteSpace(request.FullName))
        {
            user.FullName = request.FullName.Trim();
            hasChanges = true;
        }

        if (request.Settings is not null)
        {
            user.Settings ??= new UserSettings();

            if (!string.IsNullOrWhiteSpace(request.Settings.WorkTime)
                && TimeSpan.TryParse(request.Settings.WorkTime, out var workTime))
            {
                user.Settings.WorkTime = workTime;
                hasChanges = true;
            }

            if (!string.IsNullOrWhiteSpace(request.Settings.Theme))
            {
                user.Settings.Theme = request.Settings.Theme.Trim().ToLowerInvariant();
                hasChanges = true;
            }
        }

        if (request.TechStack is not null)
        {
            var oldItems = user.TechStackItems.ToList();
            dbContext.TechStackItems.RemoveRange(oldItems);

            var newItems = request.TechStack
                .OrderBy(x => x.Position)
                .Select(x => new TechStackItem
                {
                    ItemId = Guid.NewGuid(),
                    UserId = user.UserId,
                    Name = x.Name.Trim(),
                    Position = x.Position,
                    CreatedAt = now,
                    UpdatedAt = now
                })
                .ToList();

            user.TechStackItems = newItems;
            await dbContext.TechStackItems.AddRangeAsync(newItems, cancellationToken);

            hasChanges = true;
        }

        if (hasChanges)
        {
            user.UpdatedAt = now;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

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