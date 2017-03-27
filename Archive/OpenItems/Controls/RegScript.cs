namespace Controls
{
    public class RegScript
	{
        public RegScript() { }

        public void Register(System.Web.UI.Page p, string script, string path)
        {
            if (!p.ClientScript.IsClientScriptBlockRegistered(p.GetType(), script))
                p.ClientScript.RegisterClientScriptBlock(p.GetType(), script,
                    "<script language='javascript' src='" + path + "/" + script + ".js'></script>");
        }

        private string GetScript(string sVar, System.Web.UI.Page p)
        {
            var ss = (string[])p.Session[sVar];
            return ss != null && ss[1].ToString() == p.ToString() ? ss[0].ToString() + ";" : "";
        }

        public void OnLoad(System.Web.UI.Page p)
		{
            if (!p.ClientScript.IsStartupScriptRegistered(p.GetType(), "ArtOnLoad"))
            {
                var s = "{";
                if (p.ClientScript.IsClientScriptBlockRegistered(p.GetType(), "SessionVarReq"))
					s = "if(!art_session()){";

                if (p.ClientScript.IsClientScriptBlockRegistered(p.GetType(), "DataGrid"))
                    s += "art_onload();";

                s += GetScript("_OnL", p) + "var q=document.getElementById('pWait');if(q!=null) q.style.display='none'}";

                p.ClientScript.RegisterStartupScript(p.GetType(), "ArtOnLoad",
                    "<script FOR=window EVENT=onload language='javascript'>" + s + "</script>");
			}
            p.Session.Remove("_OnL");
        }
	}
}
