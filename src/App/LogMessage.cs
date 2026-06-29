using Microsoft.Extensions.Logging;

namespace App;

/// <summary>
/// Model representing a log message from legacy applications
/// </summary>
public class LogMessage
{
    public string? Level { get; set; }
    public string? Message { get; set; }
    public string? Application { get; set; }
    public string? Source { get; set; }
    public DateTime? Timestamp { get; set; }
    public Dictionary<string, object>? Properties { get; set; }
}

/// <summary>
/// Validator for LogMessage using FluentValidation
/// </summary>
public class LogMessageValidator : AbstractValidator<LogMessage>
{
    public LogMessageValidator()
    {
        RuleFor(x => x.Message)
            .NotEmpty()
            .WithMessage("Message is required")
            .MaximumLength(10000)
            .WithMessage("Message cannot exceed 10000 characters");

        RuleFor(x => x.Level)
            .NotEmpty()
            .WithMessage("Level is required")
            .Must(BeValidLogLevel)
            .WithMessage("Level must be one of: Debug, Information, Warning, Error, Critical");

        RuleFor(x => x.Application)
            .NotEmpty()
            .WithMessage("Application is required")
            .MaximumLength(100)
            .WithMessage("Application name cannot exceed 100 characters");

        RuleFor(x => x.Source)
            .MaximumLength(200)
            .WithMessage("Source cannot exceed 200 characters");

        RuleFor(x => x.Timestamp)
            .LessThanOrEqualTo(DateTime.UtcNow.AddMinutes(5))
            .WithMessage("Timestamp cannot be more than 5 minutes in the future");
    }

    private static bool BeValidLogLevel(string? level)
    {
        if (string.IsNullOrEmpty(level))
            return false;

        var validLevels = new[] { "Debug", "Information", "Warning", "Error", "Critical" };
        return validLevels.Contains(level, StringComparer.OrdinalIgnoreCase);
    }
}

/// <summary>
/// Error codes for the log command
/// </summary>
public static class LogErrorCodes
{
    public const int Success = 0;
    public const int InvalidJson = 1;
    public const int ValidationFailed = 2;
    public const int UnexpectedError = 99;
}