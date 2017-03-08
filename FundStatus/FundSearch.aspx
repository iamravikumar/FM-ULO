<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/ULO/ULOMenu.master" AutoEventWireup="true" CodeFile="FundSearch.aspx.cs" Inherits="GSA.OpenItems.Web.FundSearch" %>
<%@ Register Src="../Controls/multigrid.ascx" TagName="multigrid" TagPrefix="uc2" %>
<%@ Register Src="../Controls/SearchCriteria.ascx" TagName="SearchCriteria" TagPrefix="uc1" %>


<asp:Content ID="Content1" ContentPlaceHolderID="title" Runat="Server">
    <!--Originally had BA61 Fund Status as Application Title -->
    Funds Search
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="head" Runat="Server">
    <link href="../css/fund_status.css" type="text/css" rel="stylesheet" />
    <!--#include virtual="../include/HTTPService.js" -->
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="contentTitle" Runat="Server">
    Funds Search
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="content" Runat="Server">
     <div class="section">
                            <uc1:SearchCriteria ID="SearchCriteria" runat="server" />
                            <table width="90%" cellspacing="0">
                                <tr><td colspan="2"><!-- #include virtual="../include/ErrorLabel.inc" --></td></tr>
                                <tr><td ><asp:Label ID="lblMessages" runat ="server" CssClass="regBldGreyText"/></td>
                                    <td align="right" id="btns" runat="server">
                                        <input type="button" id="btnBack" onclick="location.href=unescape('FundsReport.aspx?back=y');" runat="server" style="width:50px" value="Back" title="Back" class="button" Visible="false" />
                                        <span style="width:8px"></span>
                                        <asp:Button ID="btnEMail" Width="90px" runat="server" ToolTip="Send by E-Mail" CssClass="button" Text="Send by E-Mail"  OnClientClick="return email_request();" />
                                        <span style="width:8px"></span>
                                        <input type="button" onclick="return data_to_excel();" style="width:90px" title="Export to Excel" class="button" value="Export to Excel" />
                                        <span style="width:8px"></span>
                                        <asp:Button ID="btnNewSearch" Width="90px" runat="server" ToolTip="New or Update Search" CssClass="button" Text="Modify Search" OnClick="btnNewSearch_Click" />
                                    </td>
                                </tr>
                                <tr><td colspan="2">
                                        <uc2:multigrid ID="mgRezult" ItemCSSClass="NotSelGrid" Height="600px" runat="server" /></td>
                                </tr>
                            </table>
                        </div>
</asp:Content>