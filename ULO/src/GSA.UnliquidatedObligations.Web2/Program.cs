using RevolutionaryStuff.Core;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;

namespace GSA.UnliquidatedObligations.Web
{
    public class Program
    {
        public static class GsaEnvironmentVariableNames
        {
            public const string AppsettingsDirectory = "APPSETTINGS_DIRECTORY";
            public const string LogfileDirectory = "LOGFILE_DIRECTORY";
        }

        private static void ConfigureConfiguration(WebHostBuilderContext hostingContext, IConfigurationBuilder builder)
        {
            builder.AddEnvironmentVariables();

            builder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            builder.AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true);

            var c = builder.Build();
            var appSettingsDirectory = c[GsaEnvironmentVariableNames.AppsettingsDirectory];
            var appName = c["APP_NAME"];

            if (!string.IsNullOrEmpty(appName))
            {
                builder.AddJsonFile($"{appSettingsDirectory}{appName}_appsettings.json", optional: true, reloadOnChange: true);
            }

            if (hostingContext.HostingEnvironment.IsDevelopment())
            {
                builder.AddUserSecrets<Program>();
            }
        }

        public static void Main(string[] args)
            => CreateWebHostBuilder(args).Build().Run();

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration(ConfigureConfiguration)
            .ConfigureLogging((hostingContext, logging) =>
            {
                var c = hostingContext.Configuration;
                var logLevel = Parse.ParseEnum<Serilog.Events.LogEventLevel>(c["Logging:LogLevel:Default"], Serilog.Events.LogEventLevel.Warning);
                var loggerConfiguration = new LoggerConfiguration()
                    .ReadFrom.Configuration(c)
                    .MinimumLevel.Is(logLevel)
                    .Enrich.With<Serilog.Enrichers.MachineNameEnricher>()
                    .Enrich.With<Serilog.Enrichers.ProcessIdEnricher>()
                    .Enrich.With<Serilog.Enrichers.ThreadIdEnricher>()
                    .Enrich.WithProperty("ApplicationName", c["ApplicationName"])
                    .Enrich.WithProperty("SprintName", c["SprintConfig:SprintName"])
                    .Enrich.FromLogContext()
                    .WriteTo.Trace();
                var connectionString = c.GetConnectionString(c["SerilogSqlServerSink:ConnectionStringName"]);
                if (!string.IsNullOrEmpty(connectionString))
                {
                    loggerConfiguration.WriteTo.MSSqlServer(connectionString, c["SerilogSqlServerSink:TableName"], schemaName: c["SerilogSqlServerSink:SchemaName"]);
                }
                var appName = c["APP_NAME"];
                var logDirectory = c[GsaEnvironmentVariableNames.LogfileDirectory];
                if (!string.IsNullOrEmpty(logDirectory) && !string.IsNullOrEmpty(appName))
                {
                    loggerConfiguration.WriteTo.RollingFile($"{logDirectory}{appName}.log", logLevel, flushToDiskInterval: TimeSpan.FromSeconds(5));
                }
                Log.Logger = loggerConfiguration.CreateLogger();
                logging.AddSerilog();
                Log.Information(
                    "Logging Initialized for APP_NAME=[{appName}] to {LOGFILE_DIRECTORY}=[{logDirectory}] from {APPSETTINGS_DIRECTORY}=[{appSettingsDirectory}]",
                    appName,
                    GsaEnvironmentVariableNames.LogfileDirectory, logDirectory,
                    GsaEnvironmentVariableNames.AppsettingsDirectory, c[GsaEnvironmentVariableNames.AppsettingsDirectory]
                    );
            })
           .UseStartup<Startup>();
    }
}
