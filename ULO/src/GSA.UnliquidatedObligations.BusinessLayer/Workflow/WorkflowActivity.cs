using System;
using System.Runtime.Serialization;

namespace GSA.UnliquidatedObligations.BusinessLayer.Workflow
{
    [DataContract(Name= "WorkflowActivity", Namespace = UloHelpers.WorkflowDescUrn)]
    public abstract class WorkflowActivity
    {
        [DataMember(Name="Key")]
        public string WorkflowActivityKey { get; set; }

        [DataMember(Name = "Name")]
        public string ActivityName { get; set; }

        [DataMember(Name = "SequenceNumber")]
        public int SequenceNumber { get; set; }

        [DataMember(Name = "OwnerUserName")]
        public string OwnerUserName { get; set; }

        public Type NextActivityChooserType
        {
            get { return Type.GetType(NextActivityChooserTypeName); }
        }

        [DataMember(Name="DueIn")]
        public TimeSpan? DueIn { get; set; }

        [DataMember(Name = "NextActivityChooserType")]
        public string NextActivityChooserTypeName { get; set; }

        [DataMember(Name = "NextActivityChooserConfig")]
        public string NextActivityChooserConfig { get; set; }

        [DataMember(Name = "EmailTemplateId")]
        public int EmailTemplateId { get; set; }

        [DataMember(Name = "AllowDocumentsEdit")]
        public bool AllowDocumentEdit { get; set; }
    }
}
