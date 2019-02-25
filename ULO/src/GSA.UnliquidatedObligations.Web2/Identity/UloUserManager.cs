using System;
using System.Collections.Generic;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GSA.UnliquidatedObligations.Web.Identity
{
    public class UloUserManager : UserManager<AspNetUser>
    {
        public UloUserManager(IUserStore<AspNetUser> store, IOptions<IdentityOptions> optionsAccessor, IPasswordHasher<AspNetUser> passwordHasher, IEnumerable<IUserValidator<AspNetUser>> userValidators, IEnumerable<IPasswordValidator<AspNetUser>> passwordValidators, ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors, IServiceProvider services, ILogger<UserManager<AspNetUser>> logger)
            : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        { }
    }
}
