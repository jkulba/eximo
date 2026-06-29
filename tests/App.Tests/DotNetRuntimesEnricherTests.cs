using Serilog.Core;
using Serilog.Events;

namespace App.Tests;

public class DotNetRuntimesEnricherTests
{
    [Fact]
    public void Enrich_AddsDotNetProperty()
    {
        var enricher = new DotNetRuntimesEnricher();
        var logEvent = CreateLogEvent();

        enricher.Enrich(logEvent, new FakePropertyFactory());

        Assert.True(logEvent.Properties.ContainsKey("DotNet"));
    }

    [Fact]
    public void Enrich_DotNetPropertyIsSequenceValue()
    {
        var enricher = new DotNetRuntimesEnricher();
        var logEvent = CreateLogEvent();

        enricher.Enrich(logEvent, new FakePropertyFactory());

        Assert.IsType<SequenceValue>(logEvent.Properties["DotNet"]);
    }

    private static LogEvent CreateLogEvent() =>
        new(DateTimeOffset.UtcNow, LogEventLevel.Information, exception: null,
            new MessageTemplate([]), properties: []);

    private sealed class FakePropertyFactory : ILogEventPropertyFactory
    {
        public LogEventProperty CreateProperty(string name, object? value, bool destructureObjects = false)
        {
            LogEventPropertyValue logEventValue = value switch
            {
                IEnumerable<LogEventPropertyValue> seq => new SequenceValue(seq),
                IEnumerable<string> strings => new SequenceValue(strings.Select(s => (LogEventPropertyValue)new ScalarValue(s))),
                _ => new ScalarValue(value)
            };
            return new LogEventProperty(name, logEventValue);
        }
    }
}
