using RevolutionaryStuff.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;

namespace GSA.UnliquidatedObligations.Web2
{
    public class Program
    {
        private static void ConfigureConfiguration(WebHostBuilderContext hostingContext, IConfigurationBuilder builder)
        {
            builder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", optional: true);
            if (hostingContext.HostingEnvironment.IsDevelopment())
            {
                builder.AddUserSecrets<Program>();
            }
        }

        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration(ConfigureConfiguration)
            .ConfigureLogging((hostingContext, logging) =>
            {
                var c = hostingContext.Configuration;
                var connectionString = c.GetConnectionString(c["SerilogSqlServerSink:ConnectionStringName"]);
                var loggerConfiguration = new LoggerConfiguration()
                    .ReadFrom.Configuration(c)
                    .MinimumLevel.Is(Parse.ParseEnum<Serilog.Events.LogEventLevel>("Logging:LogLevel:Default", Serilog.Events.LogEventLevel.Warning))
                    .Enrich.With<Serilog.Enrichers.MachineNameEnricher>()
                    .Enrich.With<Serilog.Enrichers.ProcessIdEnricher>()
                    .Enrich.With<Serilog.Enrichers.ThreadIdEnricher>()
                    .Enrich.WithProperty("ApplicationName", c["ApplicationName"])
                    .Enrich.WithProperty("SprintName", c["SprintConfig:SprintName"])
                    .Enrich.FromLogContext()
                    .WriteTo.Trace()
                    .WriteTo.MSSqlServer(connectionString, c["SerilogSqlServerSink:TableName"], schemaName: c["SerilogSqlServerSink:SchemaName"]);
                Log.Logger = loggerConfiguration.CreateLogger();
                logging.AddSerilog();
            })
           .UseStartup<Startup>();
    }
}
