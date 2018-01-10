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
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "15.5.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("http://ulo.azurewebsites.net/")]
        public string SiteUrl {
            get {
                return ((string)(this["SiteUrl"]));
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
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool RunHangfireServer {
            get {
                return ((bool)(this["RunHangfireServer"]));
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
        [global::System.Configuration.DefaultSettingValueAttribute("Zephyr")]
        public string SprintName {
            get {
                return ((string)(this["SprintName"]));
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
          *,*
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
        [global::System.Configuration.DefaultSettingValueAttribute("Eastern Standard Time")]
        public string TimezoneId {
            get {
                return ((string)(this["TimezoneId"]));
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
        [global::System.Configuration.DefaultSettingValueAttribute("00,01,02,03,04,05,06,07,08,09,10,11")]
        public string Upload192SheetsToImport {
            get {
                return ((string)(this["Upload192SheetsToImport"]));
            }
        }
    }
}
