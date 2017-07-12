using System;

namespace GSA.UnliquidatedObligations.BusinessLayer.Data
{
    public partial class Review
    {
        public DateTime CreatedAt
            => CreatedAtUtc.ToLocalTime();

        public ReviewScopeEnum ReviewScope
        {
            get => (ReviewScopeEnum)ReviewScopeId;
            set { ReviewScopeId = (int)value; }
        }

        public ReviewTypeEnum ReviewType
        {
            get => (ReviewTypeEnum)ReviewTypeId;
            set { ReviewTypeId = (int)value; }
        }
    }
}
