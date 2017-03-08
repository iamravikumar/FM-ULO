using System;
using System.Data;
using System.Web.UI.HtmlControls;
using GSA.OpenItems;
using System.Configuration;

namespace Controls
{
    public partial class DropDownGrid : Grid
	{
        //SM - loading all columns, showing ALL columns except ID - no parameters
        public void FillDropDownGridAllColumnsVisibleButID(string SPName, string sIDColumn, string sValueColumn, string sDefaultItem, bool bHeader)
        {
            DataAccess oData;
            if (Session["cn"] != null)
            {
                 oData = new DataAccess(Session["cn"].ToString());
            }
            else
            {
                //throw new Exception("Your session has been expired. Please logout and login again");
                 oData = new DataAccess(ConfigurationManager.ConnectionStrings["DefaultConn"].ConnectionString);
            }
            DataTable dt;
            //TODO: Need to figure out how to handle this in business layer
            dt = oData.GetDataSet(SPName, "TableName").Tables[0];

            for (var i = 0; i <= dt.Columns.Count - 1; i++)
            {
                if (dt.Columns[i].ColumnName == sIDColumn)
                    dt.Columns[i].ReadOnly = true;
            }

            var row = dt.NewRow();

            if (sDefaultItem.Trim() != "")
            {
                row[0] = 0;
                row[1] = sDefaultItem;
                dt.Rows.InsertAt(row, 0);
            }

            dt.Columns[sIDColumn].ReadOnly = true; //hidden column


            this.Table = dt;
            if (bHeader == true)
            {
                ShowHeader = true;
            }
            else
            {
                ShowHeader = false;
            }
            KeepInViewState = true;
            ShrinkOnBlur = true;
            ShowExpandButton = true;
            this.ItemCSSClass = "Art";
            this.TblCSSClass = "tblGrid";
        }
        public void EnableDropDownGrid(bool bEnabled)
        {
            if (bEnabled == false)
            {
                this.TblCSSClass = "tblGrid_Alt";
                this.ItemCSSClass = "ddgDisabled";
                this.ShowExpandButton = false;
            }
            else
            {
                this.TblCSSClass = "tblGrid";
                this.ItemCSSClass = "Art";
                this.ShowExpandButton = true;
            }
        }

        public void FillDropDownGridAllColumnsVisibleButID(string SPName, string sIDColumn, string sValueColumn,  string sDefaultItem,string sParamName, string sParamValue, bool bHeader)
        {
            DataAccess oData;

            if (Session["cn"] != null)
            {
                oData = new DataAccess(Session["cn"].ToString());
            }
            else
            {
                 oData = new DataAccess(ConfigurationManager.ConnectionStrings["DefaultConn"].ConnectionString);
            }

            DataTable dt;
            //TODO: Need to figure out how to handle this in business layer
            if (sParamName == "")
            {
                dt = oData.GetDataSet(SPName, "TableName").Tables[0];
            }
            else
            {
                dt = oData.GetDataSet(SPName, "TableName", sParamName, sParamValue).Tables[0];
            }


            for (var i = 0; i <= dt.Columns.Count - 1; i++)
            {
                if (dt.Columns[i].ColumnName == sIDColumn)
                    dt.Columns[i].ReadOnly = true;
            }

            var row = dt.NewRow();

            if (sDefaultItem.Trim() != "")
            {
                row[0] = 0;
                row[1] = sDefaultItem;
                dt.Rows.InsertAt(row, 0);
            }

            dt.Columns[sIDColumn].ReadOnly = true; //hidden column


            this.Table = dt;
            if (bHeader == true)
            {
                ShowHeader = true;
            }
            else
            {
                ShowHeader = false;
            }
            KeepInViewState = true;
            ShrinkOnBlur = true;
            ShowExpandButton = true;
            this.ItemCSSClass = "Art";
            this.TblCSSClass = "tblGrid";
        }

        // insert item (only value without ID) to grid with records
        public void InsertToDropDownGrid(string sValue, bool bEnabled, DataTable dt)
        {
            var DefaultRow = dt.NewRow();
            DefaultRow[0] = sValue;
            dt.Rows.Add(DefaultRow);
            //dt.Columns[sIDColumn].ReadOnly = true; //hidden column
            this.Table = dt;
            this.KeepInViewState = true;
            this.ShowHeader = false;
            if (bEnabled == false)
            {
                this.ItemCSSClass = "ddgDisabled";
                this.TblCSSClass = "tblGrid_Alt";
                this.ShowExpandButton = false;
            }
            else
            {
                //this.ItemCSSClass = "Art";
                //this.TblCSSClass = "tblGrid";
                this.ShowExpandButton = true;
            }
        }
        public void DropDownGridSelectedItem(string s, DataTable dt, bool bEnabled)
        {
            //DataTable dt = this.Table;

            var i = 0;
            while (i < dt.Rows.Count - 1 && dt.Rows[i][0].ToString() != s)
            {
                i = i + 1;
                this.SelectedIndex = i;
            }

            if (bEnabled == false)
            {
                this.ItemCSSClass = "ddgDisabled";
                this.TblCSSClass = "tblGrid_Alt";
                this.ShowExpandButton = false;
            }
        }

