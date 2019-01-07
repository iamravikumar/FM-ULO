using Microsoft.EntityFrameworkCore;

namespace GSA.UnliquidatedObligations.BusinessLayer.Data
{
    public partial class UloDbContext : DbContext
    {
        public UloDbContext(DbContextOptions<UloDbContext> options)
            : base(options)
        {}
    }
}
