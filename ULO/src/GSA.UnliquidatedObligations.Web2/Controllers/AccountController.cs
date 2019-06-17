using System;
using System.Linq;
using System.Threading.Tasks;
using GSA.Authentication.LegacyFormsAuthentication;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using GSA.UnliquidatedObligations.Web.Identity;
using GSA.UnliquidatedObligations.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RevolutionaryStuff.Core;
using RevolutionaryStuff.Core.Caching;
using Serilog;

namespace GSA.UnliquidatedObligations.Web.Controllers
{
    [Authorize]
    public class AccountController : BasePageController
    {
        public const string Name = "Account";
        private const string AuthenticationTypeExternalCookie = "ExternalCookie";

        public static class ActionNames
        {
            public const string Login = "Login";
            public const string LoginPostback = "LoginPostback";
            public const string Logout = "Logout";
            public const string DevLogin = "DevLogin";
        }

        private readonly UloSignInManager SignInManager;
        private readonly UloUserManager UserManager;
        private readonly IOptions<Config> ConfigOptions;
        private readonly ILegacyFormsAuthenticationService LegacyFormsAuthenticationService;

        public class Config
        {
            public const string ConfigSectionName = "AccountControllerConfig";
            public string SecureAuthUrl { get; set; }
            public bool UseDevAuthentication { get; set; }
            public string DevLoginPassword { get; set; }
            public string SecureAuthCookieName { get; set; }
            public LegacyFormsAuthenticationService.Config LegacyFormsAuthenticationConfig { get; set; }
        }

        public AccountController(ILegacyFormsAuthenticationService legacyFormsAuthenticationService, IOptions<Config> configOptions, UloSignInManager signInManager, UloUserManager userManager, UloDbContext db, ICacher cacher, PortalHelpers portalHelpers, UserHelpers userHelpers, ILogger logger)
            : base(db, cacher, portalHelpers, userHelpers, logger)
        {
            Requires.NonNull(legacyFormsAuthenticationService, nameof(legacyFormsAuthenticationService));
            Requires.NonNull(configOptions, nameof(configOptions));
            Requires.NonNull(signInManager, nameof(signInManager));
            Requires.NonNull(userManager, nameof(userManager));

            LegacyFormsAuthenticationService = legacyFormsAuthenticationService;
            ConfigOptions = configOptions;
            SignInManager = signInManager;
            UserManager = userManager;
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction(UloController.ActionNames.Index, UloController.Name);
        }



        [AllowAnonymous]
        [ActionName("a")]
        public ActionResult a()
        {
            Logger.Information("Method = {method}", nameof(a));
            return View();
        }

        [AllowAnonymous]
        [ActionName("B")]
        public ActionResult B()
        {
            Logger.Information("Method = {method}", nameof(B));
            return View();
        }

        [AllowAnonymous]
        [ActionName("C")]
        public ActionResult C()
        {
            Logger.Information("Method = {method}", nameof(C));
            return View();
        }

        [AllowAnonymous]
        [ActionName("d")]
        public ActionResult d(string echo)
        {
            Logger.Information("Method = {method}; Echo = {echo}", nameof(d), echo);
            ViewData["echo"] = echo;
            return View("dd");
        }

        [AllowAnonymous]
        [ActionName("e")]
        public ActionResult e(string echo)
        {
            Logger.Information("Method = {method}; Echo = {echo}", nameof(e), echo);
            ViewData["echo"] = echo;
            return View();
        }

        [AllowAnonymous]
        [ActionName("f")]
        public ActionResult f(DevLoginViewModel model=null)
        {
            Logger.Information("Method = {method}; Model={isNull}", nameof(f), model==null);
            return View("devlogin");
        }

        [Route("/account/g")]
        [AllowAnonymous]
        [ActionName("GGG")]
        public ActionResult GREAT(DevLoginViewModel model = null)
        {
            return View("g");
        }

        ////
        // GET: /Account/Login
        [Route("/account/login")]
        [AllowAnonymous]
        [ActionName(ActionNames.Login)]
        public async Task<ActionResult> Login(string returnUrl=null)
        {
            PortalHelpers.HideLoginLinks(ViewBag, true);

            Logger.Information(
                "Login being called.returnUrl={returnUrl} UseDevAuthentication={UseDevAuthentication} SecureAuthCookieName={SecureAuthCookieName} cookie={cookie}", 
                returnUrl, 
                ConfigOptions.Value.UseDevAuthentication,
                ConfigOptions.Value.SecureAuthCookieName,
                Request.Cookies[ConfigOptions.Value.SecureAuthCookieName]);

            if (ConfigOptions.Value.UseDevAuthentication)
            {
                ViewBag.ReturnUrl = returnUrl;
                return View("devlogin");
            }

            var cookie = Request.Cookies[ConfigOptions.Value.SecureAuthCookieName];
            if (cookie == null)
            {
                return View(new LoginViewModel());
            }
            return await ExternalLoginCallbackAsync(returnUrl, cookie);
        }

        [HttpPost]
        [AllowAnonymous]
//        [ValidateAntiForgeryToken]
        [ActionName(ActionNames.LoginPostback)]
        public ActionResult LoginPostback(string returnUrl)
        {
            var redirectUrl = WebHelpers.AppendParameter(ConfigOptions.Value.SecureAuthUrl, "ReturnUrl", $"https://{Request.Host.Host}/");
            return new RedirectResult(redirectUrl);
        }