        public void DropDownGridSelectedItemByValue(string s, DataTable dt, bool bEnabled)
        {

            for (var i = 0; i < dt.Rows.Count; i++)
            {
                if (dt.Rows[i][1].ToString() == s)
                {
                    this.SelectedIndex = i;
                    break;
                }
            }

            if (bEnabled == false)
            {
                this.ItemCSSClass = "ddgDisabled";
                this.TblCSSClass = "tblGrid_Alt";
                this.ShowExpandButton = false;
            }
        }

        public string ImageButtonSrc
        {
            set { img.Src = value; }
        }
        public int tabIndex
        {
            set { ArtDD.Attributes["tabIndex"] = value.ToString(); }
        }
        public string TblBorderClass
        {
            set { ArtDD.Attributes["class"] = value; }
        }
        public bool ShrinkOnBlur
		{
			get { return (dgIndex.TabIndex & 4) == 0; }
			set { if (value)
					dgIndex.TabIndex &= 0x0FB;
				  else
					dgIndex.TabIndex = (short)((ushort)dgIndex.TabIndex | 4);
				}
		}
		public bool ShowExpandButton
		{
			get { return (dgIndex.TabIndex & 2) != 0; }
			set { if (value)
					dgIndex.TabIndex = (short)((ushort)dgIndex.TabIndex | 2);
				  else
					dgIndex.TabIndex &= 0x0FD;
				}
		}
		public string ItemID
		{
            get { return (SelectedIndex < 0) ? "" : dgIndex.Text.Substring(dgIndex.Text.IndexOf(";") + 1); }
        }
		public int SelectedIndex
		{
			get {	var i = dgIndex.Text.IndexOf(";");
					if (i >= 0)
						return  Convert.ToInt32(dgIndex.Text.Substring(0, i));
					if (ArtDG.Rows.Count > 1)
					{
						dgIndex.Text = "0;" + ArtDG.Rows[1].Cells[0].InnerText;
						return 0;
					}
					dgIndex.Text = "";
					return -1;
				}
			set {	var i = value;
					dgIndex.Text = "";
					if (i >= ArtDG.Rows.Count-1)
						i = ArtDG.Rows.Count-2;
					if (i < 0)
						return;
					dgIndex.Text = i.ToString() + ";" + ArtDG.Rows[i+1].Cells[0].InnerText;
				}
		}
		public bool ExpandOnLoad
		{
			get { return (dgIndex.TabIndex & 32) != 0; }
			set 
			{
				if (value)
					dgIndex.TabIndex = (short)((ushort)dgIndex.TabIndex | 32);
				else
					dgIndex.TabIndex &= 0x0DF;
			}
		}
		protected void Page_Load(object sender, System.EventArgs e)
		{
			Grid_Load();
		}
        public void MyRender()
        {
            ArtDD.Visible = true;
            if (!ShowExpandButton || ArtDG.Rows.Count < 3)
                tdExp.Style.Add("display", "none");
            SelectedIndex = SelectedIndex;
            Grid_Save();

            var s = "url('" + this.TemplateSourceDirectory + "/c_asc.cur'),default";
            for (var j = 0; j < ArtDG.Rows[0].Cells.Count; j++)
            {
                ArtDG2.Rows[0].Cells.Add(new HtmlTableCell());
                ArtDG2.Rows[0].Cells[j].InnerHtml = ArtDG.Rows[0].Cells[j].InnerHtml;
                if (SelectedIndex >= 0)
                {
                    ArtDG2.Rows[1].Cells.Add(new HtmlTableCell());
                    ArtDG2.Rows[1].Cells[j].InnerHtml = ArtDG.Rows[SelectedIndex+1].Cells[j].InnerHtml;
                }

                var t = Convert.ToInt32(ArtDG.Rows[0].Cells[j].Attributes["tabIndex"]);
                ArtDG2.Rows[0].Cells[j].Attributes["tabIndex"] = t.ToString();
                if (-t > 4)
                {
                    ArtDG2.Rows[0].Cells[j].Style.Add("display","none");
                    if (SelectedIndex >= 0)
                        ArtDG2.Rows[1].Cells[j].Style.Add("display", "none");
                }
                if (Sorting)
                {
                    ArtDG2.Rows[0].Cells[j].Attributes["title"] = "Ascending sort";
                    ArtDG2.Rows[0].Cells[j].Style.Add("cursor", s);
                }
                
            }
            if (SelectedIndex >= 0)
                ArtDG2.Rows[1].Attributes.Add("tabindex", "-"+(SelectedIndex +1).ToString());
            else
                ArtDG2.Rows[1].Attributes["display"] = "none";
            ArtDG2.Attributes["class"] = TblCSSClass;
            ArtDG2.CellSpacing = CellSpacing;
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
