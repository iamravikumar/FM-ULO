using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RevolutionaryStuff.Core;

namespace GSA.UnliquidatedObligations.BusinessLayer.Data
{
    /// <remarks>
    /// See below for the inheritance pattern around IdentityDbContext
    /// This is because of the forced Discriminator fields on derived classes 
    /// https://entityframeworkcore.com/knowledge-base/48712868/avoid--discriminator--with-aspnetusers--aspnetroles----aspnetuserroles
    /// </remarks>
    public partial class UloDbContext :
        IdentityDbContext<AspNetUser, AspNetRole, string, AspNetUserClaim, AspNetUserRole, AspNetUserLogin, AspNetRoleClaim, AspNetUserToken>
    {
        public UloDbContext(DbContextOptions<UloDbContext> options)
            : base(options)
        {}

        partial void OnAddFluentKeys(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AspNetUserClaim>(e => e.HasOne(r => r.User).WithMany(u => u.UserAspNetUserClaims).HasForeignKey(r => r.UserId).HasPrincipalKey(r => r.Id));
        }

        /*
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IdentityUser>()
                .HasDiscriminator<string>(nameof(AspNetUser.UserType))
                .HasValue<GroupUser>(AspNetUser.UserTypes.Group)
                .HasValue<PersonUser>(AspNetUser.UserTypes.Person)
                .HasValue<SystemUser>(AspNetUser.UserTypes.System)
                ;
        }
        */

        public IQueryable<Workflow> GetWorkflows(IEnumerable<int> workflowIds)
        {
            /*
            var workflows = DB.Workflows.Where(w => workflowIds.Contains(w.WorkflowId));
            For whatever reason, linq 2 sql wont translate the above into an IN statement (maybe it only does this for string),
            As such, we have to build out a big long nasty OR predicate then apply which we do below.             
             */
            var predicate = PredicateBuilder.Create<Workflow>(wf => false);
            foreach (var wfid in workflowIds)
            {
                predicate = predicate.Or(wf => wf.WorkflowId == wfid);
            }
            return Workflows.Where(predicate);
        }

        public ICollection<Document> GetUniqueMissingLineageDocuments(Workflow wf)
             => GetUniqueMissingLineageDocuments(wf, GetUloSummariesByPdnAsync(wf.TargetUlo.PegasysDocumentNumber).ExecuteSynchronously().Select(z => z.WorkflowId));

        public ICollection<Document> GetUniqueMissingLineageDocuments(Workflow wf, IEnumerable<int> otherWorkflowIds)
        {
            var others = new List<int>(otherWorkflowIds);
            var otherDocsByName = Documents.Where(d => others.Contains(d.WorkflowId)).OrderByDescending(d => d.CreatedAtUtc).ToDictionaryOnConflictKeepLast(d => d.DocumentName, d => d);
            wf.WorkflowDocuments.ForEach(d => otherDocsByName.Remove(d.DocumentName));
            return otherDocsByName.Values;
        }


    }
}
