namespace GSA.OpenItems.Web
{
    using System;

    public partial class Organizations : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //string sLoadID = "43";
            //DataSet ds = Item.GetOrgItemPerLoadID(sLoadID);
            //int iID = 0;

            //foreach (DataRow dr in ds.Tables[0].Rows)
            //{
            //    try
            //    {
            //        string sOItemID = dr["OItemID"].ToString();
            //        string sULOOrgCode = dr["ULOOrgCode"].ToString();
            //        string sResponsibleOrg = dr["ResponsibleOrg"].ToString();
            //        string sReviewerUserID = dr["ReviewerUserID"].ToString();

            //        DataAccess da = new DataAccess(ConfigurationManager.ConnectionStrings["DefaultConn"].ConnectionString);
            //        SqlParameter[] arrParams = new SqlParameter[3]; //*** always check the correct amount of parameters!***

            //        arrParams[0] = new SqlParameter("@LoadID", SqlDbType.VarChar);
            //        arrParams[0].Value = sLoadID;

            //        arrParams[1] = new SqlParameter("@OItemID", SqlDbType.VarChar);
            //        arrParams[1].Value = sOItemID;

            //        arrParams[2] = new SqlParameter("@OrgCode", SqlDbType.VarChar);
            //        arrParams[2].Value = sULOOrgCode;

            //        da.SaveData("spCorrectOrgName", arrParams, out iID);

            //        if (iID > 0)
            //        {
            //            //lblError.Text = DisplayMessage("Record saved.", false);
            //        }
            //    }
            //    catch(Exception ex)
            //    {
            //        string sError = ex.Message;
                    
            //    }

            //}

        }
    }
}
