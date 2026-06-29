using Serilog.Core;
using Serilog.Events;
using System;
using System.IO;
using System.Text.Json;

namespace App;

public class VersionEnricher : ILogEventEnricher
{
    private static readonly string Version = LoadVersion();

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("Version", Version));
    }

    private static string LoadVersion()
    {
        try
        {
            var path = Path.Combine(AppContext.BaseDirectory, "version.json");
            if (!File.Exists(path))
                return "unknown";

            using var stream = File.OpenRead(path);
            using var doc = JsonDocument.Parse(stream);
            if (doc.RootElement.TryGetProperty("BuildNumber", out var versionProp))
                return versionProp.GetString() ?? "unknown";
        }
        catch
        {
            // Ignore errors and return "unknown"
        }
        return "unknown";
    }
}