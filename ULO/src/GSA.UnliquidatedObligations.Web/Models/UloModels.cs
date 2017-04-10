using System.Collections.Generic;
using System.Dynamic;
using GSA.UnliquidatedObligations.BusinessLayer.Data;

namespace GSA.UnliquidatedObligations.Web.Models
{

    public class UloViewModel
    {
        public UnliquidatedObligation CurretUnliquidatedObligation;
        public string UDOShouldBe { get; set; }
        public string DOShouldBe {get; set; }

        public UloViewModel()
        { }

        public UloViewModel(Workflow wf)
        { }
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