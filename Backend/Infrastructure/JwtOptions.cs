namespace Backend.Infrastructure;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "SmartTimeManager";
    public string Audience { get; set; } = "SmartTimeManager.Client";
    public string SecretKey { get; set; } = "replace-this-with-a-long-random-secret-key";
    public int AccessTokenMinutes { get; set; } = 1440;
    public int RefreshTokenDays { get; set; } = 30;
}
