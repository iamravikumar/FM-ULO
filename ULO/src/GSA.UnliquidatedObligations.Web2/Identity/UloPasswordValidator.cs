using GSA.UnliquidatedObligations.BusinessLayer.Data;
using Microsoft.AspNetCore.Identity;

namespace GSA.UnliquidatedObligations.Web.Identity
{
    public class UloPasswordValidator : PasswordValidator<AspNetUser>
    {
        public UloPasswordValidator(IdentityErrorDescriber errors = null)
            : base(errors)
        { }
    }
}
