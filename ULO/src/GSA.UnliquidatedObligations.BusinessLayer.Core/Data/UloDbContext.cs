using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GSA.UnliquidatedObligations.BusinessLayer.Data
{
    public partial class UloDbContext : IdentityDbContext
    {
        public UloDbContext(DbContextOptions<UloDbContext> options)
            : base(options)
        {}
    }
}
