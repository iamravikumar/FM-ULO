using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using RevolutionaryStuff.Core;
using RevolutionaryStuff.Core.Collections;

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

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            var pss = PreSaveChanges();
            var ret = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            await PostSaveChangesAsync(pss);
            return ret;
        }

        private IList<EntityEntry<T>> EntityList<T>(params EntityState[] states) where T : class
        {
            return this.ChangeTracker.Entries<T>().ToList().Where(z => states.Length == 0 ? true : states.Contains(z.State)).ToList();
        }

        private class PreSaveState
        {
            public readonly MultipleValueDictionary<string, EntityEntry<ISoftDelete>> DeleteKeysByTableKey = new MultipleValueDictionary<string, EntityEntry<ISoftDelete>>();
        }

        private PreSaveState PreSaveChanges()
        {
            var pss = new PreSaveState();
            EntityList<ISoftDelete>().ForEach(z =>
            {
                var et = z.Entity.GetType();
                if (z.Entity.IsDeleted && z.State != EntityState.Detached)
                {
                    z.State = EntityState.Modified;
                    var tka = et.GetCustomAttribute<TableKeyAttribute>();
                    if (tka == null)
                    {
                        throw new Exception($"Developer should have specified the TableKeyAttribute on the entity type = {et.Name}");
                    }
                    pss.DeleteKeysByTableKey.Add(tka.Key, z);
                }
                else if (z.State == EntityState.Deleted)
                {
                    throw new Exception($"Developer should have used SoftDelete on object type = {et.Name}");
                }
            });
            return pss;
        }

        private async Task PostSaveChangesAsync(PreSaveState pss)
        {
            Requires.NonNull(pss, nameof(pss));
            foreach (var item in pss.DeleteKeysByTableKey.AtomEnumerable)
            {
                var e = item.Value;
                e.State = EntityState.Detached;
                await SoftDeleteAsync(item.Key, e.Entity.DeleteKey, CurrentUserId);
            }
        }

        public string CurrentUserId { get; set; }
    }
}
