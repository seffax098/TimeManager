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
[Route("api/activity")]
public sealed class ActivityController(AppDbContext dbContext) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ActivityResponse>> Create([FromBody] ActivityRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetRequiredUserId();
        var session = await dbContext.WorkSessions
            .FirstOrDefaultAsync(x => x.SessionId == request.SessionId && x.UserId == userId, cancellationToken);

        if (session is null)
        {
            return NotFound(new { message = "Сессия не найдена." });
        }

        if (session.Status == SessionStatus.completed)
        {
            return Conflict(new { message = "Нельзя записывать активность в завершённую сессию." });
        }

        var duration = request.DurationSec;
        if (duration == 0)
        {
            duration = (int)Math.Max(0, (request.EndedAt - request.StartedAt).TotalSeconds);
        }

        var verdict = request.Verdict ?? ActivityVerdict.unknown;

        var activity = new ActivityRecord
        {
            ActivityId = Guid.NewGuid(),
            SessionId = session.SessionId,
            Domain = request.Domain.Trim(),
            Url = request.Url.Trim(),
            StartedAt = request.StartedAt,
            EndedAt = request.EndedAt,
            DurationSec = duration,
            Verdict = verdict,
            CreatedAt = DateTimeOffset.UtcNow
        };

        if (verdict == ActivityVerdict.work)
        {
            session.WorkTimeSec += duration;
        }
        else if (verdict == ActivityVerdict.rest)
        {
            session.RestTimeSec += duration;
        }

        dbContext.ActivityRecords.Add(activity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new ActivityResponse(
            activity.ActivityId,
            activity.SessionId,
            activity.Domain,
            activity.Url,
            activity.DurationSec,
            activity.Verdict,
            activity.CreatedAt));
    }
}
