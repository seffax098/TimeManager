using System.ComponentModel.DataAnnotations;

namespace Backend.Models;

public sealed class User
{
    public Guid UserId { get; set; }

    [MaxLength(32)]
    public string Login { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    public UserRole Role { get; set; } = UserRole.employee;
    public UserSettings Settings { get; set; } = new();

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<TechStackItem> TechStackItems { get; set; } = new List<TechStackItem>();
    public ICollection<WorkSession> WorkSessions { get; set; } = new List<WorkSession>();
    public ICollection<DailyReport> DailyReports { get; set; } = new List<DailyReport>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}

public sealed class UserSettings
{
    public string Theme { get; set; } = "light";
    public TimeSpan WorkTime { get; set; } = new(8, 30, 0);
}

public sealed class TechStackItem
{
    public Guid ItemId { get; set; }
    public Guid UserId { get; set; }

    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public int Position { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public User User { get; set; } = null!;
}

public sealed class WorkSession
{
    public Guid SessionId { get; set; }
    public Guid UserId { get; set; }

    public DateOnly WorkDate { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? EndedAt { get; set; }

    public int TotalSeconds { get; set; }
    public int WorkTimeSec { get; set; }
    public int RestTimeSec { get; set; }

    public SessionStatus Status { get; set; } = SessionStatus.active;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public User User { get; set; } = null!;
    public ICollection<ActivityRecord> ActivityRecords { get; set; } = new List<ActivityRecord>();
}

public sealed class ActivityRecord
{
    public Guid ActivityId { get; set; }
    public Guid SessionId { get; set; }

    public ActivitySourceType SourceType { get; set; } = ActivitySourceType.browser;

    [MaxLength(255)]
    public string? SourceName { get; set; }

    [MaxLength(255)]
    public string Domain { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset EndedAt { get; set; }
    public int DurationSec { get; set; }
    public ActivityVerdict Verdict { get; set; } = ActivityVerdict.unknown;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public WorkSession Session { get; set; } = null!;
    public Violation? Violation { get; set; }
}

public sealed class Violation
{
    public Guid ViolationId { get; set; }
    public Guid ActivityId { get; set; }
    public string ScreenshotPath { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Reason { get; set; } = string.Empty;

    public bool IsDisputed { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ActivityRecord Activity { get; set; } = null!;
}

public sealed class DailyReport
{
    public Guid ReportId { get; set; }
    public Guid UserId { get; set; }
    public DateOnly ReportDate { get; set; }
    public decimal WorkPercent { get; set; }
    public decimal RestPercent { get; set; }
    public ReportStatusColor StatusColor { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public User User { get; set; } = null!;
}

public sealed class RefreshToken
{
    public Guid TokenId { get; set; }
    public Guid UserId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? RevokedAt { get; set; }

    public User User { get; set; } = null!;
}