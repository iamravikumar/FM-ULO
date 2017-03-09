using System;
namespace Controls
{
	public partial class SessionVarReq : System.Web.UI.UserControl
	{

        public string FlashCSS
        {
            set { Mess.AddCssClass(value); }
        }
        public string ErrMess
        {
            get { return Mess.InnerHtml; }
            set { Mess.InnerHtml = value; }
        }
        public string ClientOnLoad
        {
            set { Session["_OnL"] = new string[] { value, Page.ToString() }; }
        }
        protected void Page_Load(object sender, System.EventArgs e)
        {
            CB_Art.Checked = false;
            CB_Art.Value = this.TemplateSourceDirectory + "/SessionVar.aspx";
            var o = new RegScript();
            o.Register(Page, "HttpReq", this.TemplateSourceDirectory);
            o.Register(Page, "SessionVarReq", this.TemplateSourceDirectory);
        }


		protected void MyRender(object sender, System.EventArgs e)
		{
			(new RegScript()).OnLoad(Page);
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
