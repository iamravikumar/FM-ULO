using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using GSA.UnliquidatedObligations.BusinessLayer.Workflow;
using GSA.UnliquidatedObligations.Web.Services;
using Microsoft.Owin;
using Owin;
using Autofac;
using Autofac.Integration.Mvc;
using GSA.UnliquidatedObligations.Web.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.SqlServer;

[assembly: OwinStartupAttribute(typeof(GSA.UnliquidatedObligations.Web.Startup))]
namespace GSA.UnliquidatedObligations.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var container = ConfigureBuilder().Build();
            var options = new SqlServerStorageOptions
            {
                PrepareSchemaIfNecessary = false
            };
            GlobalConfiguration.Configuration.UseAutofacActivator(container).UseSqlServerStorage("DefaultConnection", options);

          

            app.UseHangfireServer();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
            ConfigureAuth(app);

            DashboardOptions dashboardOptionsoptions = new DashboardOptions
            {
                Authorization = new[] { new MyAuthorizationFilter() }
            };
            app.UseHangfireDashboard("/hangfire", dashboardOptionsoptions);

        }

        public ContainerBuilder ConfigureBuilder()
        {
            //TODO: Add 3rd party libraries to Readme
            var builder = new ContainerBuilder();
            builder.RegisterControllers(typeof(MvcApplication).Assembly);

            builder.RegisterType<ULODBEntities>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<WorkflowManager>()
                .As<IWorkflowManager>()
                .InstancePerLifetimeScope();

            builder.RegisterType<DatabaseWorkflowDescriptionFinder>()
                .As<IWorkflowDescriptionFinder>()
                .InstancePerLifetimeScope();

            builder.RegisterType<BackgroundTasks>().As<IBackgroundTasks>().InstancePerBackgroundJob();
            builder.RegisterType<BackgroundJobClient>().As<IBackgroundJobClient>().InstancePerLifetimeScope();

            builder.Register(ctx => new EmailServer(new SmtpClient())).As<IEmailServer>();

            //May need to add more if we add more choosers.
            //If it gets to where there are A LOT, we could look more into http://docs.autofac.org/en/latest/register/scanning.html
            //to register all types implementing a certain interface.
            builder.RegisterType<FieldComparisonActivityChooser>()
                .Keyed<IActivityChooser>("FieldComparisonActivityChooser")
                .InstancePerRequest();



            //Authentication
            builder.RegisterType<ApplicationDbContext>().AsSelf().InstancePerRequest();
            builder.Register(c => new UserStore<ApplicationUser>(c.Resolve<ApplicationDbContext>())).AsImplementedInterfaces().InstancePerRequest();
            builder.Register(c => HttpContext.Current.GetOwinContext().Authentication).As<IAuthenticationManager>();
            builder.Register(c => new IdentityFactoryOptions<ApplicationUserManager>()
            {
                DataProtectionProvider = new Microsoft.Owin.Security.DataProtection.DpapiDataProtectionProvider("ULO")
            });

            //TODO: Do we need to create interfaces for these for testing?
            builder.RegisterType<ApplicationUserManager>().AsSelf().InstancePerRequest();
            builder.RegisterType<ApplicationSignInManager>().AsSelf().InstancePerRequest();
            //app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            //app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);

            return builder;
        }
    }

    public class MyAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            // In case you need an OWIN context, use the next line, `OwinContext` class
            // is the part of the `Microsoft.Owin` package.
            var owinContext = new OwinContext(context.GetOwinEnvironment());

            // Allow all authenticated users to see the Dashboard (potentially dangerous).
            return owinContext.Authentication.User.Identity.IsAuthenticated;
        }
    }
}
