using System;
using System.Collections.Generic;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using RevolutionaryStuff.Core;
using Serilog;

namespace GSA.UnliquidatedObligations.Web
{
    public class Program
    {
        public static readonly IDictionary<string, object> EnvironmentInfo = new Dictionary<string, object>();

        internal static class GsaEnvironmentVariableNames
        {
            public const string AppsettingsDirectory = "APPSETTINGS_DIRECTORY";
            public const string LogfileDirectory = "LOGFILE_DIRECTORY";
        }

        internal static class GsaAppSettingsVariablePaths
        {
            private const string Prefix = "GSAIT:";
            public const string AppPathBase = Prefix+"AppPathBase";
            public const string AppSetttingsOptional = Prefix + "AppSettingsOptional";
            public const string AppName = Prefix + "AppName";
        }

        private static void ConfigureConfiguration(WebHostBuilderContext hostingContext, IConfigurationBuilder builder)
        {
            builder.AddEnvironmentVariables();

            builder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            EnvironmentInfo["hostingContext.HostingEnvironment.EnvironmentName"] = hostingContext.HostingEnvironment.EnvironmentName;
            builder.AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true);

            var c = builder.Build();
            var appSettingsDirectory = c[GsaEnvironmentVariableNames.AppsettingsDirectory];
            var appName = c[GsaAppSettingsVariablePaths.AppName];
            EnvironmentInfo[GsaEnvironmentVariableNames.AppsettingsDirectory] = appSettingsDirectory;
            EnvironmentInfo[GsaEnvironmentVariableNames.LogfileDirectory] = c[GsaEnvironmentVariableNames.LogfileDirectory];
            EnvironmentInfo[GsaAppSettingsVariablePaths.AppName] = appName;
            if (!string.IsNullOrEmpty(appSettingsDirectory))
            {
                EnvironmentInfo[nameof(appName)] = appName;
                if (!string.IsNullOrEmpty(appName))
                {
                    var gsaAppSettingsOptional = Parse.ParseBool(c[GsaAppSettingsVariablePaths.AppSetttingsOptional]);
                    builder.SetBasePath(appSettingsDirectory);
                    var itAppSettingsFile = $"{appName}_appsettings.json";
                    builder.AddJsonFile(itAppSettingsFile, optional: gsaAppSettingsOptional, reloadOnChange: true);
                    EnvironmentInfo[nameof(itAppSettingsFile)] = itAppSettingsFile;
                }
            }

            if (hostingContext.HostingEnvironment.EnvironmentName=="Development")
            {
                EnvironmentInfo["IsDevelopment"] = true;
            }
            builder.AddUserSecrets<Program>();
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
                    .Enrich.With<Serilog.Enrichers.CorrelationIdEnricher>()
                    .Enrich.WithProperty("ApplicationName", c["ApplicationName"])
                    .Enrich.WithProperty("SprintName", c["SprintConfig:SprintName"])
                    .Enrich.FromLogContext()
                    .WriteTo.Trace();
                var connectionString = c.GetConnectionString(c["SerilogSqlServerSink:ConnectionStringName"]);
                if (!string.IsNullOrEmpty(connectionString))
                {
                    loggerConfiguration.WriteTo.MSSqlServer(connectionString, c["SerilogSqlServerSink:TableName"], schemaName: c["SerilogSqlServerSink:SchemaName"]);
                }
                var appName = c[GsaAppSettingsVariablePaths.AppName];
                var logDirectory = c[GsaEnvironmentVariableNames.LogfileDirectory];
                if (!string.IsNullOrEmpty(logDirectory))
                {
                    try
                    {
                        logDirectory = Environment.ExpandEnvironmentVariables(logDirectory);
                        System.IO.Directory.CreateDirectory(logDirectory);
                        var logRotatingFileName = $"{logDirectory}{appName}.log";
                        EnvironmentInfo[nameof(logRotatingFileName)] = logRotatingFileName;
                        loggerConfiguration.WriteTo.RollingFile(logRotatingFileName, logLevel, flushToDiskInterval: TimeSpan.FromSeconds(5));
                    }
                    catch (Exception ex)
                    {
                        EnvironmentInfo["OnDiskLogSetupError"] = ex.Message;
                    }
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
