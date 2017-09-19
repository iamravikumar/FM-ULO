using System;

namespace GSA.UnliquidatedObligations.BusinessLayer.Data
{
    public partial class Review
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
    }
}

