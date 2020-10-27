using System;
using System.Linq;
using GSA.UnliquidatedObligations.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RevolutionaryStuff.Core;

namespace GSA.UnliquidatedObligations.Web.Controllers
{
    [AllowAnonymous]
    public class ErrorController : Controller
    {
        public class Config
        {
            public const string ConfigSectionName = "ErrorControllerConfig";

            public bool ShowDetailedErrorMessages { get; set; } = false;
        }

        private readonly IOptions<SprintConfig> SprintConfigOptions;
        private readonly IOptions<Config> ConfigOptions;

        public ErrorController(IOptions<SprintConfig> sprintConfigOptions, IOptions<Config> configOptions)
        {
            SprintConfigOptions = sprintConfigOptions;
            ConfigOptions = configOptions;
        }

        public IActionResult Index()
        {
            var ehp = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            var ex = ehp?.Error ?? new UloException(UloExceptionCodes.CantAccessExceptionHandlerPathFeature);
            var aex = ex as AggregateException;
            if (aex != null && aex.InnerExceptions.Count == 1)
            {
                ex = aex.InnerExceptions.First();
            }
            var m = new ErrorModel
            {
                SprintName = SprintConfigOptions.Value.SprintName,
                RequestId = this.HttpContext.TraceIdentifier,
                Path = ehp?.Path,
                ExceptionType = ex?.GetType(),
                ExceptionCode = Stuff.ObjectToString(BaseCodedException.GetCode(ex))
            };
            if (ConfigOptions.Value.ShowDetailedErrorMessages)
            {
                m.ExceptionMessage = ex?.Message;
                m.ExceptionStack = ex?.StackTrace;
            }
            return View(m);
        }
    }
}
