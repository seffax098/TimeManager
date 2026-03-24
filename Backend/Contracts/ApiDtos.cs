using Microsoft.AspNetCore.Http;
using Backend.Models;

namespace Backend.Contracts;

public sealed record RegisterRequest(string Login, string Password, string FullName);

public sealed record UserDto(Guid UserId, string Login, string FullName, UserRole Role, DateTimeOffset CreatedAt);

public sealed record RegisterResponse(Guid UserId, string Login, string FullName, UserRole Role, DateTimeOffset CreatedAt);

public sealed record LoginRequest(string Login, string Password);

public sealed record LoginResponse(
    string AccessToken,
    string RefreshToken,
    string TokenType,
    int ExpiresIn,
    UserDto User);

public sealed record TechStackItemDto(Guid ItemId, string Name, int Position);

public sealed record SettingsDto(string WorkTime, string Theme);

public sealed record ProfileResponse(
    Guid UserId,
    string Login,
    string FullName,
    UserRole Role,
    IReadOnlyList<TechStackItemDto> TechStack,
    SettingsDto Settings,
    DateTimeOffset CreatedAt);

public sealed record UpdateProfileRequest(
    string? FullName,
    IReadOnlyList<UpdateTechStackItemRequest>? TechStack,
    UpdateSettingsRequest? Settings);

public sealed record UpdateTechStackItemRequest(string Name, int Position);

public sealed record UpdateSettingsRequest(string? WorkTime, string? Theme);

public sealed record UpdateProfileResponse(string Message, ProfileResponse User);

public sealed record StartTimerRequest(DateOnly? Date);

public sealed record StartTimerResponse(Guid SessionId, Guid UserId, DateOnly Date, DateTimeOffset StartedAt, SessionStatus Status);

public sealed record StopTimerRequest(Guid SessionId);

public sealed record StopTimerResponse(
    Guid SessionId,
    DateTimeOffset StartedAt,
    DateTimeOffset EndedAt,
    int TotalSeconds,
    int WorkTime,
    int RestTime,
    SessionStatus Status);

public sealed record ActivityRequest(
    Guid SessionId,
    string Domain,
    string Url,
    DateTimeOffset StartedAt,
    DateTimeOffset EndedAt,
    int DurationSec,
    ActivityVerdict? Verdict = null,
    ActivitySourceType? SourceType = null,
    string? SourceName = null);

public sealed record ActivityResponse(
    Guid ActivityId,
    Guid SessionId,
    string Domain,
    string Url,
    int DurationSec,
    ActivityVerdict Verdict,
    DateTimeOffset CreatedAt,
    ActivitySourceType SourceType,
    string? SourceName);

public sealed record UpdateActivityVerdictRequest(ActivityVerdict Verdict);

public sealed record UpdateActivityVerdictResponse(
    Guid ActivityId,
    Guid SessionId,
    ActivityVerdict PreviousVerdict,
    ActivityVerdict Verdict,
    int WorkTime,
    int RestTime);

public sealed class CreateViolationRequest
{
    public Guid ActivityId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public IFormFile Screenshot { get; set; } = null!;
}

public sealed record ViolationResponse(
    Guid ViolationId,
    Guid ActivityId,
    string Reason,
    string ScreenshotPath,
    bool IsDisputed,
    DateTimeOffset CreatedAt);

public sealed record SessionInfoDto(Guid SessionId, DateTimeOffset StartedAt, DateTimeOffset? EndedAt, int TotalSeconds, bool IsActive);

public sealed record ReportSummaryDto(decimal WorkPercent, decimal RestPercent, int WorkTimeSec, int RestTimeSec);

public sealed record ReportLinkDto(Guid ActivityId, string Url, int Minutes, ActivityVerdict Verdict);

public sealed record ReportSiteDto(string Domain, int TotalMinutes, ActivityVerdict Verdict, IReadOnlyList<ReportLinkDto> Links);

public sealed record DayReportResponse(
    Guid UserId,
    string FullName,
    DateOnly Date,
    SessionInfoDto? Session,
    ReportSummaryDto Summary,
    IReadOnlyList<ReportSiteDto> Sites);

public sealed record AdminEmployeeSummaryDto(
    Guid UserId,
    string FullName,
    decimal WorkPercent,
    decimal RestPercent,
    int TotalMinutes,
    ReportStatusColor StatusColor);

public sealed record AdminEmployeesResponse(DateOnly Date, int TotalEmployees, IReadOnlyList<AdminEmployeeSummaryDto> Employees);

public sealed record AdminEmployeeDetailsResponse(
    Guid UserId,
    string FullName,
    DateOnly Date,
    IReadOnlyList<ViolationResponse> Violations,
    ReportSummaryDto Summary,
    IReadOnlyList<ReportSiteDto> Sites);

public sealed record ActiveTimerSessionResponse(
    Guid SessionId,
    Guid UserId,
    DateOnly Date,
    DateTimeOffset StartedAt,
    SessionStatus Status);

public sealed record UpdateProfileRequestV2(
    string? FullName,
    IReadOnlyList<UpdateTechStackItemRequest>? TechStack,
    UpdateSettingsRequest? Settings);