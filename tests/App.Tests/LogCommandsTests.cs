using Microsoft.Extensions.Logging.Abstractions;

namespace App.Tests;

public class LogCommandsTests
{
    private readonly LogCommands _sut = new(NullLogger<LogCommands>.Instance);

    [Fact]
    public async Task LogAsync_ValidJsonPayload_ReturnsSuccess()
    {
        var json = """{"level":"Information","message":"Test message","application":"TestApp"}""";

        var result = await _sut.LogAsync(json);

        Assert.Equal(LogErrorCodes.Success, result);
    }

    [Fact]
    public async Task LogAsync_InvalidJson_ReturnsInvalidJsonCode()
    {
        var result = await _sut.LogAsync("not-valid-json");

        Assert.Equal(LogErrorCodes.InvalidJson, result);
    }

    [Fact]
    public async Task LogAsync_MissingRequiredFields_ReturnsValidationFailedCode()
    {
        var json = """{"level":"Information"}""";

        var result = await _sut.LogAsync(json);

        Assert.Equal(LogErrorCodes.ValidationFailed, result);
    }

    [Fact]
    public async Task LogAsync_InvalidLevel_ReturnsValidationFailedCode()
    {
        var json = """{"level":"Verbose","message":"Test","application":"App"}""";

        var result = await _sut.LogAsync(json);

        Assert.Equal(LogErrorCodes.ValidationFailed, result);
    }

    [Fact]
    public async Task LogAsync_WithCustomProperties_ReturnsSuccess()
    {
        var json = """{"level":"Error","message":"Failure occurred","application":"TestApp","properties":{"requestId":"abc-123","userId":42,"retried":true}}""";

        var result = await _sut.LogAsync(json);

        Assert.Equal(LogErrorCodes.Success, result);
    }

    [Theory]
    [InlineData("Debug")]
    [InlineData("Information")]
    [InlineData("Warning")]
    [InlineData("Error")]
    [InlineData("Critical")]
    public async Task LogAsync_AllValidLogLevels_ReturnsSuccess(string level)
    {
        var json = $$"""{"level":"{{level}}","message":"Test","application":"TestApp"}""";

        var result = await _sut.LogAsync(json);

        Assert.Equal(LogErrorCodes.Success, result);
    }

    [Fact]
    public async Task LogAsync_WithSourceAndTimestamp_ReturnsSuccess()
    {
        var json = """{"level":"Warning","message":"Something happened","application":"TestApp","source":"TestSource","timestamp":"2025-01-01T12:00:00Z"}""";

        var result = await _sut.LogAsync(json);

        Assert.Equal(LogErrorCodes.Success, result);
    }
}
