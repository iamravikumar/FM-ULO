<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/ULO/ULOMenu.master" AutoEventWireup="true" Inherits="GSA.OpenItems.Web.FundStatus_OrgAllowance" Codebehind="OrgAllowance.aspx.cs" %>


<asp:Content ID="Content1" ContentPlaceHolderID="title" runat="Server">
    Fund Allowance
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="head" runat="Server">
        <!--Originally had BA61 Fund Status as Application Title -->
    <link href="../css/fund_status.css" type="text/css" rel="stylesheet" />  
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="contentTitle" runat="Server">
    <asp:Label ID="lblDistributionTitle" runat="server" Text="Fund Allowance Over NCR Organizations" />
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="content" runat="Server">

    <div class="section">
        <table width="100%" id="tblCntrls" runat="server">
            <tr>
                <td class="regBldText" style="width: 80px">Fiscal Year:</td>
                <td>
                    <asp:DropDownList ID="ddlFiscalYear" runat="server" AutoPostBack="true" Width="120px" /></td>
                <td class="regBldText" style="width: 100px">Business Line:</td>
                <td>
                    <asp:DropDownList ID="ddlBusinessLine" runat="server" AutoPostBack="true" Width="150px" /></td>
                <td align="right">
                    <asp:Button ID="ReDistibute" runat="server" Visible="false" Enabled="false" CssClass="button2" Width="150px" ToolTip="Redistribute by functions for selected organizations" Text="Redistribute by functions" />
                    <span style="width: 20px">&nbsp;</span>
                    <input type="submit" value="Export to Excel" onclick="return data_to_excel();" title="Export to Excel" class="button2" />
                </td>
            </tr>
        </table>
        <hr />
        <!-- #include virtual="../include/ErrorLabel.inc" -->

        <div id="divChart">
            <asp:Label ID="lblChartMsg" runat="server" CssClass="regBldGreyText" Text="To change distribution rate please click on Month header." />
            <table id="tblChart" runat="server" width="100%" class="reportTable" onmouseover="mouse('#DDDDDD')" onmouseout="mouse('')">
                <tbody>
                </tbody>
            </table>
            <input runat="server" id="Result" style="display: none" />
        </div>

        <table width="100%" id="tblBttns" runat="server">
            <tr align="center">
                <td align="center" style="height: 40px" valign="bottom">
                    <input type="button" id="btnCancel" value="Cancel" class="button2" disabled="disabled" onclick="location.href = location.href;" />
                    <span style="width: 20px">&nbsp;</span>
                    <asp:Button ID="btnSave" runat="server" CssClass="button2" Enabled="false" Text="Save Changes" />
                </td>
            </tr>
        </table>

    </div>
</asp:Content>
