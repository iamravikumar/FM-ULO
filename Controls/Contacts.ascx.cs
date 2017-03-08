namespace GSA.OpenItems.Web
{
    using Data;
    using System;
    using System.Data;
    using System.Web.UI.HtmlControls;

    public partial class Controls_Contacts : System.Web.UI.UserControl
    {
        //TODO: Create UserControl Base class that defines these. Not able to use constructor.
        private static readonly IDataLayer Dal = new DataLayer(new zoneFinder());
        private readonly ItemBO Item = new ItemBO(Dal);

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                lblContactsErr.Visible = false;
                lblContactsErr.Text = "";
                if (!IsPostBack)
                {
                    lblCOfficer.Text = ContrOfficerByCO;

                    if (!Enabled)

                    {
                        btnAddContact.Disabled = true;
                        btnAddContact.Visible = false;
                    }


                }
                LoadContacts();
            }
            catch (Exception ex)
            {
                lblContactsErr.Visible = true;
                lblContactsErr.Text = ex.Message;
            }
        }        

        public string ContrOfficerByCO { get; set; }

        public bool Enabled { get; set; }


        public void LoadContacts()
        {
                var _page = new PageBase();
                var doc = _page.DocNumber;
                var org = _page.OrgCode;

                var ds = Item.GetItemContactList(doc, org);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    //if (tblContacts.Rows.Count > 0)
                    //{
                    //    tblContacts.Rows.Clear();
                    //}

                    //foreach (DataRow dr in tblContacts.Rows)
                    //{
                    //}
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        var td1 = new HtmlTableCell();
                        td1.Attributes.Add("style", "width:22px;");
                        if (Enabled)
                            td1.InnerHtml = "<img id='btnDeleteContact' src='../images/btn_contact_delete.gif'  title='remove contact'  class='icDelete' onclick='javascript:return remove_contact(this);'  alt='Remove Contact' />";
                        if(btnAddContact.Disabled ==false)
                            td1.InnerHtml = "<img id='btnDeleteContact' src='../images/btn_contact_delete.gif'  title='remove contact'  class='icDelete' onclick='javascript:return remove_contact(this);'  alt='Remove Contact' />";
                        var td2 = new HtmlTableCell();
                        td2.Attributes.Add("class", "regBldBlueText");
                        td2.InnerText = (string)dr["RoleDescription"];
                        var td3 = new HtmlTableCell();
                        td3.Attributes.Add("class", "regText");
                        td3.InnerText = (string)dr["FullName"];
                        var td4 = new HtmlTableCell();
                        if (dr["Phone"] != DBNull.Value)
                            td4.InnerHtml = "<label class='regText' >" + (string)dr["Phone"] + "</label>";                        
                        
                        var tr = new HtmlTableRow();
                        tr.Cells.Add(td1);
                        tr.Cells.Add(td2);
                        tr.Cells.Add(td3);
                        tr.Cells.Add(td4);
                        tblContacts.Rows.Add(tr);                        
                    }
                }
        }
    }
}