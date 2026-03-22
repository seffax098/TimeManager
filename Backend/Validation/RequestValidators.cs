using FluentValidation;
using Backend.Contracts;
using Backend.Models;

namespace Backend.Validation;

public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Login).NotEmpty().MinimumLength(3).MaximumLength(32);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6).MaximumLength(64);
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(100);
    }
}

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Login).NotEmpty();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public sealed class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator()
    {
        When(x => x.FullName is not null, () =>
        {
            RuleFor(x => x.FullName!).MaximumLength(100);
        });

        When(x => x.TechStack is not null, () =>
        {
            RuleForEach(x => x.TechStack!).ChildRules(stack =>
            {
                stack.RuleFor(s => s.Name).NotEmpty().MaximumLength(100);
                stack.RuleFor(s => s.Position).GreaterThanOrEqualTo(0);
            });
        });

        When(x => x.Settings is not null && x.Settings.WorkTime is not null, () =>
        {
            RuleFor(x => x.Settings!.WorkTime!)
                .Must(value => TimeSpan.TryParse(value, out _))
                .WithMessage("settings.workTime must be a valid time span, for example 08:30:00.");
        });

        When(x => x.Settings is not null && x.Settings.Theme is not null, () =>
        {
            RuleFor(x => x.Settings!.Theme!)
                .Must(value =>
                {
                    var normalized = value.Trim().ToLowerInvariant();
                    return normalized is "light" or "dark";
                })
                .WithMessage("settings.theme must be 'light' or 'dark'.");
        });
    }
}

public sealed class StartTimerRequestValidator : AbstractValidator<StartTimerRequest>
{
    public StartTimerRequestValidator()
    {
    }
}

public sealed class StopTimerRequestValidator : AbstractValidator<StopTimerRequest>
{
    public StopTimerRequestValidator()
    {
        RuleFor(x => x.SessionId).NotEmpty();
    }
}

public sealed class ActivityRequestValidator : AbstractValidator<ActivityRequest>
{
    public ActivityRequestValidator()
    {
        RuleFor(x => x.SessionId).NotEmpty();
        RuleFor(x => x.Domain).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Url).NotEmpty();
        RuleFor(x => x.DurationSec).GreaterThanOrEqualTo(0);

        RuleFor(x => x.EndedAt)
            .GreaterThanOrEqualTo(x => x.StartedAt)
            .WithMessage("endedAt must be greater than or equal to startedAt.");

        When(x => x.Verdict.HasValue, () =>
        {
            RuleFor(x => x.Verdict!.Value).IsInEnum();
        });

        When(x => x.SourceType.HasValue, () =>
        {
            RuleFor(x => x.SourceType!.Value).IsInEnum();
        });

        When(x => x.SourceName is not null, () =>
        {
            RuleFor(x => x.SourceName!).MaximumLength(255);
        });
    }
}

public sealed class UpdateActivityVerdictRequestValidator : AbstractValidator<UpdateActivityVerdictRequest>
{
    public UpdateActivityVerdictRequestValidator()
    {
        RuleFor(x => x.Verdict)
            .Must(verdict => verdict is ActivityVerdict.work or ActivityVerdict.rest)
            .WithMessage("verdict must be 'work' or 'rest'.");
    }
}

public sealed class CreateViolationRequestValidator : AbstractValidator<CreateViolationRequest>
{
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];

    public CreateViolationRequestValidator()
    {
        RuleFor(x => x.ActivityId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Screenshot)
            .NotNull()
            .Must(file => file is not null && file.Length > 0)
            .WithMessage("Screenshot is required.")
            .Must(file => file is not null && file.Length <= 5 * 1024 * 1024)
            .WithMessage("Screenshot size must be 5MB or less.")
            .Must(file => file is not null && AllowedExtensions.Contains(Path.GetExtension(file.FileName).ToLowerInvariant()))
            .WithMessage("Allowed screenshot formats: jpg, jpeg, png, webp.");
    }
}