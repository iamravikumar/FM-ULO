namespace GSA.UnliquidatedObligations.BusinessLayer.Data
{
    [TableKey("Notes")]
    public partial class Note : ISoftDelete
    {
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
