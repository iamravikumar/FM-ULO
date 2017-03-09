using System;
using System.Data;
using System.Collections;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Globalization;

namespace Controls
{
    public class Grid : System.Web.UI.UserControl
	{
        /*
            1   -KeepInViewState
            2   -MultiChoice/ShowExpandButton
            4   -Footer/ShrinkOnBlur
            8   -ShowHeader
            16  -KeepInSession
            32  -/ExpandOnLoad
            64  -Visible
            128 -Enable Sorting
        */
        protected TextBox dgIndex;				// hidden params
        protected HtmlTable ArtDG;
        protected HtmlInputButton btnOnCnange;
		protected HtmlGenericControl DesignView;
        private string _TblCSSClass = "tblGrid";
        private string _HeaderCSSClass = "GridHeader";

        public Unit Width
        {
            set { dgIndex.Width = value; }
        }
        public Unit Height
        {
            set { dgIndex.Height = value; }
        }
        public bool Sorting
        {
            get { return (dgIndex.TabIndex & 128) == 0; }
            set
            {
                if (value)
                    dgIndex.TabIndex &= 0x7F;
                else
                    dgIndex.TabIndex = (short)((ushort)dgIndex.TabIndex | 128);
            }
        }
        public bool Hidden
		{
			get { return (dgIndex.TabIndex & 64) > 0; }
			set 
			{
				if (value)
					dgIndex.TabIndex = (short)((ushort)dgIndex.TabIndex | 64);
				else
					dgIndex.TabIndex &= 0x0BF;
			}
		}
		public string ondblclick				//Client event script
		{
            set { ArtDG.AddOnDblClick("if(__tr(event)) " + value); }
        }
		public string OnChange					//Client event script
		{
			set {	btnOnCnange.AddOnClick(value); }
		}
		public EventHandler OnChangeHandler		//Server event
		{
			get {	return null; }
			set {	btnOnCnange.ServerClick += value; }
		}
        public string ClientOnLoad
        {
            set { Session["_OnL"] = new string[] { value, Page.ToString() }; }
        }
        public string TblCSSClass
        {
            get { return _TblCSSClass; }
            set { _TblCSSClass = ArtDG.Attributes["class"] = value; }
        }
		public string HeaderCSSClass
		{
            get { return _HeaderCSSClass; }
            set { _HeaderCSSClass = ArtDG.Rows[0].Attributes["class"] = value; }
		}
        public string ItemCSSClass
		{
			get { return dgIndex.CssClass; }
			set { dgIndex.CssClass = value; }
		}
		public int CellSpacing
		{
			get { return ArtDG.CellSpacing; }
			set { ArtDG.CellSpacing = value; }
		}
		public bool ShowHeader
		{
			get { return (dgIndex.TabIndex & 8) == 0; }
			set 
			{
				if (value)
					dgIndex.TabIndex &= 0x0F7;
				else
					dgIndex.TabIndex = (short)((ushort)dgIndex.TabIndex | 8);
			}
		}
		public bool KeepInViewState
		{
			get { return (dgIndex.TabIndex & 1) != 0; }
			set 
			{
				if (value)
					dgIndex.TabIndex = (short)((ushort)dgIndex.TabIndex | 1);
				else
					dgIndex.TabIndex &= 0x0FE;
			}
		}
		public bool KeepInSession
		{
			get { return (dgIndex.TabIndex & 16) != 0; }
			set 
			{
				if (value)
					dgIndex.TabIndex = (short)((ushort)dgIndex.TabIndex | 16);
				else
					dgIndex.TabIndex &= 0x0EF;
			}
		}
        public void ClearTable()
		{
            ArtDG.Rows[0].Cells.Clear();
            for (var i = ArtDG.Rows.Count - 1; i > 0; i--)
			{
				ArtDG.Rows.RemoveAt(i);
			}
		}
		public DataTable Table
		{
			get
			{
				var dt = new DataTable();
				for (var i = 0; i < ArtDG.Rows[0].Cells.Count; i++)
				{
					var col = new DataColumn();
					col.ColumnName = ArtDG.Rows[0].Cells[i].InnerHtml;
					if (Convert.ToInt32(ArtDG.Rows[0].Cells[i].Attributes["tabIndex"]) < -4)
						col.ReadOnly = true;
					dt.Columns.Add(col);
				}
				for(var i=1; i<ArtDG.Rows.Count; i++)
				{
					var dtRow = dt.NewRow();
					for(var j=0; j<ArtDG.Rows[i].Cells.Count; j++)
					{
						dtRow[dt.Columns[j].ColumnName] = ArtDG.Rows[i].Cells[j].InnerHtml;
					}
					dt.Rows.Add(dtRow);
				}
				return dt;
			}
			set
			{
				ClearTable();
				var cols = value.Columns;
				var f = 0;
				var old_al = (ArrayList) ViewState[this.ClientID];
				if (old_al == null )
					old_al = (ArrayList) Session[this.ClientID];
				if (old_al != null && ((ArrayList) old_al[0]).Count == cols.Count)
				{
					f = 1;
					for (var j=0; j<cols.Count; j++)
					{
						if (cols[j].ToString()!=((ArrayList) old_al[0])[j].ToString())
						{
							f = 0; break;
						}
					}
				}
				var type = new int[cols.Count];
				for (var j=0; j<cols.Count; j++)
				{
                    ArtDG.Rows[0].Cells.Add(new HtmlTableCell());
                    ArtDG.Rows[0].Cells[j].InnerHtml = cols[j].ToString();
                    if (f == 0)
					{
						switch( cols[j].DataType.ToString() )
						{   
							case "System.String":
							case "System.Boolean":
							case "System.Char":
							case "System.TimeSpan":
								type[j] = 1; 
								break;
							case "System.DateTime":
								type[j] = 2;
								break;      
							default:		//Number
								type[j] = 3;
								break;      
						}
					}
					else
						type[j] = -Convert.ToInt32(((ArrayList) old_al[old_al.Count-1])[j]);

					if (cols[j].ReadOnly)
						type[j] |= 4;
					else
						type[j] &= 3;

                    ArtDG.Rows[0].Cells[j].Attributes["tabIndex"] = (-type[j]).ToString();

					if (type[j] > 4)
                        ArtDG.Rows[0].Cells[j].Style.Add("display", "none");

				}
				var rows = value.Rows;
				HtmlTableRow HRow;
				HtmlTableCell HCell;
				if (rows.Count==0)
					return;

				for (var i=0; i<rows.Count; i++)
				{
					HRow = new HtmlTableRow();
					for (var j=0; j<rows[i].ItemArray.Length; j++)
					{
						HCell = new HtmlTableCell();
						if (rows[i][j].GetType()!= typeof(DateTime))
							HCell.InnerHtml = rows[i][j].ToString();
                        else
                            HCell.InnerHtml = Convert.ToDateTime(rows[i][j]).ToShortDateString().Replace(".", "/").Replace("-", "/");

                        if (type[j] == 3 || cols[j].ColumnName.Contains("$"))
                        {
                            HCell.Style.Add("white-space", "nowrap");
                            HCell.Attributes.Add("align", "right");
                        }

                        if (type[j] > 4)
                            HCell.Style.Add("display", "none");

						HRow.Cells.Add(HCell);
					}
					ArtDG.Rows.Add(HRow);
				}
			}
		}
        protected void Grid_Save()
        {
            DesignView.Visible = false;

            for (var i = 1; i < ArtDG.Rows.Count; i++)
            {
                ArtDG.Rows[i].AddCssClass(i % 2 != 0 ? ItemCSSClass : ItemCSSClass + "Alt");
                ArtDG.Rows[i].AddTabIndex(0 - i);
            }
            if (Sorting)
            {
                var s = "url('" + this.TemplateSourceDirectory + "/c_asc.cur'),default";
                dgIndex.Style.Add("cursor", s);
                for (var j = 0; j < ArtDG.Rows[0].Cells.Count; j++)
                {
                    ArtDG.Rows[0].Cells[j].Attributes["title"] = "Ascending sort";
                    ArtDG.Rows[0].Cells[j].Style.Add("cursor", s);
                }
            }
            if (KeepInViewState || KeepInSession)
            {
                var al = new ArrayList();
                for (var i = 0; i < ArtDG.Rows.Count; i++)
                {
                    var row = new ArrayList();
                    for (var j = 0; j < ArtDG.Rows[i].Cells.Count; j++)
                    {
                        row.Add(ArtDG.Rows[i].Cells[j].InnerHtml);
                    }
                    al.Add(row);
                }
                var sort = new ArrayList();
                for (var j = 0; j < ArtDG.Rows[0].Cells.Count; j++)
                {
                    sort.Add(ArtDG.Rows[0].Cells[j].Attributes["tabIndex"]);
                }
                al.Add(sort);
                if (KeepInViewState)
                    ViewState[this.ClientID] = al;
                else
                    Session[this.ClientID] = al;
            }
            (new RegScript()).OnLoad(Page);
        }
		public void ExtractTable()
		{
			ClearTable();
			ArrayList al;
			if ( KeepInViewState )
				al = (ArrayList) ViewState[this.ClientID];
			else
				al = (ArrayList) Session[this.ClientID];
			if (al == null)
				return;
			var type = new int[((ArrayList) al[0]).Count];
			for (var j=0; j<type.Length; j++)
			{
				var HCell = new HtmlTableCell();
				HCell.InnerHtml = ((ArrayList) al[0])[j].ToString();
				type[j] = Convert.ToInt32(((ArrayList) al[al.Count-1])[j]);
				HCell.Attributes["tabIndex"] = type[j].ToString();
				if (type[j] < -4)
                    HCell.Style.Add("display", "none");
				ArtDG.Rows[0].Cells.Add(HCell);
			}
			for (var i=1; i < al.Count-1; i++)
			{
				var HRow = new HtmlTableRow();
				for (var j=0; j<( (ArrayList) al[i]).Count; j++)
				{
					var HCell = new HtmlTableCell();
					HCell.InnerHtml = ( (ArrayList) al[i])[j].ToString();
					if (type[j] < -4)
                        HCell.Style.Add("display", "none");
					HRow.Cells.Add(HCell);
				}
				ArtDG.Rows.Add(HRow);
			}
		}

		protected void Grid_Load()
		{
			(new RegScript()).Register(Page, "DataGrid", this.TemplateSourceDirectory);
			if(IsPostBack && ( KeepInViewState || KeepInSession ))
				 ExtractTable();
		}
        override protected void OnInit(EventArgs e)
        {
            CultureInfo CI = null;
            for (var i = 0; i < Request.UserLanguages.Length; i++)
            {
                var s = Request.UserLanguages[i];
                if (s.Length > 5)
                    s = s.Substring(0, 5);
                CI = CultureInfo.CreateSpecificCulture(s);
                if (CI != null)
                    break;
            }
            if (CI == null)
                CI = new CultureInfo("en-us");
            System.Threading.Thread.CurrentThread.CurrentCulture = CI;
            System.Threading.Thread.CurrentThread.CurrentUICulture = CI;
            base.OnInit(e);
        }
    }
}
