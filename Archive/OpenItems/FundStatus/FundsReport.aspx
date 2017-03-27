<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/ULO/ULOMenu.master" AutoEventWireup="true" Inherits="GSA.OpenItems.Web.FundsReport" Codebehind="FundsReport.aspx.cs" %>
<%@ Register TagPrefix="uc" TagName="fundscriteria" Src="~/Controls/FundsCriteria.ascx" %>


<asp:Content ID="Content1" ContentPlaceHolderID="title" runat="Server">
    
    Funds Status Report
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="head" runat="Server">
    <link href="../css/fund_status.css" type="text/css" rel="stylesheet" />
        <!--Originally had BA61 Fund Status as Application Title -->
    <!--#include virtual="../include/HTTPService.js" -->
    <script type="text/javascript">
        //onunload="javascript:close_child();"
        //abive was in body tag of page. need to handle
    </script>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="contentTitle" runat="Server">
    Funds Status Report
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="content" runat="Server">
    <div class="section">

        <uc:fundscriteria id="ctrlCriteria" runat="server" />

        <hr />

        <div id="divReportState" runat="server" visible="false">
            <table width="100%">
                <tr>
                    <td align="left">
                        <asp:Label ID="lblState" CssClass="regBldBlueText" runat="server" Text="" />
                    </td>
                    <td valign="middle" align="right">
                        <input id="btnRecalculate" type="button" value="Recalculate Report Now" onclick="recalc_chart();" class="button" />
                    </td>
                </tr>
            </table>
            <hr />
        </div>

        <table width="100%">
            <tr>
                <td>
                    <!-- #include virtual="../include/ErrorLabel.inc" -->
                </td>
            </tr>
            <tr>
                <td>
                    <asp:Label ID="lblMessages" runat="server" CssClass="regBldGreyText" /></td>
            </tr>
        </table>
        <br />

        <div id="divTable">
            <table id="tblData" runat="server" width="100%" onmouseover="onmouse(1,event,'reportAmountLinkHover')" onmouseout="onmouse(0,event,'reportAmountLink')" />
        </div>

    </div>
</asp:Content>
