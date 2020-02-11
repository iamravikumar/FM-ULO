﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace GSA.UnliquidatedObligations.Web.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "16.4.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool UseDevAuthentication {
            get {
                return ((bool)(this["UseDevAuthentication"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("%temp%")]
        public string DocPath {
            get {
                return ((string)(this["DocPath"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool AllowFileShareInfo {
            get {
                return ((bool)(this["AllowFileShareInfo"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Reassign Group")]
        public string ReassignGroupUserName {
            get {
                return ((string)(this["ReassignGroupUserName"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool UseHangfireDashboards {
            get {
                return ((bool)(this["UseHangfireDashboards"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("PreAssignment")]
        public string PreAssignmentUserUsername {
            get {
                return ((string)(this["PreAssignmentUserUsername"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("https://secureauth.dev.gsa.gov/SecureAuth199/SecureAuth.aspx")]
        public string SecureAuthUrl {
            get {
                return ((string)(this["SecureAuthUrl"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("jonathan.chen@gsa.gov")]
        public string AdminstratorEmail {
            get {
                return ((string)(this["AdminstratorEmail"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("ULO")]
        public string ApplicationName {
            get {
                return ((string)(this["ApplicationName"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("1")]
        public int ManualReassignmentEmailTemplateId {
            get {
                return ((int)(this["ManualReassignmentEmailTemplateId"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("PostAuthToken199")]
        public string SecureAuthCookieName {
            get {
                return ((string)(this["SecureAuthCookieName"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Debug")]
        public global::Serilog.Events.LogEventLevel LogLevel {
            get {
                return ((global::Serilog.Events.LogEventLevel)(this["LogLevel"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute(@"
          AR,AR
          CL,CL
          CT,CT
          EJ,EJ
          EK,EK
          EN,EN
          EP,EP
          EQ,EQ
          GP,GP
          GX,GX
          IX,IX
          LO,LO
          LR,LR
          LU,LU
          LY,LY
          1B,1B
          PJ,PJ
          PN,PN
          PS,PS
          PX,PX
          PY,PY
          QP,QP
          RB,RB
          RO,RO
          S2,S2
          UE,UE
        ")]
        public string DocTypesCsv {
            get {
                return ((string)(this["DocTypesCsv"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("\r\n          Region,Region Workflow\r\n          WholeAgency,ULO Workflow\r\n        ")]
        public string ReviewScopeWorkflowMapCsv {
            get {
                return ((string)(this["ReviewScopeWorkflowMapCsv"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("123abc")]
        public string DevLoginPassword {
            get {
                return ((string)(this["DevLoginPassword"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("audio/*|video/*|image/*|.txt|.pdf|.doc|.docx|.xls|.xlsx|.zip|.csv|.ppt|.pptx")]
        public string AttachmentFileUploadAccept {
            get {
                return ((string)(this["AttachmentFileUploadAccept"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Only images, videos, audio files, documents, and archives are accepted.")]
        public string AttachmentFileUploadAcceptMessage {
            get {
                return ((string)(this["AttachmentFileUploadAcceptMessage"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Region Review,Region Approval,CO Review 1,CO Review 2,Action Needed,Deobligate,Co" +
            "mplete")]
        public string ReviewStatusOrdering {
            get {
                return ((string)(this["ReviewStatusOrdering"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool ShowSprintName {
            get {
                return ((bool)(this["ShowSprintName"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool ForceAdvanceFromUloSubmit {
            get {
                return ((bool)(this["ForceAdvanceFromUloSubmit"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("R.00,R.01,R.02,R.03,R.04,R.05,R.06,R.07,R.08,R.09,R.10,R.11")]
        public string RetaSheetsToImport {
            get {
                return ((string)(this["RetaSheetsToImport"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("00,01,02,03,04,05,06,07,08,09,10,11,Blank")]
        public string Upload192SheetsToImport {
            get {
                return ((string)(this["Upload192SheetsToImport"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool RunHangfireServer {
            get {
                return ((bool)(this["RunHangfireServer"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("3")]
        public int Upload192SkipRawRows {
            get {
                return ((int)(this["Upload192SkipRawRows"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool GetEligibleReviewersQualifiedOnly {
            get {
                return ((bool)(this["GetEligibleReviewersQualifiedOnly"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("{0} +")]
        public string GetEligibleReviewersQualifiedUsernameFormat {
            get {
                return ((string)(this["GetEligibleReviewersQualifiedUsernameFormat"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("{0} -")]
        public string GetEligibleReviewersNotQualifiedUsernameFormat {
            get {
                return ((string)(this["GetEligibleReviewersNotQualifiedUsernameFormat"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool UseOldGetEligibleReviewersAlgorithm {
            get {
                return ((bool)(this["UseOldGetEligibleReviewersAlgorithm"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool SendBatchEmailsDuringAssignWorkflows {
            get {
                return ((bool)(this["SendBatchEmailsDuringAssignWorkflows"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Closed")]
        public string TheCloserUserUsername {
            get {
                return ((string)(this["TheCloserUserUsername"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Someone edited this record while you were working.  You\'ll need to reload this pa" +
            "ge and re-apply your changes if you still have edit rights.")]
        public string StaleWorkflowErrorMessageTemplate {
            get {
                return ((string)(this["StaleWorkflowErrorMessageTemplate"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Report 1")]
        public string CreditCardAliasSheetsToImport {
            get {
                return ((string)(this["CreditCardAliasSheetsToImport"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("1")]
        public int CreditCardAliasSkipRawRows {
            get {
                return ((int)(this["CreditCardAliasSkipRawRows"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Sheet1")]
        public string PegasysOpenItemsCreditCardsSheetsToImport {
            get {
                return ((string)(this["PegasysOpenItemsCreditCardsSheetsToImport"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("3")]
        public int PegasysOpenItemsCreditCardsSkipRawRows {
            get {
                return ((int)(this["PegasysOpenItemsCreditCardsSkipRawRows"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public int RetaSkipRawRows {
            get {
                return ((int)(this["RetaSkipRawRows"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Sheet1")]
        public string ActiveCardholderSheetsToImport {
            get {
                return ((string)(this["ActiveCardholderSheetsToImport"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public int ActiveCardholderSkipRawRows {
            get {
                return ((int)(this["ActiveCardholderSkipRawRows"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("http://2ulo.azurewebsites.net/")]
        public string SiteUrl {
            get {
                return ((string)(this["SiteUrl"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Dalmation")]
        public string SprintName {
            get {
                return ((string)(this["SprintName"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public string ShowSprintNameOnFooter {
            get {
                return ((string)(this["ShowSprintNameOnFooter"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("2")]
        public int BatchAssignmentNotificationEmailTemplateId {
            get {
                return ((int)(this["BatchAssignmentNotificationEmailTemplateId"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Central Standard Time")]
        public string TimezoneId {
            get {
                return ((string)(this["TimezoneId"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Region Review,Region Approval,BGP Review 1,BGP Review 2,Action Needed,Deobligate")]
        public string MyTasksTabsCsv {
            get {
                return ((string)(this["MyTasksTabsCsv"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("01:00:00")]
        public global::System.TimeSpan DatabaseSprocCommandTimeout {
            get {
                return ((global::System.TimeSpan)(this["DatabaseSprocCommandTimeout"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("gsa.gov,guidehouse.com")]
        public string ReportRecipientEmailDomains {
            get {
                return ((string)(this["ReportRecipientEmailDomains"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("4")]
        public int ReportEmailTemplateId {
            get {
                return ((int)(this["ReportEmailTemplateId"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Payment,Receipt,Modification,X,Y,Z")]
        public string FinancialActivityTypes {
            get {
                return ((string)(this["FinancialActivityTypes"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string Setting {
            get {
                return ((string)(this["Setting"]));
            }
        }
    }
}
