using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace App;

public class LogCommands
{
    private readonly ILogger<LogCommands> _logger;
    private readonly LogMessageValidator _validator;

    public LogCommands(ILogger<LogCommands> logger)
    {
        _logger = logger;
        _validator = new LogMessageValidator();
    }

    [Command("log", Description = "Process a JSON log message from legacy applications")]
    public async Task<int> LogAsync(
        [Argument(Description = "JSON payload containing the log message")] string jsonPayload)
    {
        try
        {
            // Parse JSON payload
            LogMessage? logMessage;
            try
            {
                logMessage = JsonSerializer.Deserialize<LogMessage>(jsonPayload, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    AllowTrailingCommas = true
                });
            }
            catch (JsonException ex)
            {
                await Console.Error.WriteLineAsync($"Invalid JSON format: {ex.Message}");
                return LogErrorCodes.InvalidJson;
            }

            if (logMessage == null)
            {
                await Console.Error.WriteLineAsync("JSON payload resulted in null object");
                return LogErrorCodes.InvalidJson;
            }

            // Validate the log message
            var validationResult = await _validator.ValidateAsync(logMessage);
            if (!validationResult.IsValid)
            {
                var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                await Console.Error.WriteLineAsync($"Validation failed: {errors}");
                return LogErrorCodes.ValidationFailed;
            }

            // Convert timestamp to UTC if provided, otherwise use current UTC time
            var timestamp = logMessage.Timestamp?.ToUniversalTime() ?? DateTime.UtcNow;

            // Create structured log entry with all properties
            var logLevel = ParseLogLevel(logMessage.Level!);

            // Create a dictionary with context properties for scoping
            var scopeProperties = new Dictionary<string, object>
            {
                ["Application"] = logMessage.Application!,
                ["Source"] = logMessage.Source ?? "Unknown",
                ["OriginalTimestamp"] = timestamp
            };

            // Convert JsonElement properties to proper values and add to scope
            Dictionary<string, object>? convertedProperties = null;
            if (logMessage.Properties != null)
            {
                convertedProperties = new Dictionary<string, object>();
                foreach (var prop in logMessage.Properties)
                {
                    var value = ConvertJsonElementToValue(prop.Value);
                    scopeProperties[prop.Key] = value;
                    convertedProperties[prop.Key] = value;
                }
            }

            using var scope = _logger.BeginScope(scopeProperties);

            // Log the message with custom properties as structured data
            if (convertedProperties != null && convertedProperties.Any())
            {
                _logger.Log(logLevel, "Legacy Log: {Message} {@CustomProperties}",
                    logMessage.Message, convertedProperties);
            }
            else
            {
                _logger.Log(logLevel, "Legacy Log: {Message}", logMessage.Message);
            }

            return LogErrorCodes.Success;
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Unexpected error: {ex.Message}");
            return LogErrorCodes.UnexpectedError;
        }
    }

    private static object ConvertJsonElementToValue(object value)
    {
        if (value is JsonElement jsonElement)
        {
            return jsonElement.ValueKind switch
            {
                JsonValueKind.String => jsonElement.GetString() ?? "",
                JsonValueKind.Number => jsonElement.TryGetInt64(out var longVal) ? longVal : jsonElement.GetDouble(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => null!,
                _ => jsonElement.ToString()
            };
        }
        return value;
    }

    private static LogLevel ParseLogLevel(string level)
    {
        return level.ToLowerInvariant() switch
        {
            "debug" => LogLevel.Debug,
            "information" => LogLevel.Information,
            "warning" => LogLevel.Warning,
            "error" => LogLevel.Error,
            "critical" => LogLevel.Critical,
            _ => LogLevel.Information // Default fallback
        };
    }
}