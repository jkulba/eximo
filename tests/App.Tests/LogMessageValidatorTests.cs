namespace App.Tests;

public class LogMessageValidatorTests
{
    private readonly LogMessageValidator _validator = new();

    private static LogMessage ValidMessage() => new()
    {
        Message = "Test message",
        Level = "Information",
        Application = "TestApp",
        Source = "TestSource",
        Timestamp = DateTime.UtcNow
    };

    [Fact]
    public async Task Validate_ValidMessage_ReturnsSuccess()
    {
        var result = await _validator.ValidateAsync(ValidMessage(), TestContext.Current.CancellationToken);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_NullMessage_ReturnsFailure()
    {
        var msg = ValidMessage();
        msg.Message = null;

        var result = await _validator.ValidateAsync(msg, TestContext.Current.CancellationToken);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Validate_EmptyMessage_ReturnsFailure()
    {
        var msg = ValidMessage();
        msg.Message = string.Empty;

        var result = await _validator.ValidateAsync(msg, TestContext.Current.CancellationToken);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Validate_MessageAtMaxLength_ReturnsSuccess()
    {
        var msg = ValidMessage();
        msg.Message = new string('x', 10000);

        var result = await _validator.ValidateAsync(msg, TestContext.Current.CancellationToken);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_MessageExceedingMaxLength_ReturnsFailure()
    {
        var msg = ValidMessage();
        msg.Message = new string('x', 10001);

        var result = await _validator.ValidateAsync(msg, TestContext.Current.CancellationToken);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Validate_NullLevel_ReturnsFailure()
    {
        var msg = ValidMessage();
        msg.Level = null;

        var result = await _validator.ValidateAsync(msg, TestContext.Current.CancellationToken);

        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData("Debug")]
    [InlineData("Information")]
    [InlineData("Warning")]
    [InlineData("Error")]
    [InlineData("Critical")]
    public async Task Validate_ValidLevel_ReturnsSuccess(string level)
    {
        var msg = ValidMessage();
        msg.Level = level;

        var result = await _validator.ValidateAsync(msg, TestContext.Current.CancellationToken);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("debug")]
    [InlineData("information")]
    [InlineData("WARNING")]
    [InlineData("ERROR")]
    public async Task Validate_ValidLevelCaseInsensitive_ReturnsSuccess(string level)
    {
        var msg = ValidMessage();
        msg.Level = level;

        var result = await _validator.ValidateAsync(msg, TestContext.Current.CancellationToken);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_InvalidLevel_ReturnsFailure()
    {
        var msg = ValidMessage();
        msg.Level = "Verbose";

        var result = await _validator.ValidateAsync(msg, TestContext.Current.CancellationToken);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Validate_NullApplication_ReturnsFailure()
    {
        var msg = ValidMessage();
        msg.Application = null;

        var result = await _validator.ValidateAsync(msg, TestContext.Current.CancellationToken);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Validate_ApplicationAtMaxLength_ReturnsSuccess()
    {
        var msg = ValidMessage();
        msg.Application = new string('a', 100);

        var result = await _validator.ValidateAsync(msg, TestContext.Current.CancellationToken);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_ApplicationExceedingMaxLength_ReturnsFailure()
    {
        var msg = ValidMessage();
        msg.Application = new string('a', 101);

        var result = await _validator.ValidateAsync(msg, TestContext.Current.CancellationToken);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Validate_NullSource_ReturnsSuccess()
    {
        var msg = ValidMessage();
        msg.Source = null;

        var result = await _validator.ValidateAsync(msg, TestContext.Current.CancellationToken);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_SourceAtMaxLength_ReturnsSuccess()
    {
        var msg = ValidMessage();
        msg.Source = new string('s', 200);

        var result = await _validator.ValidateAsync(msg, TestContext.Current.CancellationToken);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_SourceExceedingMaxLength_ReturnsFailure()
    {
        var msg = ValidMessage();
        msg.Source = new string('s', 201);

        var result = await _validator.ValidateAsync(msg, TestContext.Current.CancellationToken);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Validate_NullTimestamp_ReturnsSuccess()
    {
        var msg = ValidMessage();
        msg.Timestamp = null;

        var result = await _validator.ValidateAsync(msg, TestContext.Current.CancellationToken);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_PastTimestamp_ReturnsSuccess()
    {
        var msg = ValidMessage();
        msg.Timestamp = DateTime.UtcNow.AddDays(-1);

        var result = await _validator.ValidateAsync(msg, TestContext.Current.CancellationToken);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_TimestampMoreThanFiveMinutesInFuture_ReturnsFailure()
    {
        var msg = ValidMessage();
        msg.Timestamp = DateTime.UtcNow.AddMinutes(6);

        var result = await _validator.ValidateAsync(msg, TestContext.Current.CancellationToken);

        Assert.False(result.IsValid);
    }
}
