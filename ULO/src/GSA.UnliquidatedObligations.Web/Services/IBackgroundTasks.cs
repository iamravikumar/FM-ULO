using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSA.UnliquidatedObligations.Web.Services
{
    public interface IBackgroundTasks
    {
        void Email(string subject, string recipient, string template, object model);

        void UploadReviewHoldIngTable(int reviewId, string uploadPath);
    }
}
