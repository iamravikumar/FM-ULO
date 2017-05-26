using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using GSA.UnliquidatedObligations.BusinessLayer.Workflow;
using GSA.UnliquidatedObligations.Web.Services;
using Microsoft.Owin;
using Owin;
using Autofac;
using Autofac.Integration.Mvc;
using GSA.UnliquidatedObligations.BusinessLayer.Authorization;
using GSA.UnliquidatedObligations.Web.Controllers;
using GSA.UnliquidatedObligations.Web.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Hangfire;
using Hangfire.SqlServer;

[assembly: OwinStartupAttribute(typeof(GSA.UnliquidatedObligations.Web.Startup))]
namespace GSA.UnliquidatedObligations.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var postAuthToken =
    "C416E18046F6CB9D25AA27577A47CC75B45693679DD76380142A80483BABE5DB3ED3D02D98890171622BCED4544B36FCBD1E43A574EE08C537B56F67C361277E0AD93E940892789CEB2EE29A7A8E81B2FFCDDA50CE0C5882326C78893B2201A806FA423049E97BCC440B51A349095E7E02393DA850224F4FD3619660AB34C884895B6D30DB15CA8BDBC6165F0A57E268D674F6EEB1054A0F5E8FB0DE04D832004486E6CF";
            var decrypted = FormsAuthentication.Decrypt(postAuthToken);
            var container = ConfigureBuilder().Build();
            var options = new SqlServerStorageOptions
            {
                PrepareSchemaIfNecessary = false
            };
            GlobalConfiguration.Configuration.UseAutofacActivator(container).UseSqlServerStorage("DefaultConnection", options);

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
