using System.Collections.Generic;
using GSA.UnliquidatedObligations.BusinessLayer.Data;

namespace GSA.UnliquidatedObligations.Web.Models
{

    public class UloViewModel
    {
        public UnliquidatedObligation CurretUnliquidatedObligation;
        public IEnumerable<Region> Regions;
    }

    public class FormAModel
    {
        public string Field0Value { get; set; }

        public FormAModel()
        { }

        public FormAModel(Workflow wf)
        {

        }
    }
}