using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GSA.UnliquidatedObligations.BusinessLayer.Data
{
    public partial class UloDbContext : IdentityDbContext
    {
        public UloDbContext(DbContextOptions<UloDbContext> options)
            : base(options)
        {}

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IdentityUser>()
                .HasDiscriminator<string>(nameof(AspNetUser.UserType))
                .HasValue<GroupUser>(AspNetUser.UserTypes.Group)
                .HasValue<PersonUser>(AspNetUser.UserTypes.Person)
                .HasValue<SystemUser>(AspNetUser.UserTypes.System)
                ;
        }
    }
}
