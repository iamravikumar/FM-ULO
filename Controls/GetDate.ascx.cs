using System;
using System.Data;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

namespace Controls
{
	public partial class GetDate : System.Web.UI.UserControl
	{
        public int tabIndex
        {
            get { return lDate.TabIndex; }
            set { lDate.TabIndex = (short)value; }
        }
        public bool ShrinkOnBlur
		{
            get { return (day.TabIndex & 4) == 0; }
			set { if (value)
					day.TabIndex &= 0x0FB;
				  else
                    day.TabIndex = (short)((ushort)day.TabIndex | 4); 
				}
		}
		public bool FreezeMode
		{
			get { return (day.TabIndex & 2) != 0; }
			set	{ if (value)
					day.TabIndex = (short)((day.TabIndex&0x0FC) + 2);
				  else
					day.TabIndex &= 0x0FD;
				}
		}
		public bool VoidEdit
		{
			get { return (day.TabIndex & 1) == 0; }
			set { if (!FreezeMode)
					day.TabIndex = (short)((day.TabIndex&0x0FC) + (value ? 0 : 1));
				}
		}
        private string DateToS(DateTime d)
        {
            return d.Year.ToString() + "/" + d.Month.ToString() + "/" + d.Day.ToString();
        }
		public DateTime MinDate
		{
			get	
			{
				var d = new DateTime(DateTime.Now.Year - 11, 1, 1);
                    var ss = day.ToolTip.Split(new char[] { ';' })[0].Split(new char[] { '/' });
                    d = new DateTime(Convert.ToInt32(ss[0]), Convert.ToInt32(ss[1]), Convert.ToInt32( ss[2] ));
				return d;
			}
            set { day.ToolTip = DateToS(value) + ";" + DateToS(MaxDate); }
		}
		public DateTime MaxDate
		{
			get	
			{
				var d = new DateTime(DateTime.Now.Year, 12, 31);
                    var ss = day.ToolTip.Split(new char[] { ';' })[1].Split(new char[] { '/' });
                    d = new DateTime(Convert.ToInt32(ss[0]), Convert.ToInt32(ss[1]), Convert.ToInt32(ss[2]));
				return d;
			}
			set { day.ToolTip = DateToS(MinDate) + ";" + DateToS(value); }
		}

        public DateTime Date
        {
            get
            {
                var d = DateTime.Now; //by SMM
                //DateTime d = DateTime.MinValue;
                d = DateTime.Parse(lDate.Text); 
                return d;
            }
            set
            {
                lDate.Text = value.ToShortDateString().Replace(".", "/").Replace("-", "/");
            }
        }
		public int CellSpacing
		{
			get { return ArtDT.CellSpacing; }
			set { ArtDT.CellSpacing = value; }
		}
		public string DateCSSClass
		{
			set { lDate.CssClass = value; }
		}
		public Unit Width
		{
			set	{ ArtCC.Style.Add("width", value.ToString()); }
		}
		public Unit YearHeight
		{
			set	{ ddgYear.Height = value; }
		}
		public bool isEmpty
		{
			get	{	return lDate.Text == "" ? true : false; }
		}
		public bool isValid
		{
            //TODO: do with TryParse instead.
			get	{	try		{	var d = DateTime.Parse(lDate.Text); }
					catch	{	return false; }
					return true;
				}
		}
		public bool VoidEmpty				//do not show clear button
		{
			get {	return !btnClear.Visible; }
			set {	btnClear.Visible = !value; }
		}
		public string OnChange						//Client event script
		{
			set	{	btnOnCnange.Attributes.Add("onclick", value); }
		}
		public EventHandler OnChangeHandler			//Server event
		{
			get {	return null; }
			set {	btnOnCnange.ServerClick += value; }
		}
		public string ClientOnLoad
		{
            set { Session["_OnL"] = new string[] { value, Page.ToString() }; }
		}
        public void Clear()
		{
			lDate.Text = "";
		}
		public void MyRender()
		{
            clndr.Visible = btnOnCnange.Visible = true;
            day.Attributes["alt"] = (new DateTime(2009, 12, 31)).ToShortDateString().Split(new char[] { '/','-','.',' ' })[0];

            if (isEmpty && VoidEmpty)
				Date = DateTime.Now;

            lDate.Style.Clear();
            lDate.Width = Unit.Parse("100%");
            lDate.Attributes.Add("onmouseout", "KDate(event,this)");
            lDate.Attributes.Add("onkeyup", "KDate(event,this)");
			var tMonth = new DataTable();
			var colMonth = new DataColumn("Month", typeof(string));
			tMonth.Columns.Add(colMonth);
			var tYear = new DataTable();
			var colYear = new DataColumn("Year", typeof(string));
			tYear.Columns.Add(colYear);
			DataRow rMonth, rYear;

			for(var i=0; i<12; i++)
			{
				rMonth = tMonth.NewRow();
                rMonth["Month"] = System.Threading.Thread.CurrentThread.CurrentCulture.DateTimeFormat.MonthNames[i];
				tMonth.Rows.Add(rMonth);
			}
			ddgMonth.Table = tMonth;
			ddgMonth.SelectedIndex = Date.Month - 1;

			if (MaxDate < Date)
				MaxDate = Date;
			if (MinDate > Date)
				MinDate = Date;

			var iEndYear = MaxDate.Year;
			var iStartYear = MinDate.Year;
			for(var i=iEndYear; i>=iStartYear; i--)
			{
				rYear = tYear.NewRow();
				rYear["Year"] = i.ToString();
				tYear.Rows.Add(rYear);
			}
			ddgYear.Table = tYear;
			ddgYear.SelectedIndex = iEndYear - Date.Year;
			day.Text = Date.Day.ToString();
            if (!FreezeMode)
            {
                cAdd(tdBttn);
                if (VoidEdit)
                {
                    cAdd(tdDate);
                    lDate.Style.Add("cursor", "pointer");
                }
            }
        }
        private void cAdd(HtmlControl t)
        {
            t.Style.Add("cursor", "pointer");
            t.Attributes["title"] = "Click to change date";
        }
 
		protected void MyRender(object sender, System.EventArgs e)
		{
			MyRender();
		}

		protected void Page_Load(object sender, System.EventArgs e)
		{
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
