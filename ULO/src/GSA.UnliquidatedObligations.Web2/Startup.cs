﻿using System.Diagnostics;
using System.Net.Mail;
using GSA.Authentication.LegacyFormsAuthentication;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using GSA.UnliquidatedObligations.BusinessLayer.Workflow;
using GSA.UnliquidatedObligations.Web.Authorization;
using GSA.UnliquidatedObligations.Web.Controllers;
using GSA.UnliquidatedObligations.Web.Identity;
using GSA.UnliquidatedObligations.Web.Services;
using Hangfire;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RevolutionaryStuff.AspNetCore.Filters;
using RevolutionaryStuff.Core;
using RevolutionaryStuff.Core.Caching;

namespace GSA.UnliquidatedObligations.Web
{
    public class Startup
    {
        public static Startup Instance { get; private set; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            Instance = this;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Add(new ServiceDescriptor(typeof(IConfiguration), Configuration));

            services.AddOptions();

            services.ConfigureOptions<HttpHeadersFilter.Config>(HttpHeadersFilter.Config.ConfigSectionName);
            services.ConfigureOptions<SprintConfig>(SprintConfig.ConfigSectionName);
            services.ConfigureOptions<PortalHelpers.Config>(PortalHelpers.Config.ConfigSectionName);
            services.ConfigureOptions<UserHelpers.Config>(UserHelpers.Config.ConfigSectionName);
            services.ConfigureOptions<BackgroundTasks>(BackgroundTasks.Config.ConfigSectionName);
            services.ConfigureOptions<UloController.Config>(UloController.Config.ConfigSectionName);            
            services.ConfigureOptions<AccountController.Config>(AccountController.Config.ConfigSectionName);
            services.ConfigureOptions<LegacyFormsAuthenticationService.Config>(LegacyFormsAuthenticationService.Config.ConfigSectionName);
            services.ConfigureOptions<WorkflowManager.Config>(WorkflowManager.Config.ConfigSectionName);
            services.ConfigureOptions<ReportsController.Config>(ReportsController.Config.ConfigSectionName);

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            //https://stackoverflow.com/questions/38184583/how-to-add-ihttpcontextaccessor-in-the-startup-class-in-the-di-in-asp-net-core-1
            services.AddHttpContextAccessor();

            services
                .AddIdentity<AspNetUser, AspNetRole>()
                .AddEntityFrameworkStores<UloDbContext>()
                .AddUserManager<UloUserManager>()
                .AddDefaultTokenProviders()
                .AddSignInManager<UloSignInManager>();

            services.AddAuthentication();
            services.UseSitePermissions();

            services.AddSingleton<IRecurringJobManager, RecurringJobManager>();
            services.AddScoped<IReportRunner, ReportRunner>();
            services.AddScoped<IBackgroundTasks, BackgroundTasks>();
            services.AddScoped<SmtpClient>();
            services.AddScoped<IEmailServer, EmailServer>();
            services.AddScoped<IWorkflowDescriptionFinder, DatabaseWorkflowDescriptionFinder>();
            services.AddScoped<IWorkflowManager, WorkflowManager>();
            services.AddScoped<ILegacyFormsAuthenticationService, LegacyFormsAuthenticationService>();
            services.AddScoped<IUserClaimsPrincipalFactory<AspNetUser>, NoUloClaimsUserClaimsPrincipalFactory>();

            services.AddScoped<IActivityChooser, FieldComparisonActivityChooser>();
            services.AddScoped<FieldComparisonActivityChooser>();

            services.AddDbContext<UloDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString(PortalHelpers.DefaultConectionStringName), z => z.EnableRetryOnFailure(1))
                );

            services.AddSingleton(_=>Cache.DataCacher);

            services.AddApplicationInsightsTelemetry();

            services.AddMvc(config =>
            {
                config.Filters.Add(typeof(HttpHeadersFilter));
            }).SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            services.AddSingleton(provider => Serilog.Log.ForContext<Startup>());
            services.AddScoped<PortalHelpers>();

            services.AddScoped<UserHelpers>();

            services.AddHangfire(x => x.UseSqlServerStorage(Configuration.GetConnectionString(Configuration["Hangfire:ConnectionStringName"])));
            if (Configuration.GetValue<bool>("Hangfire:UseServer"))
            {
                services.AddHangfireServer();
            }

            services.AddScoped<EmailController>();
            services.AddSingleton<DynamicTemplateFileProvider>();
            services.AddRazorPages().AddRazorRuntimeCompilation();
            services.AddOptions<MvcRazorRuntimeCompilationOptions>().Configure((MvcRazorRuntimeCompilationOptions r, DynamicTemplateFileProvider e) => {
                r.FileProviders.Add(e);
            });
        }

        /*
         * This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
         * 
         * There is some MAJOR voodoo going on with this signature!
         * Is this some creepy DI action?
         * How do we know what other magic parameters to add?
         * Because... TelemetryConfiguration is one of them
         * https://docs.microsoft.com/en-us/azure/azure-monitor/app/asp-net-core#disable-telemetry-dynamically
        */
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, TelemetryConfiguration telemetryConfiguration)
        {
            var appPathBase = Configuration[Program.GsaAppSettingsVariablePaths.AppPathBase];
            if (!string.IsNullOrEmpty(appPathBase))
            {
                app.UsePathBase(appPathBase);
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });


            var dashboardPath = Configuration["Hangfire:DashboardPath"];
            Trace.WriteLine($"Hangfire client dashboard path = [{dashboardPath}]");
            if (!string.IsNullOrEmpty(dashboardPath))
            {
                app.UseHangfireDashboard(dashboardPath, new DashboardOptions
                {
                    Authorization = new[] { new HangfireDashboardAuthorizer() }
                });
            }
        }       
    }
}
