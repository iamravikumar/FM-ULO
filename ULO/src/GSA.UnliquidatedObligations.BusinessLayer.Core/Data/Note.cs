using System.ComponentModel.DataAnnotations.Schema;

namespace GSA.UnliquidatedObligations.BusinessLayer.Data
{
    [TableKey("Notes")]
    public partial class Note : ISoftDelete
    {
        [NotMapped]
        public bool IsDeleted
        {
            get; private set;
        }

        string ISoftDelete.DeleteKey
            => NoteId.ToString();

        public void Delete(string deletorUserId = null)
        {
            IsDeleted = true;
            DeletedByUserId = deletorUserId;
        }
    }
}
