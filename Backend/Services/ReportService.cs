using Microsoft.EntityFrameworkCore;
using Backend.Contracts;
using Backend.Data;
using Backend.Models;

namespace Backend.Services;

public sealed class ReportService(AppDbContext dbContext)
{
    public async Task<DayReportResponse?> BuildDayReportAsync(Guid userId, DateOnly date, CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users.AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

        if (user is null)
        {
            return null;
        }

        var session = await dbContext.WorkSessions.AsNoTracking()
            .Where(x => x.UserId == userId && x.WorkDate == date)
            .OrderByDescending(x => x.StartedAt)
            .FirstOrDefaultAsync(cancellationToken);

        var activities = await dbContext.ActivityRecords.AsNoTracking()
            .Where(x => x.Session.UserId == userId && x.Session.WorkDate == date)
            .OrderByDescending(x => x.DurationSec)
            .ToListAsync(cancellationToken);

        var summary = BuildSummary(session, activities);
        var sites = BuildSites(activities);

        SessionInfoDto? sessionInfo = null;
        if (session is not null)
        {
            var calculatedTotalSeconds = session.EndedAt.HasValue
                ? (int)Math.Max(0, (session.EndedAt.Value - session.StartedAt).TotalSeconds)
                : (int)Math.Max(0, (DateTimeOffset.UtcNow - session.StartedAt).TotalSeconds);

            var totalSeconds = session.TotalSeconds > 0
                ? Math.Max(session.TotalSeconds, calculatedTotalSeconds)
                : calculatedTotalSeconds;

            sessionInfo = new SessionInfoDto(
                session.SessionId,
                session.StartedAt,
                session.EndedAt,
                totalSeconds,
                session.Status == SessionStatus.active || session.Status == SessionStatus.paused);
        }

        return new DayReportResponse(
            user.UserId,
            user.FullName,
            date,
            sessionInfo,
            summary,
            sites);
    }

    public async Task<AdminEmployeesResponse> BuildAdminEmployeesAsync(
        DateOnly date,
        string? sortBy,
        string? order,
        CancellationToken cancellationToken = default)
    {
        var employees = await dbContext.Users.AsNoTracking()
            .Where(x => x.Role == UserRole.employee)
            .OrderBy(x => x.FullName)
            .Select(x => new { x.UserId, x.FullName })
            .ToListAsync(cancellationToken);

        var sessionLookup = await dbContext.WorkSessions.AsNoTracking()
            .Where(x => x.WorkDate == date)
            .GroupBy(x => x.UserId)
            .Select(g => new
            {
                UserId = g.Key,
                TotalSeconds = g.Sum(x => x.TotalSeconds),
                WorkTimeSec = g.Sum(x => x.WorkTimeSec),
                RestTimeSec = g.Sum(x => x.RestTimeSec)
            })
            .ToDictionaryAsync(x => x.UserId, cancellationToken);

        var result = employees.Select(employee =>
        {
            sessionLookup.TryGetValue(employee.UserId, out var totals);

            var workSec = totals?.WorkTimeSec ?? 0;
            var restSec = totals?.RestTimeSec ?? 0;
            var totalSec = totals?.TotalSeconds ?? 0;

            if (totalSec == 0)
            {
                totalSec = workSec + restSec;
            }

            var workPercent = totalSec == 0 ? 0 : Math.Round((decimal)workSec * 100m / totalSec, 2);
            var restPercent = totalSec == 0 ? 0 : Math.Round((decimal)restSec * 100m / totalSec, 2);
            var color = GetStatusColor(workPercent);

            return new AdminEmployeeSummaryDto(
                employee.UserId,
                employee.FullName,
                workPercent,
                restPercent,
                totalSec / 60,
                color);
        });

        var ordered = ApplySorting(result, sortBy, order).ToList();
        return new AdminEmployeesResponse(date, ordered.Count, ordered);
    }

