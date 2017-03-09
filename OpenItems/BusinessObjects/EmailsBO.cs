using OpenItems.Properties;

namespace GSA.OpenItems
{
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using System.IO;
    using System.Configuration;
    using Data;

    /// <summary>
    /// Summary description for EmailsBO
    /// </summary>
    public class EmailsBO
    {
        private readonly IEmailDataLayer Dal;

        public EmailsBO(IEmailDataLayer dal)
        {
            Dal = dal;
        }


        public int SendPassword(string sUsername, string sAppPath)
        {

            var ds = Dal.GetUserByUsername(sUsername);

            if (ds == null || ds.Tables[0].Rows.Count == 0 || ds.Tables[0].Rows[0]["Password"] == DBNull.Value)
                return 0;
            else
            {
                var password = ds.Tables[0].Rows[0]["Password"].ToString();

                var file_name = sAppPath + Settings.Default.ForgotPswdFile;
                var str_subject = Utility.GetFileText(file_name, "SUBJECT");
                var str_body = Utility.GetFileText(file_name, "BODY");
                str_body = str_body.Replace("[p_pswd]", password);
                var to = new string[1];
                to[0] = sUsername + "@gsa.gov";

                var email = new EmailUtility();
                email.Subject = str_subject;
                email.MessageBody = str_body;
                email.To = to;
                email.SendEmail();

                return 1;
            }
        }

        public void SendEmail(string sTo, string sCc, string sSubject, string sBody, string sCurrentUserLogin)
        {
            var email = new EmailUtility();
            email.Subject = sSubject;
            email.To = sTo.Split(new char[] { ',' });
            email.Cc = sCc.Split(new char[] { ',' });
            email.MessageBody = sBody;
            email.IsBodyHTML = false;

            if (sCurrentUserLogin.IndexOf("@") == -1)
                email.From = sCurrentUserLogin + "@gsa.gov";

            email.SendEmail();

        }

        public void SendEmail(string sTo, string sCc, string sSubject, string sBody, string sAttachmentDocIDs, string sCurrentUserLogin)
        {
            var doc_array = sAttachmentDocIDs.Split(new char[] { ',' });

            var email = new EmailUtility();
            email.Subject = sSubject;
            email.To = sTo.Split(new char[] { ',' });
            if (sCc.Length > 0)
                email.Cc = sCc.Split(new char[] { ',' });
            email.MessageBody = sBody;
            email.IsBodyHTML = false;

            if (sCurrentUserLogin.IndexOf("@") == -1)
                email.From = sCurrentUserLogin + "@gsa.gov";

            email.AttachmentsBinary = new EmailUtility.AttachmentContent[doc_array.Length];

            for (var i = 0; i < doc_array.Length; i++)
            {
                var doc_id = 0;
                if (doc_array[i].Length > 0 && Int32.TryParse(doc_array[i], out doc_id))
                {
                    var doc = new Document(doc_id);
                    email.AttachmentsBinary[i].FileName = doc.FileName;
                    email.AttachmentsBinary[i].ContentType = doc.ContentType;
                    var ms = new MemoryStream(doc.FileData, 0, doc.FileSize);
                    email.AttachmentsBinary[i].DataStream = ms;
                }
            }

            email.SendEmail();

        }

        public int InsertEmailRequest(int iCurrentUserID, int iHistoryAction, bool bSendNow)
        {
            
                //some emails ready to be sent immidiately - such as Reassign, RerouteRequest, etc...
                //emails on Verification / first time Assign will be sent on special user's request (by click on "Sent Email" button)
                //and now the records to the EmailRequests table will be inserted in the status "Pending".

            return Dal.InsertEmailRequest(iCurrentUserID, iHistoryAction, bSendNow);
        }

        public void SendAssignInfoEmails(int iCurrentUserID, int iSelectedLoad)
        {
            //this function will update all Email Requests from status 1 = 'Pending' to status 2 = 'Ready to Send'
            //for specific User, selected Load and following History actions:
            //HistoryActions.haAssignmentVerification = 4 and HistoryActions.haReviewerAssignment = 3
            Dal.ActivateAssignEmailRequest(iCurrentUserID, iSelectedLoad);

        }
    }
}