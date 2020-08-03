using System.Linq;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RevolutionaryStuff.Core;
using RevolutionaryStuff.Core.Caching;

namespace GSA.UnliquidatedObligations.Web.Controllers
{
    public abstract class BaseViewComponent : ViewComponent
    {
        protected readonly PortalHelpers PortalHelpers;
        protected readonly ILogger Logger;
        protected readonly ICacher Cacher;
        protected readonly UloDbContext DB;
        protected readonly UserHelpers UserHelpers;

        protected BaseViewComponent(UloDbContext db, ICacher cacher, PortalHelpers portalHelpers, UserHelpers userHelpers, ILogger logger)
        {
            Requires.NonNull(logger, nameof(logger));

            DB = db;
            Cacher = cacher;
            PortalHelpers = portalHelpers;
            UserHelpers = userHelpers;
            DB.CurrentUserId = userHelpers.CurrentUserId;
            logger.LogDebug("Page request to {Controller}", this.GetType().Name);
        }

        public string CurrentUserId
            => UserHelpers.CurrentUserId;

        public string CurrentUserName
            => UserHelpers.CurrentUserName;

        protected AspNetUser CurrentUser
        {
            get
            {
                if (CurrentUser_p == null)
                {
                    CurrentUser_p = DB.AspNetUsers.FirstOrDefault(u => u.UserName == User.Identity.Name);
                }
                return CurrentUser_p;
            }
        }
        private AspNetUser CurrentUser_p;
    }
}
