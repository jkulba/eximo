using System.Runtime.InteropServices;
using Serilog.Core;
using Serilog.Events;

namespace App;

public class OsEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        string os = RuntimeInformation.OSDescription;

        if (OperatingSystem.IsWindows())
        {
            os = "Windows";
        }
        else if (OperatingSystem.IsLinux())
        {
            os = "Linux";
        }
        else if (OperatingSystem.IsMacOS())
        {
            os = "macOS";
        }
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("OS", os));
    }
}