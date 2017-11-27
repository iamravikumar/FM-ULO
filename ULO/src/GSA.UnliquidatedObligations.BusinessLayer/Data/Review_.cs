using System;

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

        public DateTime CreatedAt
            => CreatedAtUtc.ToLocalTime();

        public ReviewScopeEnum ReviewScope
        {
            get { return (ReviewScopeEnum)ReviewScopeId; }
            set { ReviewScopeId = (int)value; }
        }

        public ReviewTypeEnum ReviewType
        {
            get { return (ReviewTypeEnum)ReviewTypeId; }
            set { ReviewTypeId = (int)value; }
        }

        public void SetStatusDependingOnClosedBit(bool? isClosed=null)
        {
            IsClosed = isClosed.GetValueOrDefault(IsClosed);
            Status = IsClosed ? StatusNames.Closed : StatusNames.Open;
        }

        public bool IsDeleted
        {
            get; private set;
        }

        string ISoftDelete.DeleteKey
            => ReviewId.ToString();

        public void Delete(string deletorUserId=null)
        {
            IsDeleted = true;
            DeletedByUserId = deletorUserId;
        }
    }
}