        [Route("/account/devlogin")]
        [ActionName(ActionNames.DevLogin)]
        [HttpPost]
        [AllowAnonymous]
//        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DevLogin(DevLoginViewModel model, string returnUrl)
        {
            Logger.Information("DevLogin being called!");

            OnlySupportedInDevelopmentEnvironment();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = DB.AspNetUsers.FirstOrDefault(u => u.UserName == model.Username);
            if (user != null && model.Password == ConfigOptions.Value.DevLoginPassword)
            {
                await SignInManager.SignInAsync(user, model.RememberMe);
                Log.Information("Dev Login success for user {NewUserName}", model.Username);
                return RedirectToLocal(returnUrl);
            }
            ModelState.AddModelError("", "Invalid login attempt.");
            return View(model);
        }

        [ActionName(ActionNames.Logout)]
        [HttpPost]
//        [ValidateAntiForgeryToken]
        public async Task<ActionResult> LogOff()
        {
            if (ConfigOptions.Value.UseDevAuthentication)
            {
                Log.Information("Dev LogOff");
                await SignInManager.SignOutAsync();
            }
            else
            {
                Log.Information("GSA LogOff");
            }
/*
            Session.Abandon();
            Response.Cookies.Clear();
            var requestCookies = new HttpCookieCollection();
            for (int z = 0; z < Request.Cookies.Count; ++z)
            {
                requestCookies.Add(Request.Cookies[z]);
            }
            for (int z = 0; z < requestCookies.Count; ++z)
            {
                var c = requestCookies[z];
                Response.Cookies.Add(new HttpCookie(c.Name)
                {
                    Domain = c.Domain,
                    Expires = DateTime.Now.AddYears(-1),
                    HttpOnly = c.HttpOnly,
                    Path = c.Path,
                    Secure = c.Secure,
                    Shareable = c.Shareable,
                });
            }
            */
            return RedirectToAction(ActionNames.Login);
        }

#if false

        //
        // POST: /Account/DevLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(DevLoginViewModel model, string returnUrl)
        {            
            var redirectUrl = WebHelpers.AppendParameter(ConfigOptions.Value.SecureAuthUrl, "ReturnUrl", $"https://{Request.Host.Host}/Account/ExternalLoginCallback");
            return ExternalLogin(AuthenticationTypeExternalCookie, redirectUrl);
        }

        private void TestTheSecureAuthCookie()
        {
            foreach (var c in new[] {
                "3FAC45FFF89CB74AFEB68E97113CFDD9794497D66A129AC8752857FA60538A706865875BF9E6E410FF71D9E88DB994FF1AD6FF32CC0362321E960D770F95658E663648A845BEDCB5A5C45B1FD7483BDD3411BFE9D4EF387C5FA1D57E2D3640BBEE3A994FD4094B92E2A8CF941CD39EC062146D70F735EFB0E3178CD5E73E02012D0AB9553F9942AA882C067B99419BDFE5AE0B2FF5189F1D9E6AB86F8D75597DE866FE85",
                "5DE1F6716D33E07B8B47CFD8702DDCF0D0C40E586E99D21FE4027AD6614DED99CE8EABAD1176F1CD0D395B0A817F40598F35BFD4567FA54250B8B3FAB3D8E9E2AE431EDACCA53CB3DA75949A5B7232F567F93F6EC9E8EFC1F37C21C6C8A127601A103367CD39D0EED3C85AB318A96BA8B64C456913AA8FEFC9CD42B8A00BC0D44CA3CBA1923B09382CCABA02AD6995B96247C030",
                "FB36CF43A1BC3AC9EE74DCD772DBFEB0B30DE867E9FE35E0BB95DDEE91CF4A5B8CFAB224D77F2D2EC04BAA85195CBCA9A1D3B2571D3C61438574510B3D36A774F89AF21C7C13D146F1C86D6166F46F523B07249137F0E2FD1AAE25D0BA8F7B7E914656C30F64A47CAEFD59C851419FE2F266DE4CDF4610B65158C2222AE1277761E57117",
                "4DDF5C6F8B84199D92E011345E37BFEA53201B9AB790210732D67E2C16C2F70A94A06CF0BF4AF1E375CE27197444C04904FEABE2E546E55DD47B684FDBB9938545AEB7119E8AA3442C33B81CF70FE452D518896550E6A7EC639DEA037BDC6361FDE53C6E0291F94F636EE7E362C43F3C270E1B7971B59DC9D6A37BBDDC14B1288B142B43030E6B48786DD6B42A8D45761B114FE778C9C2008F0AB67D9432159BDCBE41E6"
            })
            {
                try
                {
                    var fac = LegacyFormsAuthenticationService.Unprotect(c);
                    Stuff.Noop(fac);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Must redirect as we could not decrypt {EncryptedTicket}", c);
                }
            }
        }

#endif
        private async Task<ActionResult> ExternalLoginCallbackAsync(string returnUrl, string cookie)
        {
            FormsAuthenticationCookie ticket;
            try
            {
                ticket = LegacyFormsAuthenticationService.Unprotect(cookie);
                if (ticket == null)
                {
                    throw new Exception("Could not unprotect the ticket");
                } 
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Must redirect as we could not decrypt {EncryptedTicket}", cookie);
                return RedirectToAction(ActionNames.Login);
            }
            Log.Information("Decrypted cookie from secureAuth yields {UserName}.", ticket.UserName);

            try
            {
                var user = DB.AspNetUsers.FirstOrDefault(u => u.UserName == ticket.UserName);
                if (user == null)
                {
                    throw new Exception($"could not find user {ticket.UserName} in our database");
                }
                await SignInManager.SignInAsync(user, false);
                Log.Information("External Login success for user {NewUserName} with {EncryptedTicket}", ticket.UserName, cookie);
                return RedirectToLocal(returnUrl);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "We have a secureAuth return for {UserName} but could not sign them in with {EncryptedTicket}", ticket.UserName, cookie);
                return View(ActionNames.Login, new LoginViewModel(true));
            }
        }
    }
}
