using System;
using System.Net.Mail;
using GSA.Authentication.LegacyFormsAuthentication;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using GSA.UnliquidatedObligations.BusinessLayer.Workflow;
using GSA.UnliquidatedObligations.Web.Controllers;
using GSA.UnliquidatedObligations.Web.Identity;
using GSA.UnliquidatedObligations.Web.Permission;
using GSA.UnliquidatedObligations.Web.Services;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RevolutionaryStuff.Core;
using RevolutionaryStuff.Core.Caching;

namespace GSA.UnliquidatedObligations.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        private IServiceCollection ConfigureOptionsServices;

        public void ConfigureOptions<TOptions>(string sectionName) where TOptions : class
        {
            Requires.NonNull(ConfigureOptionsServices, nameof(ConfigureOptionsServices));
            ConfigureOptionsServices.Configure<TOptions>(Configuration.GetSection(sectionName));
        }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            ConfigureOptionsServices = services;
            try
            {
                OnConfigureServices(services);
            }
            finally
            {
                ConfigureOptionsServices = null;
            }
        }

        protected virtual void OnConfigureServices(IServiceCollection services)
        {
            services.Add(new ServiceDescriptor(typeof(IConfiguration), Configuration));

            services.AddOptions();

            ConfigureOptions<SprintConfig>(SprintConfig.ConfigSectionName);
            ConfigureOptions<PortalHelpers.Config>(PortalHelpers.Config.ConfigSectionName);
            ConfigureOptions<UserHelpers.Config>(UserHelpers.Config.ConfigSectionName);
            ConfigureOptions<BackgroundTasks>(BackgroundTasks.Config.ConfigSectionName);
            ConfigureOptions<UloController.Config>(UloController.Config.ConfigSectionName);
            ConfigureOptions<AccountController.Config>(AccountController.Config.ConfigSectionName);
            ConfigureOptions<LegacyFormsAuthenticationService.Config>(LegacyFormsAuthenticationService.Config.ConfigSectionName);
            ConfigureOptions<WorkflowManager.Config>(WorkflowManager.Config.ConfigSectionName);

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
            
            services.AddAuthorization(options =>
            {
                options.AddPolicy("ApplicationPermissionPolicy", policy => policy.Requirements.Add(new PermissionRequirement("ApplicationPermissionClaim")));               

            });
            services.AddTransient<IAuthorizationHandler, PermissionHandler>();       
            
            services.AddScoped<IBackgroundTasks, BackgroundTasks>();
            services.AddScoped<SmtpClient>();
            services.AddScoped<IEmailServer, EmailServer>();
            services.AddScoped<IWorkflowDescriptionFinder, DatabaseWorkflowDescriptionFinder>();
            services.AddScoped<IWorkflowManager, WorkflowManager>();
            services.AddScoped<ILegacyFormsAuthenticationService, LegacyFormsAuthenticationService>();
            services.AddScoped<IUserClaimsPrincipalFactory<AspNetUser>, NoUloClaimsUserClaimsPrincipalFactory>();

            services.AddDbContext<UloDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection"), z => z.EnableRetryOnFailure(1)));

            services.AddSingleton<ICacher>(_=>Cache.DataCacher);
             
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddSingleton(provider => Serilog.Log.ForContext<Startup>());
            services.AddScoped<PortalHelpers>();

            services.AddScoped<UserHelpers>();


            services.AddHangfire(x => x.UseSqlServerStorage(Configuration.GetConnectionString(Configuration["Hangfire:ConnectionStringName"])));


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider serviceProvider)
        {
            var appPathBase = Configuration[Program.GsaAppSettingsVariablePaths.AppPathBase];
            if (!string.IsNullOrEmpty(appPathBase))
            {
                app.UsePathBase(appPathBase);
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            app.UseHangfireServer();
            app.UseHangfireDashboard("/Hangfire", new DashboardOptions
            {
//                Authorization = new[] { new HangfireDashboardAuthorizer() }
            });           
        }       

    }
}
