<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Popups/Popup.master" AutoEventWireup="true" CodeFile="Reroute.aspx.cs" Inherits="GSA.OpenItems.Web.Reroute" %>

<%@ Register TagPrefix="uc" TagName="UsersOrganizations" Src="~/Controls/OrgUsers.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="title" runat="Server">
    Open Items - Reroute Item
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="head" runat="Server">
    <!--#include virtual="../include/HTTPService.js" -->
    <!--#include virtual="include/Reroute.js" -->
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="contentTitle" runat="Server">
    Reroute Item
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="content" runat="Server">
    <div class="section">

        <input type="hidden" id="txtItemID" runat="server" />
        <input type="hidden" id="txtRequestID" runat="server" />
        <input type="hidden" id="txtPrevOrganization" runat="server" />
        <input type="hidden" id="txtPrevReviewer" runat="server" />
        <input type="hidden" id="txtGroupAssign" runat="server" />

        <asp:Label ID="lblMessage" runat="server" CssClass="errorsum" Visible="false" />
        <asp:Label ID="lblGroupAssign" runat="server" CssClass="regBldText" Visible="false"
            Text="Group Reassignment for all selected Open Items" />
        <p />

        <table width="100%" cellspacing="4" id="tblLabels" runat="server">
            <tr>
                <td style="width: 150px">
                    <label class="regBldText">Document Number: </label>
                </td>
                <td>
                    <asp:Label ID="lblDocNumber" runat="server" CssClass="lrgBldText" /></td>
            </tr>
            <tr>
                <td style="width: 150px">
                    <label class="regBldText">Lines: </label>
                </td>
                <td>
                    <asp:Label ID="lblLines" runat="server" CssClass="lrgBldText" /></td>
            </tr>
            <tr>
                <td>
                    <label class="regBldText">Current Organization: </label>
                </td>
                <td>
                    <asp:Label ID="lblPrevOrganization" runat="server" CssClass="lrgBldText" /></td>
            </tr>
            <tr>
                <td>
                    <label class="regBldText">Current OrgCode: </label>
                </td>
                <td>
                    <asp:Label ID="lblPrevOrgCode" runat="server" CssClass="lrgBldText" /></td>
            </tr>
            <tr>
                <td>
                    <label class="regBldText">Current Reviewer: </label>
                </td>
                <td>
                    <asp:Label ID="lblPrevReviewerName" runat="server" CssClass="lrgBldText" /></td>
            </tr>
        </table>

        <uc:UsersOrganizations ID="ctrlOrgUsers" runat="server" />

        <table width="100%" cellspacing="4">
            <tr>
                <td style="width: 150px">
                    <label class="regBldText">Comment: </label>
                </td>
                <td>
                    <textarea id="txtComment" rows="3" cols="43" runat="server"></textarea>
                </td>
            </tr>
            <tr>
                <td colspan="2" align="right">
                    <input type="button" id="btnCancel" value="Cancel" onclick="return on_cancel();" class="button" title="Cancel" />&nbsp;&nbsp;
                <input type="button" id="btnReroute" value="Reroute" onclick="return on_reroute();" class="button" title="Reroute Open Item to Selected Organization" />
                </td>
            </tr>
        </table>
    </div>
</asp:Content>
