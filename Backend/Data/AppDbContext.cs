using Microsoft.EntityFrameworkCore;
using Backend.Models;

namespace Backend.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<TechStackItem> TechStackItems => Set<TechStackItem>();
    public DbSet<WorkSession> WorkSessions => Set<WorkSession>();
    public DbSet<ActivityRecord> ActivityRecords => Set<ActivityRecord>();
    public DbSet<Violation> Violations => Set<Violation>();
    public DbSet<DailyReport> DailyReports => Set<DailyReport>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresEnum<UserRole>(name: "user_role");
        modelBuilder.HasPostgresEnum<SessionStatus>(name: "session_status");
        modelBuilder.HasPostgresEnum<ActivityVerdict>(name: "activity_verdict");
        modelBuilder.HasPostgresEnum<ReportStatusColor>(name: "report_status_color");
        modelBuilder.HasPostgresEnum<ActivitySourceType>(name: "activity_source_type");

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users", table =>
            {
                table.HasCheckConstraint("chk_users_settings_theme", "(settings->>'theme') in ('light', 'dark')");
                table.HasCheckConstraint("chk_users_settings_has_work_time", "settings ? 'work_time'");
            });

            entity.HasKey(x => x.UserId);
            entity.Property(x => x.UserId).HasColumnName("user_id");
            entity.Property(x => x.Login).HasColumnName("login");
            entity.Property(x => x.PasswordHash).HasColumnName("password_hash");
            entity.Property(x => x.FullName).HasColumnName("full_name");
            entity.Property(x => x.Role).HasColumnName("role").HasColumnType("user_role.user_role");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasIndex(x => x.Login).IsUnique();

            entity.OwnsOne(x => x.Settings, settings =>
            {
                settings.ToJson("settings");
                settings.Property(x => x.Theme).HasColumnName("theme");
                settings.Property(x => x.WorkTime).HasColumnName("work_time");
            });
        });

        modelBuilder.Entity<TechStackItem>(entity =>
        {
            entity.ToTable("tech_stack_items");
            entity.HasKey(x => x.ItemId);
            entity.Property(x => x.ItemId).HasColumnName("item_id");
            entity.Property(x => x.UserId).HasColumnName("user_id");
            entity.Property(x => x.Name).HasColumnName("name");
            entity.Property(x => x.Position).HasColumnName("position");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");

            entity.HasIndex(x => x.UserId).HasDatabaseName("idx_tech_stack_items_user_id");
            entity.HasIndex(x => new { x.UserId, x.Position }).HasDatabaseName("uq_tech_stack_user_position").IsUnique();
            entity.HasIndex(x => new { x.UserId, x.Name }).HasDatabaseName("uq_tech_stack_user_name").IsUnique();

            entity.HasOne(x => x.User)
                .WithMany(x => x.TechStackItems)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<WorkSession>(entity =>
        {
            entity.ToTable("work_sessions", table =>
            {
                table.HasCheckConstraint("chk_work_sessions_time", "ended_at is null or ended_at >= started_at");
                table.HasCheckConstraint("chk_work_sessions_totals", "total_seconds >= work_time + rest_time");
            });

            entity.HasKey(x => x.SessionId);
            entity.Property(x => x.SessionId).HasColumnName("session_id");
            entity.Property(x => x.UserId).HasColumnName("user_id");
            entity.Property(x => x.WorkDate).HasColumnName("work_date");
            entity.Property(x => x.StartedAt).HasColumnName("started_at");
            entity.Property(x => x.EndedAt).HasColumnName("ended_at");
            entity.Property(x => x.TotalSeconds).HasColumnName("total_seconds");
            entity.Property(x => x.WorkTimeSec).HasColumnName("work_time");
            entity.Property(x => x.RestTimeSec).HasColumnName("rest_time");
            entity.Property(x => x.Status).HasColumnName("status").HasColumnType("session_status.session_status");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");

            entity.HasIndex(x => x.UserId).HasDatabaseName("idx_work_sessions_user_id");
            entity.HasIndex(x => new { x.UserId, x.WorkDate }).HasDatabaseName("idx_work_sessions_user_date");
            entity.HasIndex(x => x.Status).HasDatabaseName("idx_work_sessions_status");
            entity.HasIndex(x => x.UserId)
                .HasDatabaseName("uq_work_sessions_one_open_per_user")
                .IsUnique()
                .HasFilter("status in ('active', 'paused')");

            entity.HasOne(x => x.User)
                .WithMany(x => x.WorkSessions)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ActivityRecord>(entity =>
        {
            entity.ToTable("activity_records", table =>
            {
                table.HasCheckConstraint("chk_activity_records_time", "ended_at >= started_at");
            });

            entity.HasKey(x => x.ActivityId);
            entity.Property(x => x.ActivityId).HasColumnName("activity_id");
            entity.Property(x => x.SessionId).HasColumnName("session_id");
            entity.Property(x => x.SourceType).HasColumnName("source_type").HasColumnType("activity_source_type");
            entity.Property(x => x.SourceName).HasColumnName("source_name");
            entity.Property(x => x.Domain).HasColumnName("domain");
            entity.Property(x => x.Url).HasColumnName("url");
            entity.Property(x => x.StartedAt).HasColumnName("started_at");
            entity.Property(x => x.EndedAt).HasColumnName("ended_at");
            entity.Property(x => x.DurationSec).HasColumnName("duration_sec");
            entity.Property(x => x.Verdict).HasColumnName("verdict").HasColumnType("activity_verdict.activity_verdict");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");

            entity.HasIndex(x => x.SessionId).HasDatabaseName("idx_activity_records_session_id");
            entity.HasIndex(x => new { x.SessionId, x.StartedAt }).HasDatabaseName("idx_activity_records_session_started_at");
            entity.HasIndex(x => x.Domain).HasDatabaseName("idx_activity_records_domain");
            entity.HasIndex(x => x.Verdict).HasDatabaseName("idx_activity_records_verdict");
            entity.HasIndex(x => x.SourceType).HasDatabaseName("idx_activity_records_source_type");
            entity.HasIndex(x => x.StartedAt).HasDatabaseName("idx_activity_records_started_at");

            entity.HasOne(x => x.Session)
                .WithMany(x => x.ActivityRecords)
                .HasForeignKey(x => x.SessionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Violation>(entity =>
        {
            entity.ToTable("violations");
            entity.HasKey(x => x.ViolationId);
            entity.Property(x => x.ViolationId).HasColumnName("violation_id");
            entity.Property(x => x.ActivityId).HasColumnName("activity_id");
            entity.Property(x => x.ScreenshotPath).HasColumnName("screenshot_path");
            entity.Property(x => x.Reason).HasColumnName("reason");
            entity.Property(x => x.IsDisputed).HasColumnName("is_disputed");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.HasIndex(x => x.ActivityId).IsUnique();
            entity.HasIndex(x => x.CreatedAt).HasDatabaseName("idx_violations_created_at");

            entity.HasOne(x => x.Activity)
                .WithOne(x => x.Violation)
                .HasForeignKey<Violation>(x => x.ActivityId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DailyReport>(entity =>
        {
            entity.ToTable("daily_reports", table =>
            {
                table.HasCheckConstraint(
                    "chk_daily_reports_total",
                    "(work_percent = 0 and rest_percent = 0) or abs((work_percent + rest_percent) - 100) <= 0.01");
            });

            entity.HasKey(x => x.ReportId);
            entity.Property(x => x.ReportId).HasColumnName("report_id");
            entity.Property(x => x.UserId).HasColumnName("user_id");
            entity.Property(x => x.ReportDate).HasColumnName("report_date");
            entity.Property(x => x.WorkPercent).HasColumnName("work_percent").HasColumnType("numeric(5,2)");
            entity.Property(x => x.RestPercent).HasColumnName("rest_percent").HasColumnType("numeric(5,2)");
            entity.Property(x => x.StatusColor).HasColumnName("status_color").HasColumnType("report_status_color.report_status_color");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");

            entity.HasIndex(x => x.UserId).HasDatabaseName("idx_daily_reports_user_id");
            entity.HasIndex(x => x.ReportDate).HasDatabaseName("idx_daily_reports_report_date");
            entity.HasIndex(x => new { x.UserId, x.ReportDate }).HasDatabaseName("uq_daily_reports_user_date").IsUnique();

            entity.HasOne(x => x.User)
                .WithMany(x => x.DailyReports)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("refresh_tokens", table =>
            {
                table.HasCheckConstraint("chk_refresh_tokens_dates", "expires_at > created_at");
            });

            entity.HasKey(x => x.TokenId);
            entity.Property(x => x.TokenId).HasColumnName("token_id");
            entity.Property(x => x.UserId).HasColumnName("user_id");
            entity.Property(x => x.TokenHash).HasColumnName("token_hash");
            entity.Property(x => x.ExpiresAt).HasColumnName("expires_at");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.RevokedAt).HasColumnName("revoked_at");

            entity.HasIndex(x => x.UserId).HasDatabaseName("idx_refresh_tokens_user_id");
            entity.HasIndex(x => x.ExpiresAt).HasDatabaseName("idx_refresh_tokens_expires_at");
            entity.HasIndex(x => x.TokenHash).IsUnique();

            entity.HasOne(x => x.User)
                .WithMany(x => x.RefreshTokens)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}