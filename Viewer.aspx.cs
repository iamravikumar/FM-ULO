namespace GSA.OpenItems.Web
{
    using System;
    using GSA.OpenItems;

    public partial class Viewer : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            
            if (Request.QueryString["id"] != null && Request.QueryString["id"] != "")
            {           
                int doc_id;
                if (Int32.TryParse(Request.QueryString["id"], out doc_id))
                {
                    var doc = new Document(doc_id);

                    if (doc != null)
                    {
                        try
                        {
                            Response.Clear();
                            //Response.AddHeader("OpenItemDocument", "attachment; filename:" + doc.FileName);
                            Response.AddHeader("Content-Disposition", "inline; filename=" + doc.FileName);
                            //Response.AddHeader("Content-Disposition", "attachment; filename=" + doc.FileName);
                            Response.ContentType = doc.ContentType;
                            Response.OutputStream.Write(doc.FileData, 0, doc.FileSize);
                        }
                        catch (Exception ex)
                        {
                            Response.Write("<font color='red'>Unfortunately the document you selected is no longer available and cannot be restored or recreated.  Our apologies for this inconvenience.</font> ");
                        }
                    }
                    else
                        Response.Write("File Not Found.");
                }
                else
                    Response.Write("File Not Found.");
            }

            /*
            string id = Request.QueryString["id"];
            
            string[] arr = id.Split(new char[] { ',' });

            DataAccess da = new DataAccess(ConfigurationManager.ConnectionStrings["DefaultConn"].ConnectionString);
            SqlParameter[] arrParams;
            int file_id;
            //Response.ClearHeaders();
            Response.Clear();            
            //Response.Flush();

            foreach (string sid in arr)
            {
                file_id = Int32.Parse(sid);
                arrParams = new SqlParameter[1];
                arrParams[0] = new SqlParameter("@FileID", SqlDbType.Int);
                arrParams[0].Value = file_id;
                SqlDataReader dr = da.GetDataReader("spGetFile", arrParams);
                if (dr.Read())
                {
                    Response.AddHeader("Content-Disposition", "inline; filename=" + (string)dr["FileName"]);
                    //Response.AddHeader("Content-Disposition", "attachment; filename=" + doc.FileName);
                    Response.ContentType = (string)dr["ContentType"];
                    Response.OutputStream.Write((byte[])dr["FileData"], 0, (int)dr["FileSize"]);
                }
            }
            
            Response.End();
             */
        }
    }
}