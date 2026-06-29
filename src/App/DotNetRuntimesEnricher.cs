using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace App;

public class DotNetRuntimesEnricher : ILogEventEnricher
{
    private static readonly IReadOnlyList<string> DotNetVersions = GetDotNetVersions();

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        logEvent.AddPropertyIfAbsent(
            propertyFactory.CreateProperty("DotNet", DotNetVersions)
        );
    }

    private static IReadOnlyList<string> GetDotNetVersions()
    {
        var runtimeDirs = new List<string>();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // System-wide
            runtimeDirs.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "dotnet", "shared"));
            // User-specific
            var userDotnet = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".dotnet", "shared");
            if (Directory.Exists(userDotnet))
                runtimeDirs.Add(userDotnet);
        }
        else
        {
            // Linux/macOS system-wide
            runtimeDirs.Add("/usr/share/dotnet/shared");
            // User-specific
            var userDotnet = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".dotnet", "shared");
            if (Directory.Exists(userDotnet))
                runtimeDirs.Add(userDotnet);
            // If running as root, also check /root/.dotnet/shared
            if (Environment.UserName == "root" && Directory.Exists("/root/.dotnet/shared"))
                runtimeDirs.Add("/root/.dotnet/shared");
        }

        var versions = new HashSet<string>();
        foreach (var dir in runtimeDirs)
        {
            if (!Directory.Exists(dir))
                continue;

            foreach (var runtime in Directory.GetDirectories(dir))
            {
                var runtimeName = Path.GetFileName(runtime);
                foreach (var version in Directory.GetDirectories(runtime))
                {
                    var versionName = Path.GetFileName(version);
                    versions.Add($"{runtimeName}-{versionName}");
                }
            }
        }

        return versions.OrderBy(v => v).ToList();
    }
}