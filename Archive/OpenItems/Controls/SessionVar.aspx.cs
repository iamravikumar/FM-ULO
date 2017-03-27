using System;
namespace Controls
{
	public partial class SessionVar : System.Web.UI.Page
	{
		protected void Page_Load(object sender, System.EventArgs e)
		{
			var sName = Request.ServerVariables["HTTP_ACCEPT"];
			var sVar = "";
			if (Session[sName] != null)
				sVar = Session[sName].ToString();
			Response.Write(sVar);
			if (sName == "Error")
				Session[sName] = "";
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
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{    
		}
		#endregion
	}
}
