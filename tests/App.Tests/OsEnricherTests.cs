using Serilog.Core;
using Serilog.Events;

namespace App.Tests;

public class OsEnricherTests
{
    [Fact]
    public void Enrich_AddsOsProperty()
    {
        var enricher = new OsEnricher();
        var logEvent = CreateLogEvent();

        enricher.Enrich(logEvent, new FakePropertyFactory());

        Assert.True(logEvent.Properties.ContainsKey("OS"));
    }

    [Fact]
    public void Enrich_OsPropertyIsNonEmpty()
    {
        var enricher = new OsEnricher();
        var logEvent = CreateLogEvent();

        enricher.Enrich(logEvent, new FakePropertyFactory());

        var osValue = Assert.IsType<ScalarValue>(logEvent.Properties["OS"]);
        Assert.NotNull(osValue.Value);
        Assert.NotEqual(string.Empty, osValue.Value?.ToString());
    }

    [Fact]
    public void Enrich_OsPropertyIsKnownPlatform()
    {
        var enricher = new OsEnricher();
        var logEvent = CreateLogEvent();

        enricher.Enrich(logEvent, new FakePropertyFactory());

        var osValue = Assert.IsType<ScalarValue>(logEvent.Properties["OS"]);
        var validOs = new[] { "Windows", "Linux", "macOS" };
        Assert.Contains(osValue.Value?.ToString(), validOs);
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
