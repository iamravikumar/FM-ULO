using System.Data.Entity;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using GSA.UnliquidatedObligations.Web.Services;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using GSA.UnliquidatedObligations.Web.Models;

namespace GSA.UnliquidatedObligations.Web.Controllers
{
    [Authorize]
    //[ApplicationPermissionAuthorize(ApplicationPermissionNames.ApplicationUser)]
    public class UloController : Controller
    {
        protected readonly IWorkflowManager Manager;
        protected readonly ULODBEntities DB;
        private readonly ApplicationUserManager UserManager;

        public UloController(IWorkflowManager manager, ULODBEntities db, ApplicationUserManager userManager)
        {
            Manager = manager;
            DB = db;
            UserManager = userManager;
        }

        // GET: Ulo
        public async Task<ActionResult> Index()
        {
            var currentUser = await UserManager.FindByNameAsync(this.User.Identity.Name);
            var workflows = DB.Workflows.Where(wf => wf.OwnerUserId == currentUser.Id).Include(wf => wf.UnliquidatedObligation);
            return View(workflows);
        }

        [Route("Ulo/{id}")]
        public async Task<ActionResult> Details(int id)
        {
            var ulo = await DB.UnliquidatedObligations.Include(u => u.Workflows).FirstOrDefaultAsync(u => u.UloId == id);
            //var regions = await DB.Regions.AllAsync();
            return View("Details/Index", ulo);
        }



        private async Task<Workflow> FindWorkflowAsync(int workflowId)
        {
            var wf = await DB.Workflows.Include(q => q.AspNetUser).Include(q => q.UnliquidatedObligation).FirstOrDefaultAsync(q => q.WorkflowId == workflowId);
            if (wf != null)
            {
                var currentUser = await UserManager.FindByNameAsync(this.User.Identity.Name);
                if (currentUser != null)
                {
                    if (wf.OwnerUserId == currentUser.Id) return wf;
                    if (wf.AspNetUser.UserType == UserTypes.Group.ToString())
                    {
                        //TODO: Write recursive then call recursive sproc to see if current user is in the group
                    }
                }
            }
            return null;
        }

        //Referred to by WebActionWorkflowActivity
        [ActionName("FormA")]
        [Route("FormA/{workflowId}")]
        public async Task<ActionResult> FormA(int workflowId)
        {
            var wf = await FindWorkflowAsync(workflowId);
            if (wf == null) return HttpNotFound();
            return View(new FormAModel(wf));
        }

        [HttpPost]
        [ActionName("FormA")]
        [Route("FormA/{workflowId}")]
        public async Task<ActionResult> FormA(
            int workflowId,
            [Bind(Include = nameof(FormAModel.Field0Value))]
            FormAModel model)
        {
            var wf = await FindWorkflowAsync(workflowId);
            if (wf == null) return HttpNotFound();
            if (ModelState.IsValid)
            {
                //wf.TargetUlo.FieldS0 = model.Field0Value;
                return await AdvanceAsync(wf);
            }
            return View(model);
        }

        private async Task<ActionResult> AdvanceAsync(Workflow wf)
        {
            var ret = await Manager.AdvanceAsync(wf);
            await DB.SaveChangesAsync();
            return ret;
        }
    }
}