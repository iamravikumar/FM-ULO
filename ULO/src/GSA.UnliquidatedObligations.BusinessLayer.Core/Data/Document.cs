namespace GSA.UnliquidatedObligations.BusinessLayer.Data
{
    [TableKey("Documents")]
    public partial class Document : ISoftDelete
    {
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
