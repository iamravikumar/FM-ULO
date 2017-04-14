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
using GSA.UnliquidatedObligations.Web.Controllers;
using GSA.UnliquidatedObligations.Web.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Hangfire;

[assembly: OwinStartupAttribute(typeof(GSA.UnliquidatedObligations.Web.Startup))]
namespace GSA.UnliquidatedObligations.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var container = ConfigureBuilder().Build();
            GlobalConfiguration.Configuration.UseAutofacActivator(container).UseSqlServerStorage("DefaultConnection");

            app.UseHangfireDashboard();

            app.UseHangfireServer();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
            ConfigureAuth(app);

        }

        public ContainerBuilder ConfigureBuilder()
        {
            //TODO: Add 3rd party libraries to Readme
            var builder = new ContainerBuilder();
            builder.RegisterControllers(typeof(MvcApplication).Assembly);

            builder.RegisterType<ULODBEntities>().AsSelf().InstancePerRequest();
            builder.RegisterType<WorkflowManager>()
                .As<IWorkflowManager>()
                .InstancePerRequest();

            builder.RegisterType<DatabaseWorkflowDescriptionFinder>()
                .As<IWorkflowDescriptionFinder>()
                .InstancePerRequest();

            builder.RegisterType<BackgroundTasks>().As<IBackgroundTasks>().InstancePerBackgroundJob();
            builder.RegisterType<BackgroundJobClient>().As<IBackgroundJobClient>().InstancePerRequest();

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
}
