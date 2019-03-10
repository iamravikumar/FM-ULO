using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
//using GSA.UnliquidatedObligations.Web.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Hangfire;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using RevolutionaryStuff.Core.Caching;
using GSA.UnliquidatedObligations.Web.Controllers;
using RevolutionaryStuff.Core;
using GSA.UnliquidatedObligations.Web.Identity;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GSA.UnliquidatedObligations.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Add(new ServiceDescriptor(typeof(IConfiguration), Configuration));
            services.AddOptions();
            services.Configure<SprintConfig>(Configuration.GetSection(SprintConfig.ConfigSectionName));
            services.Configure<PortalHelpers.Config>(Configuration.GetSection(PortalHelpers.Config.ConfigSectionName));
            services.Configure<UloController.Config>(Configuration.GetSection(UloController.Config.ConfigSectionName));
            services.Configure<AccountController.Config>(Configuration.GetSection(AccountController.Config.ConfigSectionName));

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

            services.AddScoped<IUserClaimsPrincipalFactory<AspNetUser>, NoUloClaimsUserClaimsPrincipalFactory>();

            services.AddDbContext<UloDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection"), z => z.EnableRetryOnFailure(1)));

            services.AddSingleton<ICacher>(_=>Cache.DataCacher);
             
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddSingleton(provider => Serilog.Log.ForContext<Startup>());
            services.AddSingleton<PortalHelpers>();

            services.AddScoped<UserHelpers>();


            services.AddHangfire(x => x.UseSqlServerStorage(Configuration.GetConnectionString(Configuration["Hangfire:ConnectionStringName"])));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
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
