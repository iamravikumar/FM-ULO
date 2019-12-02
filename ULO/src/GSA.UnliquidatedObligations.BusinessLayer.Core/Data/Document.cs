using System.ComponentModel.DataAnnotations.Schema;

namespace GSA.UnliquidatedObligations.BusinessLayer.Data
{
    [TableKey("Documents")]
    public partial class Document : ISoftDelete
    {
        [NotMapped]
        public bool IsDeleted
        {
            get; private set;
        }

        string ISoftDelete.DeleteKey
            => DocumentId.ToString();

        public void Delete(string deletorUserId)
        {
            DeletedByUserId = deletorUserId;
            IsDeleted = true;
        }
    }
}
