//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace OpenItems.Data
{
    using System;
    using System.Collections.Generic;
    
    public partial class tblOISendAttachment
    {
        public string DocNumber { get; set; }
        public int DocID { get; set; }
        public int LoadID { get; set; }
        public bool IncludeInEmail { get; set; }
        public Nullable<System.DateTime> LastEmailDate { get; set; }
        public bool IncludeRevisionEmail { get; set; }
        public Nullable<System.DateTime> LastRevisionEmail { get; set; }
        public System.DateTime LastUpdated { get; set; }
    }
}