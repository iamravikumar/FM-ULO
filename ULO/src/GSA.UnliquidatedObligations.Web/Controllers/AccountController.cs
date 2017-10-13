using Autofac;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using GSA.UnliquidatedObligations.Web.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using RevolutionaryStuff.Core.Caching;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace GSA.UnliquidatedObligations.Web.Controllers
{
    [Authorize]
    public class AccountController : BaseController
    {
        public const string Name = "Account";

        public static class ActionNames
        {
            public const string Login = "Login";
        }

        private ApplicationSignInManager SignInManager;
        private ApplicationUserManager UserManager;
        private readonly IAuthenticationManager AuthenticationManager;

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager, IAuthenticationManager authenticationManager, ULODBEntities db, IComponentContext componentContext, ICacher cacher, Serilog.ILogger logger)
            : base(db, componentContext, cacher, logger)
        {
            UserManager = userManager;
            SignInManager = signInManager;
            AuthenticationManager = authenticationManager;
        }

        ////
        // GET: /Account/Login
        [AllowAnonymous]
        [ActionName(ActionNames.Login)]
        public ActionResult Login(string returnUrl)
        {
            if (PortalHelpers.UseDevAuthentication)
            {
                ViewBag.ReturnUrl = returnUrl;
                return View("DevLogin");
            }

            var cookie = Request.Cookies[Properties.Settings.Default.SecureAuthCookieName];
            if (cookie == null)
            {
                return View(new LoginViewModel());
            }
            return ExternalLoginCallback(returnUrl);
        }

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
        // POST: /Account/DevLogin
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

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.Username, "", model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    Log.Information("Dev Login success for user {NewUserName}", model.Username);
                    return RedirectToLocal(returnUrl);
                //case SignInStatus.LockedOut:
                //    return View("Lockout");
                //case SignInStatus.RequiresVerification:
                //    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                //case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }

        }

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            OnlySupportedInDevelopmentEnvironment();

            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            OnlySupportedInDevelopmentEnvironment();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            OnlySupportedInDevelopmentEnvironment();
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            OnlySupportedInDevelopmentEnvironment();
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                    // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                    return RedirectToAction("Index", "Home");
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            OnlySupportedInDevelopmentEnvironment();
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            OnlySupportedInDevelopmentEnvironment();
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            OnlySupportedInDevelopmentEnvironment();
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                // string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                // var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
                // await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                // return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            OnlySupportedInDevelopmentEnvironment();
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            OnlySupportedInDevelopmentEnvironment();
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            OnlySupportedInDevelopmentEnvironment();
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            OnlySupportedInDevelopmentEnvironment();
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            //return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }), null, AuthenticationManager);

            return new ChallengeResult(provider, returnUrl, null, AuthenticationManager);
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            OnlySupportedInDevelopmentEnvironment();
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            OnlySupportedInDevelopmentEnvironment();
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public ActionResult ExternalLoginCallback(string returnUrl)
        {
            FormsAuthenticationTicket ticket = null;
            var cookie = Request.Cookies[Properties.Settings.Default.SecureAuthCookieName];
            if (cookie == null)
            {
                Log.Error("Cannot find expected cookie from secureAuth name={CookieName}", Properties.Settings.Default.SecureAuthCookieName);
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
                    Log.Information("External Login success for user {NewUserName}", ticket.Name);
                    return RedirectToLocal(returnUrl);
                default:
                    Log.Error("We have a secureAuth return for {UserName} but a signin status of {SignInStatus}", ticket.Name, result);
                    return View(ActionNames.Login, new LoginViewModel(true));
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            if (Properties.Settings.Default.UseDevAuthentication)
            {
                Log.Information("Dev LogOff");
                AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            }
            else
            {
                Log.Information("GSA LogOff");
                Response.Cookies.Clear();
                var c = new HttpCookie(Properties.Settings.Default.SecureAuthCookieName);
                c.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(c);
                Request.GetOwinContext()
                        .Authentication
                        .SignOut(HttpContext.GetOwinContext()
                                            .Authentication.GetAuthenticationTypes()
                                            .Select(o => o.AuthenticationType).ToArray());
            }
            return RedirectToAction(ActionNames.Login);
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (UserManager != null)
                {
                    UserManager.Dispose();
                    UserManager = null;
                }

                if (SignInManager != null)
                {
                    SignInManager.Dispose();
                    SignInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction(UloController.ActionNames.Index, UloController.Name);
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            private readonly IAuthenticationManager AuthenticationManager;

            public ChallengeResult(string provider, string redirectUri, string userId, IAuthenticationManager authenticationManager)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
                AuthenticationManager = authenticationManager;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }


            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                AuthenticationManager.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}