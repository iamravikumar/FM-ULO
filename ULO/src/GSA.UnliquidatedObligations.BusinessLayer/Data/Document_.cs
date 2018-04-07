namespace GSA.UnliquidatedObligations.BusinessLayer.Data
{
    [TableKey("Documents")]
    public partial class Document : ISoftDelete
    {
        public static readonly Document[] None = new Document[0];

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

        public string CreatedAtLocalTimeString
            => CreatedAtUtc.ToLocalTime().ToString("MM/dd/yyyy");
    }
}
