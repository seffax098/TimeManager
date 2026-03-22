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

        var duration = request.DurationSec > 0
            ? request.DurationSec
            : (int)Math.Max(0, (request.EndedAt - request.StartedAt).TotalSeconds);

        var verdict = request.Verdict ?? ActivityVerdict.unknown;
        var now = DateTimeOffset.UtcNow;

        var activity = new ActivityRecord
        {
            ActivityId = Guid.NewGuid(),
            SessionId = session.SessionId,
            SourceType = request.SourceType ?? ActivitySourceType.browser,
            SourceName = string.IsNullOrWhiteSpace(request.SourceName) ? null : request.SourceName.Trim(),
            Domain = request.Domain.Trim(),
            Url = request.Url.Trim(),
            StartedAt = request.StartedAt,
            EndedAt = request.EndedAt,
            DurationSec = duration,
            Verdict = verdict,
            CreatedAt = now
        };

        session.TotalSeconds += duration;
        ApplyVerdictDelta(session, null, verdict, duration);
        session.UpdatedAt = now;

        dbContext.ActivityRecords.Add(activity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(MapActivity(activity));
    }

    [HttpPut("{activityId:guid}/verdict")]
    public async Task<ActionResult<UpdateActivityVerdictResponse>> UpdateVerdict(
        Guid activityId,
        [FromBody] UpdateActivityVerdictRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetRequiredUserId();

        var activity = await dbContext.ActivityRecords
            .Include(x => x.Session)
            .FirstOrDefaultAsync(x => x.ActivityId == activityId && x.Session.UserId == userId, cancellationToken);

        if (activity is null)
        {
            return NotFound(new { message = "Активность не найдена." });
        }

        var previousVerdict = activity.Verdict;
        if (previousVerdict == request.Verdict)
        {
            return Ok(new UpdateActivityVerdictResponse(
                activity.ActivityId,
                activity.SessionId,
                previousVerdict,
                activity.Verdict,
                activity.Session.WorkTimeSec,
                activity.Session.RestTimeSec));
        }

        ApplyVerdictDelta(activity.Session, previousVerdict, request.Verdict, activity.DurationSec);
        activity.Verdict = request.Verdict;
        activity.Session.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new UpdateActivityVerdictResponse(
            activity.ActivityId,
            activity.SessionId,
            previousVerdict,
            activity.Verdict,
            activity.Session.WorkTimeSec,
            activity.Session.RestTimeSec));
    }

    private static ActivityResponse MapActivity(ActivityRecord activity)
    {
        return new ActivityResponse(
            activity.ActivityId,
            activity.SessionId,
            activity.Domain,
            activity.Url,
            activity.DurationSec,
            activity.Verdict,
            activity.CreatedAt,
            activity.SourceType,
            activity.SourceName);
    }

    private static void ApplyVerdictDelta(
        WorkSession session,
        ActivityVerdict? oldVerdict,
        ActivityVerdict? newVerdict,
        int durationSec)
    {
        if (oldVerdict == ActivityVerdict.work)
        {
            session.WorkTimeSec = Math.Max(0, session.WorkTimeSec - durationSec);
        }
        else if (oldVerdict == ActivityVerdict.rest)
        {
            session.RestTimeSec = Math.Max(0, session.RestTimeSec - durationSec);
        }

        if (newVerdict == ActivityVerdict.work)
        {
            session.WorkTimeSec += durationSec;
        }
        else if (newVerdict == ActivityVerdict.rest)
        {
            session.RestTimeSec += durationSec;
        }
    }
}