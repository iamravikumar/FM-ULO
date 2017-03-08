<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SearchCriteria.ascx.cs" Inherits="GSA.OpenItems.Web.Controls_SearchCriteria" %>
<%@ Register Src="multigrid.ascx" TagName="multigrid" TagPrefix="uc2" %>
<%@ Register Src="dropdowngrid.ascx" TagName="dropdowngrid" TagPrefix="uc1" %>
<script language="javascript">
function ClearC()
{var p=document.getElementsByTagName("INPUT");var i=p.length-1
 do{	if(p[i].id.indexOf('_dgIndex')>0)
		{	dg=_nxt(p[i])
			if(dg.parentNode.id=="")
				SelectRow(dg.id.replace("_ArtDG",""),0)
			else
			{	p[i].value=""
				SetIC(dg,dg.rows.length,p[i].className)
			}
		}
		if(p[i].id.indexOf('txtDocNumber')>0)
			p[i].value=""
 }while(i--)
 p=_e('lblMessages')
 if(p!=null) p.innerHTML=""
}
function setTxt()
{var p=document.getElementsByTagName("INPUT");var i=p.length-1
 do{	if(p[i].id.indexOf('txtDocNumber')>0)
		{	p[i].onfocus=function(){this.className="txtEditFocus"}
			p[i].onblur=function(){this.className="txtEdit"}
		}
		if(p[i].id.indexOf('ddgBLine_btnOnCnange')>0)
		{	var s=p[i].id.substr(0,p[i].id.indexOf("_btnOnCnange"))
			BL=GetSelectedRow(s)[0].innerHTML
			s=s.replace("ddgBLine","ddlOrganization")
			dgFilter2(s,isRowVisible)
		}
		var j=p[i].id.indexOf('_dgIndex')
		if(j>0&&p[i].parentNode.id!="")
		{	var dg=_nxt(p[i])
			j=_findIdx(dg)
			if(j>1)
				ShowItem(dg.rows[j])
		}
 }while(i--)
}
function checkOrg()
{var p=document.getElementsByTagName("INPUT");var i=p.length-1
 do{	if(p[i].id.indexOf('ddlOrganization_dgIndex')>0)
			break
 }while(i--)
 if(p[i].value!="")
	return true
 p=_e('lblMessages')
 if(p!=null) p.innerHTML="Select one Organization at least!"
 return false
}
var BL="All"
var isRowVisible=function(cells)
{	return BL=="All"||cells[1].innerHTML==BL
}
function FilterOrg(t)
{var s=t.id.substr(0,t.id.indexOf("_btnOnCnange"))
 BL=GetSelectedRow(s)[0].innerHTML
 s=s.replace("ddgBLine","ddlOrganization")

 var p=_e(s+"_dgIndex")
 dg=_nxt(p)
 p.value=""
 SetIC(dg,dg.rows.length,p.className)
 
 dgFilter2(s,isRowVisible)

 if(BL!="All")
	for(var i=1;i<dg.rows.length;i++)
	{	if(dg.rows[i].cells[1].innerHTML==BL)
			dgmClick(dg.rows[i])
	}
}
</script>
<table id="tblControls" runat="server" width="90%" border="0" cellspacing="4" cellpadding="0">
    <tr valign="middle">
        <td ><div class="regBldText" runat="server" id="aView">Select View:
            <uc1:dropdowngrid ID="ddlView" ShowHeader="false" ShowExpandButton="true" runat="server" Width="180px" /></div></td>
        <td class="regBldText">Business Line:
            <uc1:dropdowngrid ID="ddgBLine" OnChange="FilterOrg(this)" ShowHeader="false" ShowExpandButton="true" runat="server" Width="165px" /></td>
        <td rowspan="4" valign="top"><div class="regBldText" runat="server" id="aGroup">Function Report Group:
            <uc2:multigrid ID="lstGroupCD" Height="168px" Width="190px" MultiChoice="false" ShowHeader="false" runat="server" /></div></td>
        <td rowspan="4" valign="top"><div class="regBldText" runat="server" id="aOCC">OC Code:
            <uc2:multigrid ID="lstOCCode" Height="168px" Width="270px" MultiChoice="true" ShowHeader="false" runat="server" /></div></td>
        <td rowspan="5" valign="bottom" align="left">
            <input type="button" style="width:60px" title="Clear Criteria" class="button" value="Clear" onclick="ClearC()" /><br />
            <span style="width:60px">&nbsp;</span><br />
            <asp:Button ID="Search" runat="server" ToolTip="Search" CssClass="button" Text="Search" /></td>
    </tr>
    <tr valign="middle">
        <td><a class="regBldText" runat="server" id="aDoc">DocNumber:<br />
            <asp:TextBox ID="txtDocNumber" CssClass="txtEdit" runat="server" Width="180px" /></a></td>
        <td class="regBldText" rowspan="4" valign="top">Organization:
            <uc2:multigrid ID="ddlOrganization" MultiChoice="true" Height="360px" Width="165px" ShowHeader="false" runat="server" /></td>
    </tr>
    <tr valign="top">
        <td class="regBldText">Budget Activity:
            <uc1:dropdowngrid ID="ddgBA" ShowHeader="false" ShowExpandButton="true" runat="server" Width="180px" /></td>
    </tr>
    <tr valign="top">
        <td class="regBldText">Fiscal Year:
            <uc1:dropdowngrid ID="ddlFiscalYear" ShowHeader="false" ShowExpandButton="true" runat="server" Width="180px" /></td>
    </tr>
    <tr valign="top">
        <td class="regBldText">Book Month:
            <uc2:multigrid ID="ddlBookMonth" Height="220px" MultiChoice="true" ShowHeader="false"  runat="server" Width="180px" /></td>
        <td class="regBldText"><div class="regBldText" runat="server" id="aFunc">Summary Function:
            <uc2:multigrid ID="lstSumFunc" Height="220px" Width="190px" MultiChoice="true" ShowHeader="false" runat="server" /></div></td>
        <td class="regBldText"><div class="regBldText" runat="server" id="aCE">Cost Element:
            <uc2:multigrid ID="mgCostElem" Height="220px" Width="270px" MultiChoice="true" ShowHeader="false" runat="server" /></div></td>
    </tr>
    <tr><td colspan="5"><asp:Label ID="lblCriteriaMsg" runat ="server" CssClass="regBldBlueText" /></td>
    </tr>
</table>
