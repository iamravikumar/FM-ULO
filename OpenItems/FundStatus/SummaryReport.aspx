<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/ULO/ULOMenu.master" AutoEventWireup="true" Inherits="GSA.OpenItems.Web.FundStatus_SummaryReport" Codebehind="SummaryReport.aspx.cs" %>
<%@ Register TagPrefix="uc" TagName="fundscriteria" Src="~/Controls/FundsCriteria.ascx" %>


<asp:Content ID="Content1" ContentPlaceHolderID="title" runat="Server">
    Funds Summary Projection Report
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="head" runat="Server">
        <!--Originally had BA61 Fund Status as Application Title -->
    <link href="../css/fund_status.css" type="text/css" rel="stylesheet" />
    <!--#include virtual="../include/HTTPService.js" -->
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="contentTitle" runat="Server">
    Summary Projection Report
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
                        <input id="btnRecalculate" type="button" onclick="alert2('Report is recalculating.<br/>See it after a few minutes.', 'Fund Status message'); recalc_chart();" value="Recalculate Report Now" class="button" />
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

        <div id="divData" runat="server" style="display: none;" onmouseover="onmouse(1,event,'reportAmountLinkHover')" onmouseout="onmouse(0,event,'reportAmountLink')">

            <input type="image" src="../images/Plus1.gif" id="btnShowHide1" onclick="javascript: return show_data(this, 1);" title="hide" />
            <asp:Label ID="lblShowTable_1" runat="server" CssClass="regBldGreyText" />
            <div id="divTable_1" style="display: inline;">
                <table id="tblData_1" runat="server" width="100%" />
            </div>
            <br />
            <input type="image" src="../images/Plus1.gif" id="btnShowHide7" onclick="javascript: return show_data(this, 7);" title="hide" />
            <asp:Label ID="lblShowTable_7" runat="server" CssClass="regBldGreyText" />
            <div id="divTable_7" style="display: inline;">
                <table id="tblData_7" runat="server" width="100%" />
            </div>
            <br />
            <input type="image" src="../images/Plus1.gif" id="btnShowHide2" onclick="javascript: return show_data(this, 2);" title="hide" />
            <asp:Label ID="lblShowTable_2" runat="server" CssClass="regBldGreyText" />
            <div id="divTable_2" style="display: inline;">
                <table id="tblData_2" runat="server" width="100%" />
            </div>
            <br />
            <input type="image" src="../images/Plus1.gif" id="btnShowHide8" onclick="javascript: return show_data(this, 8);" title="hide" />
            <asp:Label ID="lblShowTable_8" runat="server" CssClass="regBldGreyText" />
            <div id="divTable_8" style="display: inline;">
                <table id="tblData_8" runat="server" width="100%" />
            </div>
            <br />
            <input type="image" src="../images/Plus1.gif" id="btnShowHide3" onclick="javascript: return show_data(this, 3);" title="hide" />
            <asp:Label ID="lblShowTable_3" runat="server" CssClass="regBldGreyText" />
            <div id="divTable_3" style="display: inline;">
                <table id="tblData_3" runat="server" width="100%" />
            </div>
            <br />
            <input type="image" src="../images/Plus1.gif" id="btnShowHide4" onclick="javascript: return show_data(this, 4);" title="hide" />
            <asp:Label ID="lblShowTable_4" runat="server" CssClass="regBldGreyText" />
            <div id="divTable_4" style="display: inline;">
                <table id="tblData_4" runat="server" width="95%" />
            </div>
            <br />
            <input type="image" src="../images/Plus1.gif" id="btnShowHide5" onclick="javascript: return show_data(this, 5);" title="hide" />
            <asp:Label ID="lblShowTable_5" runat="server" CssClass="regBldGreyText" />
            <div id="divTable_5" style="display: inline;">
                <table id="tblData_5" runat="server" width="100%" />
            </div>
            <br />
            <input type="image" src="../images/Plus1.gif" id="btnShowHide6" onclick="javascript: return show_data(this, 6);" title="hide" />
            <asp:Label ID="lblShowTable_6" runat="server" CssClass="regBldGreyText" />
            <div id="divTable_6" style="display: inline;">
                <table id="tblData_6" runat="server" width="95%" />
            </div>
            <br />

        </div>

    </div>
</asp:Content>
