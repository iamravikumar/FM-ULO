using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using GSA.UnliquidatedObligations.Web.Identity;
using GSA.UnliquidatedObligations.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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

        public static class ActionNames
        {
            public const string Login = "Login";
            public const string Logout = "Logout";
        }

        private readonly UloSignInManager SignInManager;
        private readonly UloUserManager UserManager;
        private readonly IOptions<Config> ConfigOptions;

        public class Config
        {
            public const string ConfigSectionName = "AccountControllerConfig";
            public bool UseDevAuthentication { get; set; }
            public string DevLoginPassword { get; set; }
            public string SecureAuthCookieName { get; set; }
        }

        public AccountController(IOptions<Config> configOptions, UloSignInManager signInManager, UloUserManager userManager, UloDbContext db, ICacher cacher, PortalHelpers portalHelpers, UserHelpers userHelpers, ILogger logger)
            : base(db, cacher, portalHelpers, userHelpers, logger)
        {
            Requires.NonNull(configOptions, nameof(configOptions));
            Requires.NonNull(signInManager, nameof(signInManager));
            Requires.NonNull(userManager, nameof(userManager));

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

        ////
        // GET: /Account/Login
        [AllowAnonymous]
        [ActionName(ActionNames.Login)]
        public ActionResult Login(string returnUrl)
        {
            PortalHelpers.HideLoginLinks(ViewBag, true);

            if (ConfigOptions.Value.UseDevAuthentication)
            {
                ViewBag.ReturnUrl = returnUrl;

                return View("DevLogin");
            }

            var cookie = Request.Cookies[ConfigOptions.Value.SecureAuthCookieName];
            if (cookie == null)
            {
                return View(new LoginViewModel());
            }
            throw new NotImplementedException();
//            return ExternalLoginCallback(returnUrl);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DevLogin(DevLoginViewModel model, string returnUrl)
        {
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
        [ValidateAntiForgeryToken]
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
            var redirectUrl = Properties.Settings.Default.SecureAuthUrl +
                            new QueryString(
                                "ReturnUrl",
                                $"https://{Request.Url.Host}/Account/ExternalLoginCallback");
            return ExternalLogin(DefaultAuthenticationTypes.ExternalCookie, redirectUrl);
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public ActionResult ExternalLoginCallback(string returnUrl)
        {
            FormsAuthenticationTicket ticket = null;
            var cookie = Request.Cookies[ConfigOptions.Value.SecureAuthCookieName];
            if (cookie == null)
            {
                Log.Error("Cannot find expected cookie from secureAuth name={CookieName}", ConfigOptions.Value.SecureAuthCookieName);
                return RedirectToAction(ActionNames.Login);
            }
            else
            {
                ticket = FormsAuthentication.Decrypt(cookie.Value);
            }
            if (ticket == null)
            {
                Log.Error("Must redirect as we could not decrypt {EncryptedTicket}", cookie.Value);
                return RedirectToAction(ActionNames.Login);
            }

            var result = SignInManager.PasswordSignIn(ticket.Name, "", true, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    Log.Information("External Login success for user {NewUserName} with {EncryptedTicket}", ticket.Name, cookie.Value);
                    return RedirectToLocal(returnUrl);
                default:
                    Log.Error("We have a secureAuth return for {UserName} but a signin status of {SignInStatus} for {EncryptedTicket}", ticket.Name, result, cookie.Value);
                    return View(ActionNames.Login, new LoginViewModel(true));
            }
        }
#endif
    }
}
