using System;
using System.Collections.Generic;
using System.IO;
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
            var appName = c[GsaAppSettingsVariablePaths.AppName];
            EnvironmentInfo[GsaAppSettingsVariablePaths.AppName] = appName;
            var gas = c.GetGsaAdministrativeSettings();
            EnvironmentInfo[nameof(gas.LogFileDirectory)] = gas.LogFileDirectory;
            EnvironmentInfo[nameof(gas.TempFileDirectory)] = gas.TempFileDirectory;
            EnvironmentInfo[nameof(gas.AppSettingsDirectory)] = gas.AppSettingsDirectory;
            EnvironmentInfo[nameof(gas.AttachmentDirectory)] = gas.AttachmentDirectory;
            if (!string.IsNullOrEmpty(gas.AppSettingsDirectory))
            {
                EnvironmentInfo[nameof(appName)] = appName;
                if (!string.IsNullOrEmpty(appName))
                {
                    var gsaAppSettingsOptional = Parse.ParseBool(c[GsaAppSettingsVariablePaths.AppSetttingsOptional]);
                    builder.SetBasePath(gas.AppSettingsDirectory);
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
                var gas = c.GetGsaAdministrativeSettings();
                if (!string.IsNullOrEmpty(gas.LogFileDirectory))
                {
                    try
                    {
                        if (File.Exists(gas.LogFileDirectory))
                        {
                            Stuff.FileTryDelete(gas.LogFileDirectory);
                        }
                        if (!Directory.Exists(gas.LogFileDirectory))
                        { 
                            Directory.CreateDirectory(gas.LogFileDirectory);
                        }
                        var logRotatingFileName = Path.Combine(gas.LogFileDirectory, $"{appName}.log");
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
                    @"Logging Initialized for:
{appName}
{LogFileDirectory}
{TempFileDirectory}
{AppSettingsDirectory}
{AttachmentDirectory}",
                    appName,
                    gas.LogFileDirectory, gas.TempFileDirectory, gas.AppSettingsDirectory, gas.AttachmentDirectory
                    );
            })
           .UseStartup<Startup>();
    }
}
