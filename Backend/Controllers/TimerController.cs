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
[Route("api/timer")]
public sealed class TimerController(AppDbContext dbContext) : ControllerBase
{
    [HttpPost("start")]
    public async Task<ActionResult<StartTimerResponse>> Start([FromBody] StartTimerRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetRequiredUserId();

        var hasActiveSession = await dbContext.WorkSessions.AnyAsync(
            x => x.UserId == userId && x.Status != SessionStatus.completed,
            cancellationToken);

        if (hasActiveSession)
        {
            return Conflict(new { message = "У пользователя уже есть активная или поставленная на паузу сессия." });
        }

        var now = DateTimeOffset.UtcNow;

        var session = new WorkSession
        {
            SessionId = Guid.NewGuid(),
            UserId = userId,
            WorkDate = request.Date ?? DateOnly.FromDateTime(now.UtcDateTime),
            StartedAt = now,
            EndedAt = null,
            TotalSeconds = 0,
            WorkTimeSec = 0,
            RestTimeSec = 0,
            Status = SessionStatus.active,
            CreatedAt = now,
            UpdatedAt = now
        };

        dbContext.WorkSessions.Add(session);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new StartTimerResponse(session.SessionId, session.UserId, session.WorkDate, session.StartedAt, session.Status));
    }

    [HttpPost("stop")]
    public async Task<ActionResult<StopTimerResponse>> Stop([FromBody] StopTimerRequest request, CancellationToken cancellationToken)
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
            return Conflict(new { message = "Сессия уже завершена." });
        }

        var endedAt = DateTimeOffset.UtcNow;
        var elapsedSeconds = (int)Math.Max(0, (endedAt - session.StartedAt).TotalSeconds);

        session.EndedAt = endedAt;
        session.TotalSeconds = Math.Max(session.TotalSeconds, elapsedSeconds);
        session.Status = SessionStatus.completed;
        session.UpdatedAt = endedAt;

        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new StopTimerResponse(
            session.SessionId,
            session.StartedAt,
            session.EndedAt.Value,
            session.TotalSeconds,
            session.WorkTimeSec,
            session.RestTimeSec,
            session.Status));
    }
}