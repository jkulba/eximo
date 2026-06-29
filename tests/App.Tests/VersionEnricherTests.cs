using Serilog.Core;
using Serilog.Events;

namespace App.Tests;

public class VersionEnricherTests
{
    [Fact]
    public void Enrich_AddsVersionProperty()
    {
        var enricher = new VersionEnricher();
        var logEvent = CreateLogEvent();

        enricher.Enrich(logEvent, new FakePropertyFactory());

        Assert.True(logEvent.Properties.ContainsKey("Version"));
    }

    [Fact]
    public void Enrich_VersionPropertyIsNonEmpty()
    {
        var enricher = new VersionEnricher();
        var logEvent = CreateLogEvent();

        enricher.Enrich(logEvent, new FakePropertyFactory());

        var versionValue = Assert.IsType<ScalarValue>(logEvent.Properties["Version"]);
        Assert.NotNull(versionValue.Value);
        Assert.NotEqual(string.Empty, versionValue.Value?.ToString());
    }

    private static LogEvent CreateLogEvent() =>
        new(DateTimeOffset.UtcNow, LogEventLevel.Information, exception: null,
            new MessageTemplate([]), properties: []);

    private sealed class FakePropertyFactory : ILogEventPropertyFactory
    {
        public LogEventProperty CreateProperty(string name, object? value, bool destructureObjects = false)
            => new(name, new ScalarValue(value));
    }
}
