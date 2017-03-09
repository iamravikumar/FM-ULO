<%@ Control Language="C#" AutoEventWireup="true" Inherits="GSA.OpenItems.Web.Controls_FundsCriteria" Codebehind="FundsCriteria.ascx.cs" %>

<script type="text/javascript" >

    
    function funds_data_to_excel(type)
    {            
        var popup = window.open("FundReportExcel.aspx?type="+type, "FundsDataToExcel", 
                "width=600,height=600,menubar=no,status=no,resizable=yes,scrollbars=yes,toolbar=yes,top=200,left=200");                
        popup.focus();
         
        return false;
    }       

</script>

<table width="100%" border="0" cellspacing="0" cellpadding="0">
<tr><td>
    <table id="tblControls" runat="server" width="100%" border="0" cellspacing="12" cellpadding="0">
        <tr id="trViewSelection" runat="server">
            <td>
                <asp:Label id="lblView" runat="server" CssClass="regBldText" Text="Select View: " />
            </td>
            <td class="regText">
                <asp:DropDownList ID="ddlView" runat="server" Width="140px" />
            </td>
            <td colspan="6"></td>
        </tr>    
        <tr>     
            <td>
                <label class="regBldText">Business Line: </label>                
            </td>
            <td class="regText">
                <asp:DropDownList ID="ddlBusinessLine" runat="server" Width="140px" AutoPostBack="true" />
            </td>   
            <td>
                <label class="regBldText">Organization: </label>                
            </td>
            <td class="regText">
                <asp:DropDownList ID="ddlOrganization" runat="server" Width="140px" />
            </td>
            <td>
                <label class="regBldText">Fiscal Year: </label>        
            </td>
            <td class="regText">
                <asp:DropDownList ID="ddlFiscalYear" runat="server" Width="140px" AutoPostBack="true" />            
            </td>        
            <td>
                <asp:Label id="lblBookMonth" runat="server" CssClass="regBldText" Text="Book Month: " />      
            </td>
            <td class="regText">
                <asp:DropDownList ID="ddlBookMonth" runat="server" Width="140px" />
            </td>                                
        </tr>
        <tr id="trSearchCriteria" runat="server" valign="top" >
            <td>
                <label class="regBldText">DocNumber: </label>        
            </td>
            <td class="regText">
                <asp:TextBox ID="txtDocNumber" runat="server"  Width="140px" />
            </td> 
            <td>
                <label class="regBldText">Summary Function: </label>        
            </td>
            <td class="regText" >            
                <asp:ListBox ID="lstSumFunc" runat="server" Width="140px" SelectionMode="Multiple" Height="100" />
            </td>        
            <td>
                <label class="regBldText">OC Code: </label>
            </td>
            <td class="regText" >            
                <asp:ListBox ID="lstOCCode" runat="server" Width="140px" SelectionMode="Multiple" Height="100" />
            </td>    
            <td colspan="2"></td>
       </tr>       
    </table> 
</td></tr>
<tr><td>
    <table width="100%" border="0" cellspacing="0" cellpadding="0">
        <tr>
            <td colspan="2" >
                <hr />
                <asp:Label ID="lblCriteriaMsg" runat ="server" CssClass="regBldBlueText" />
            </td>
        </tr>       
       <tr>
            <td align="left" style="padding-left:8px;">
                <asp:Button ID="btnExpand" runat="server" Text="Expand by Month" ToolTip="Get Expanded View by Month" CssClass="button2" Enabled="false" Visible="false" />&nbsp;&nbsp;
                <asp:Button ID="btnExcel" runat="server" Text="Export to Excel" ToolTip="Export to Excel" CssClass="button2" Enabled="false" />&nbsp;&nbsp;
                <asp:Button ID="btnEmail" runat="server" Text="Get by Email" ToolTip="Get by Email" CssClass="button2" Enabled="false" Visible="false" />
            </td>
            <td align="right" style="padding-right:8px;">
                <asp:Button ID="btnBack" runat="server" Text="Back" ToolTip="Back" CssClass="button" Visible="false" />&nbsp;&nbsp;
                <asp:Button ID="btnEdit" runat="server" Text="Edit" ToolTip="Edit Criteria" CssClass="button" Enabled="false" Visible="false" />&nbsp;&nbsp;
                <asp:Button ID="btnClear" runat="server" Text="Clear" ToolTip="Clear Selected Criteria" CssClass="button" Visible="false" />&nbsp;&nbsp;
                <asp:Button ID="btnSubmit" runat="server" Text="Search" ToolTip="Search" CssClass="button" />
            </td>
        </tr>   
    </table>
</td></tr>
</table>
