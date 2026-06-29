using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Settings.Configuration;

var builder = CoconaApp.CreateBuilder();

// Set the base path to the directory containing the executable
var exeDir = AppContext.BaseDirectory;
builder.Configuration.SetBasePath(exeDir)
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

builder.Host.UseSerilog((hostContext, loggerConfiguration) =>
{
    var options = new ConfigurationReaderOptions(
        typeof(ConsoleLoggerConfigurationExtensions).Assembly,
        typeof(FileLoggerConfigurationExtensions).Assembly,
        typeof(EnvironmentLoggerConfigurationExtensions).Assembly,
        typeof(ThreadLoggerConfigurationExtensions).Assembly
    );
    loggerConfiguration.ReadFrom.Configuration(hostContext.Configuration, options)
    .Enrich.With(new DotNetRuntimesEnricher())
    .Enrich.With(new OsEnricher())
    .Enrich.With(new VersionEnricher());
});

var app = builder.Build();

app.AddCommands<LogCommands>();

app.Run();