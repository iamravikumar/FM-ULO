using System;

namespace GSA.UnliquidatedObligations.BusinessLayer.Data
{
    public partial class Review
    {
        public DateTime CreatedAt
            => CreatedAtUtc.ToLocalTime();
    }
}
