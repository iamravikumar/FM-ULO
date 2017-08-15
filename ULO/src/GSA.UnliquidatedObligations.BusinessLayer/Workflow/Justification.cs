using System.Runtime.Serialization;

namespace GSA.UnliquidatedObligations.BusinessLayer.Workflow
{
    [DataContract(Name = "Justification", Namespace = UloHelpers.WorkflowDescUrn)]
    public class Justification
    {
        [DataMember(Name = "Key", IsRequired =true)]
        public string Key { get; set; }

        [DataMember(Name = "Description")]
        public string Description { get; set; }
    }
}
