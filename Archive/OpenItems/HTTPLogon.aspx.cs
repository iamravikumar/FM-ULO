namespace GSA.OpenItems.Web
{
    using System;
    using System.Xml;

    public partial class HTTPLogon : PageBase
    {

        private readonly EmailsBO Email;

        public HTTPLogon()
        {
            Email =  new EmailsBO(this.Dal);
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            var str_result = "";
            var obj_xml = new XmlDataDocument();

            try
            {
                obj_xml.Load(Request.InputStream);

                if (obj_xml.DocumentElement.Name == "send_pswd")
                {
                    var node = obj_xml.DocumentElement.FirstChild;

                    var user_name = node.Attributes.GetNamedItem("user").Value;
                    var sPath = Server.MapPath("~");
                    
                    if (Email.SendPassword(user_name, sPath) == 0)
                        str_result = "<error><err_msg msg='The Username is not valid. Please try again.' /></error>";
                }

                if (str_result == "")
                    str_result = "<ok/>";
            }
            catch (Exception ex)
            {
                str_result = "<error><err_msg msg='" + ex.Message + "' /></error>";
            }

            Response.ContentType = "text/xml";
            Response.Write(str_result);
            Response.Flush();

        }
    }
}
