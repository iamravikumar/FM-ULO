namespace GSA.UnliquidatedObligations.BusinessLayer.Data
{
    [TableKey("Reviews")]
    public partial class Review : ISoftDelete
    {
        public static class StatusNames
        {
            public const string Creating = "Creating";
            public const string Assigning = "Assigning";
            public const string Open = "Open";
            public const string Closed = "Closed";
        }

        public void SetStatusDependingOnClosedBit(bool? isClosed=null)
        {
            IsClosed = isClosed.GetValueOrDefault(IsClosed);
            Status = IsClosed ? StatusNames.Closed : StatusNames.Open;
        }

       

        string ISoftDelete.DeleteKey
            => ReviewId.ToString();        

        bool ISoftDelete.IsDeleted
       => IsDeleted;

        private bool IsDeleted { get; set; }


        public void Delete(string deletorUserId=null)
        {
            IsDeleted = true;
            DeletedByUserId = deletorUserId;
        }

       
    }
}

