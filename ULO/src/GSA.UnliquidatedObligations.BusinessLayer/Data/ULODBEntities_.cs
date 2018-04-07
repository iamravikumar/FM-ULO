using RevolutionaryStuff.Core;
using RevolutionaryStuff.Core.Collections;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GSA.UnliquidatedObligations.BusinessLayer.Data
{
    public partial class ULODBEntities
    {
        private IList<DbEntityEntry<T>> EntityList<T>(params EntityState[] states) where T : class
        {
            return this.ChangeTracker.Entries<T>().Where(z => states.Length == 0 ? true : states.Contains(z.State)).ToList();
        }

        public override int SaveChanges()
        {
            var pss = PreSaveChanges();
            var ret = base.SaveChanges();
            PostSaveChanges(pss);
            return ret;
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            var pss = PreSaveChanges();
            var ret = await base.SaveChangesAsync(cancellationToken);
            PostSaveChanges(pss);
            return ret;
        }

        private class PreSaveState
        {
            public readonly MultipleValueDictionary<string, DbEntityEntry<ISoftDelete>> DeleteKeysByTableKey = new MultipleValueDictionary<string, DbEntityEntry<ISoftDelete>>();
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
                else if (z.State==EntityState.Deleted)
                {
                    throw new Exception($"Developer should have used SoftDelete on object type = {et.Name}");
                }
            });
            return pss;
        }

        private void PostSaveChanges(PreSaveState pss)
        {
            Requires.NonNull(pss, nameof(pss));
            foreach (var item in pss.DeleteKeysByTableKey.AtomEnumerable)
            {
                var e = item.Value;
                e.State = EntityState.Detached;
                SoftDelete(item.Key, e.Entity.DeleteKey, null);
            }
        }

        protected ObjectContext ObjectContext
        {
            get { return ((IObjectContextAdapter)this).ObjectContext; }
        }

        public void Refresh(object entity, RefreshMode mode = RefreshMode.StoreWins)
        {
            ObjectContext.Refresh(mode, entity);
        }

        public void Refresh(IEnumerable<object> entities, RefreshMode mode = RefreshMode.StoreWins)
        {
            ObjectContext.Refresh(mode, entities);
        }
    }
}
