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
    
    public partial class tblFS_UserEntryData
    {
        public int EntryID { get; set; }
        public int EntryType { get; set; }
        public string FiscalYear { get; set; }
        public string BookMonth { get; set; }
        public string OrgCode { get; set; }
        public string FunctionCode { get; set; }
        public string OCCode { get; set; }
        public string DocNumber { get; set; }
        public decimal Amount { get; set; }
        public string Explanation { get; set; }
        public System.DateTime LastUpdated { get; set; }
        public Nullable<int> UpdateUserID { get; set; }
        public bool ActiveEntry { get; set; }
    }
}