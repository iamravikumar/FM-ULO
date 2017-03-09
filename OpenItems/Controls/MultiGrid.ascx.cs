using System;
using System.Data;
using System.Collections;
using GSA.OpenItems;
using System.Configuration;
using OpenItems.Properties;
using DataAccess = GSA.OpenItems.DataAccess;

namespace Controls
{
	public partial class MultiGrid : Grid
	{
        private string FooterClass = "";


        // fill mg with 1 parameters
        public void FillMultiGrid(string SPName, string sIDColumn, string sParamName, string sParamValue, bool bShowID, out int iCount)
        {
            //DataAccess oData = new DataAccess(Session["cn"].ToString());
            DataAccess oData;

            if (Session["cn"] != null)
            {
                oData = new DataAccess(Session["cn"].ToString());
            }
            else
            {
                //throw new Exception("Your session has been expired. Please logout and login again");
                 oData = new DataAccess(Settings.Default.DefaultConn);

            }
            DataTable dt;
            //TODO: Need to figure out how to handle this in business layer
            if (sParamName == "")
            {
                dt = oData.GetDataSet(SPName, "TableName").Tables[0];
                iCount = dt.Rows.Count;
            }
            else
            {
                dt = oData.GetDataSet(SPName, "TableName", sParamName, sParamValue).Tables[0];
                iCount = dt.Rows.Count;
            }


            var row = dt.NewRow();

            if (bShowID == false)
            {
                dt.Columns[sIDColumn].ReadOnly = true; //hidden column
            }
            this.Table = dt;
            ShowHeader = true;
            KeepInViewState = true;
            //ShrinkOnBlur = true;
            //ShowExpandButton = true;
            //this.ItemCSSClass = "Art";
            //this.TblCSSClass = "tblGrid";
        }

        public void EnableMultiGrid(bool bEnabled)
        {
            if (bEnabled == false)
            {
                this.TblCSSClass = "tblGrid_Alt";
                this.ItemCSSClass = "ddgDisabled";
            }
            else
            {
                this.TblCSSClass = "tblGrid";
                this.ItemCSSClass = "Art";
            }
        }


        public int tabIndex
        {
            set { ArtMM.Attributes["tabIndex"] = value.ToString(); }
        }
        public string TblBorderClass
        {
            set { ArtMM.Attributes["class"] = value; }
        }
		public string ItemID
		{
			get {	var s = dgIndex.Text;
					if (!MultiChoice)
					{
						var i = s.IndexOf(";");
						if (i >= 0)
							return s.Substring(i+1);
					}
					return "";
				}
		}
		public int SelectedIndex
		{
			get	{	var s = dgIndex.Text;
					if (!MultiChoice)
					{
						var i = s.IndexOf(";");
						if (i >= 0)
							return Convert.ToInt32(s.Substring(0, i));
					}
					else
					{
						if (SelectedIndexes.Length == 1)
							return SelectedIndexes[0];
					}
					return -1;
				}
			set {	var i = value;
					if (i < ArtDG.Rows.Count-1 && i >= 0 )
					{
						if (!MultiChoice)
							dgIndex.Text = i.ToString() + ";" + ArtDG.Rows[i+1].Cells[0].InnerText;
						else
							dgIndex.Text += i.ToString() + ";" ;
						return;
					}
					dgIndex.Text = "";
				}
		}
		public int[] SelectedIndexes
		{
			get {	var idx = dgIndex.Text.Split(new char[]{';'});
					var list = new ArrayList();
					if (MultiChoice)
					{			
						foreach (var s in idx) 
						{
							if (s != "")
								list.Add( Convert.ToInt32(s));
						}
					}
					else
					{
						if (SelectedIndex >= 0)
							list.Add(SelectedIndex);
					}
					var iIndexes = new int[list.Count];
					for(var i=0; i < list.Count; i++)
					{
						iIndexes[i] = (int)list[i];
					}
					return iIndexes;
				}
			set	{	dgIndex.Text = "";
					foreach (var i in value)
					{
						dgIndex.Text += i.ToString() + ";" ;
					}
				}
		}
		public string ToolTip
		{
			set { ArtDG.Attributes["title"] = value; }
		}
		public bool MultiChoice
		{
			get { return (dgIndex.TabIndex & 2) != 0; }
			set { if (value)
					dgIndex.TabIndex = (short)((ushort)dgIndex.TabIndex | 2);
				  else
					dgIndex.TabIndex &= 0x0FD;
				}
		}
		public bool Footer
		{
			get { return (dgIndex.TabIndex & 4) != 0; }
			set { if (value)
					dgIndex.TabIndex = (short)((ushort)dgIndex.TabIndex | 4);
				  else
					dgIndex.TabIndex &= 0x0FB;
				}
		}
        public string FooterCSSClass
        {
            set { FooterClass = value; }
        }
        protected void Page_Load(object sender, System.EventArgs e)
		{
			Grid_Load();
		}
        public void MyRender()
        {
            ArtMM.Visible = true;
            var idxs = SelectedIndexes;
            dgIndex.Text = "";
            foreach (var i in idxs)
            {
                SelectedIndex = i;
            }
            Grid_Save();
            if (Footer && ArtDG.Rows.Count > 1)
                ArtDG.Rows[ArtDG.Rows.Count - 1].Attributes["class"] = FooterClass == "" ? HeaderCSSClass : FooterClass;
        }
		protected void MyRender(object sender, System.EventArgs e)
		{
			MyRender();
		}
		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit(e);
		}
		
		/// <summary>
		///		Required method for Designer support - do not modify
		///		the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.PreRender += new System.EventHandler(this.MyRender);

		}
		#endregion
	}
}
