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
    
    public partial class tblEmailRequest
    {
        public int EmailRequestID { get; set; }
        public System.DateTime RequestDate { get; set; }
        public int SenderUserID { get; set; }
        public int HistoryAction { get; set; }
        public int EmailStatus { get; set; }
        public System.DateTime LastUpdate { get; set; }
    }
}