    public async Task<AdminEmployeeDetailsResponse?> BuildAdminEmployeeDetailsAsync(
        Guid userId,
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users.AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

        if (user is null)
        {
            return null;
        }

        var session = await dbContext.WorkSessions.AsNoTracking()
            .Where(x => x.UserId == userId && x.WorkDate == date)
            .OrderByDescending(x => x.StartedAt)
            .FirstOrDefaultAsync(cancellationToken);

        var activities = await dbContext.ActivityRecords.AsNoTracking()
            .Where(x => x.Session.UserId == userId && x.Session.WorkDate == date)
            .OrderByDescending(x => x.DurationSec)
            .ToListAsync(cancellationToken);

        var violations = await dbContext.Violations.AsNoTracking()
            .Where(x => x.Activity.Session.UserId == userId && x.Activity.Session.WorkDate == date)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new ViolationResponse(
                x.ViolationId,
                x.ActivityId,
                x.Reason,
                x.ScreenshotPath,
                x.IsDisputed,
                x.CreatedAt))
            .ToListAsync(cancellationToken);

        return new AdminEmployeeDetailsResponse(
            user.UserId,
            user.FullName,
            date,
            violations,
            BuildSummary(session, activities),
            BuildSites(activities));
    }

    private static IEnumerable<AdminEmployeeSummaryDto> ApplySorting(
        IEnumerable<AdminEmployeeSummaryDto> source,
        string? sortBy,
        string? order)
    {
        var desc = string.Equals(order, "desc", StringComparison.OrdinalIgnoreCase);
        var normalizedSort = sortBy?.Trim().ToLowerInvariant();

        Func<AdminEmployeeSummaryDto, object> selector = normalizedSort switch
        {
            "work_percent" => x => x.WorkPercent,
            "rest_percent" => x => x.RestPercent,
            "total_minutes" => x => x.TotalMinutes,
            _ => x => x.FullName
        };

        return desc ? source.OrderByDescending(selector) : source.OrderBy(selector);
    }

    private static ReportSummaryDto BuildSummary(WorkSession? session, IReadOnlyCollection<ActivityRecord> activities)
    {
        var workTime = session?.WorkTimeSec ?? activities.Where(x => x.Verdict == ActivityVerdict.work).Sum(x => x.DurationSec);
        var restTime = session?.RestTimeSec ?? activities.Where(x => x.Verdict == ActivityVerdict.rest).Sum(x => x.DurationSec);
        var total = workTime + restTime;

        var workPercent = total == 0 ? 0 : Math.Round((decimal)workTime * 100m / total, 2);
        var restPercent = total == 0 ? 0 : Math.Round((decimal)restTime * 100m / total, 2);

        return new ReportSummaryDto(workPercent, restPercent, workTime, restTime);
    }

    private static IReadOnlyList<ReportSiteDto> BuildSites(IEnumerable<ActivityRecord> activities)
    {
        return activities
            .GroupBy(x => x.Domain)
            .Select(group =>
            {
                var totalDuration = group.Sum(x => x.DurationSec);

                var dominantVerdict = group
                    .GroupBy(x => x.Verdict)
                    .OrderByDescending(x => x.Sum(v => v.DurationSec))
                    .Select(x => x.Key)
                    .FirstOrDefault();

                var links = group
                    .OrderByDescending(x => x.DurationSec)
                    .Select(x => new ReportLinkDto(x.ActivityId, x.Url, x.DurationSec / 60, x.Verdict))
                    .ToList();

                return new ReportSiteDto(group.Key, totalDuration / 60, dominantVerdict, links);
            })
            .OrderByDescending(x => x.TotalMinutes)
            .ToList();
    }

    private static ReportStatusColor GetStatusColor(decimal workPercent)
    {
        if (workPercent >= 75) return ReportStatusColor.green;
        if (workPercent >= 50) return ReportStatusColor.yellow;
        return ReportStatusColor.red;
    }
}