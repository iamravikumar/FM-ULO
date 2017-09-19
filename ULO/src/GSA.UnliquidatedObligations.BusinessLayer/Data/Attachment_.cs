namespace GSA.UnliquidatedObligations.BusinessLayer.Data
{
    [TableKey("Attachments")]
    public partial class Attachment : ISoftDelete
    {
        public bool IsDeleted
        {
            get; private set;
        }

        string ISoftDelete.DeleteKey
            => AttachmentsId.ToString();

        public void Delete(string deletorUserId)
        {
            IsDeleted = true;
            DeletedByUserId = deletorUserId;
        }
    }
}
