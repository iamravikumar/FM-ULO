using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RevolutionaryStuff.Core;
using Serilog;
using Serilog.Core;
using Serilog.Core.Enrichers;

namespace GSA.UnliquidatedObligations.Web2.Controllers
{
    public abstract class BasePageController : Controller
    {
        protected readonly PortalHelpers PortalHelpers;
        protected readonly ILogger Logger;

        protected BasePageController(PortalHelpers portalHelpers, ILogger logger)
        {
            Requires.NonNull(logger, nameof(logger));

            PortalHelpers = portalHelpers;

            Logger = logger.ForContext(new ILogEventEnricher[]
            {
                new PropertyEnricher(typeof(Type).Name, this.GetType().Name),
            });

            Log.Debug("Page request to {Controller}", this.GetType().Name);
        }
    }
}
