using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RevolutionaryStuff.Core;
using RevolutionaryStuff.Core.Caching;

namespace GSA.UnliquidatedObligations.Web.Services
{
    public class UloClaimsTransformation : BaseLoggingDisposable, IClaimsTransformation
    {
        private readonly UloDbContext Db;
        private readonly ICacher Cacher;
        private readonly IOptions<Config> ConfigOptions;

        public class Config
        {
            public const string ConfigSectionName = "UloClaimsTransformationConfig";

            public TimeSpan ClaimsCacheTimeout { get; set; } = TimeSpan.FromMinutes(5);
        }

        public UloClaimsTransformation(UloDbContext db, ICacher cacher, ILogger<UloClaimsTransformation> logger, IOptions<Config> configOptions)
            : base(logger)
        {
            Db = db;
            Cacher = cacher.CreateScope(nameof(UloClaimsTransformation));
            ConfigOptions = configOptions;
        }

        Task<ClaimsPrincipal> IClaimsTransformation.TransformAsync(ClaimsPrincipal principal)
        {
            var userName = principal.Identity.Name;
            var ci = Cacher.FindOrCreateValue(
                userName,
                () =>
                {
                    var ci = new ClaimsIdentity("ulo");
                    foreach (var c in Db.AspNetUserClaims.Where(c => c.User.UserName == userName).AsNoTracking())
                    {
                        ci.AddClaim(new Claim(c.ClaimType, c.ClaimValue));
                    }
                    return ci;
                },
                ConfigOptions.Value.ClaimsCacheTimeout);
            LogInformation("Loaded {claimCount} for {principal}", ci.Claims.Count(), userName);
            principal.AddIdentity(ci);
            return Task.FromResult(principal);
        }
    }
}
