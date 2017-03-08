<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Popups/Popup.master" AutoEventWireup="true" CodeFile="ReassignRequest.aspx.cs" Inherits="GSA.OpenItems.Web.ReassignRequest" %>

<%@ Register TagPrefix="uc" TagName="UsersOrganizations" Src="~/Controls/OrgUsers.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="title" runat="Server">
    Open Items - Reassign Request
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="head" runat="Server">
    <!--#include virtual="../include/HTTPService.js" -->
    <!--#include virtual="include/ReassignRequest.js" -->
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="contentTitle" runat="Server">
    Reassign Request
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="content" runat="Server">
    <div class="section">

        <input type="hidden" id="txtItemID" runat="server" />

        <table width="100%" cellspacing="4">
            <tr>
                <td style="width: 150px">
                    <label class="regBldText">Document Number: </label>
                </td>
                <td>
                    <asp:Label ID="lblDocNumber" runat="server" CssClass="lrgBldText" /></td>
            </tr>
            <tr>
                <td>
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
                    <asp:Label ID="lblOrgCode" runat="server" CssClass="lrgBldText" /></td>
            </tr>
            <tr>
                <td colspan="2">
                    <uc:UsersOrganizations ID="ctrlOrgUsers" runat="server" />
                </td>
            </tr>
            <tr>
                <td>
                    <label class="regBldText">Comment: </label>
                </td>
                <td>
                    <textarea id="txtComment" rows="3" cols="43"></textarea>
                </td>
            </tr>
            <tr>
                <td colspan="2" align="right">
                    <input type="button" id="btnCancel" value="Cancel" onclick="return on_cancel();" class="button" title="Cancel" />&nbsp;&nbsp;
                <input type="button" id="btnAssign" runat="server" value="Send to OrgAdmin" onclick="return on_assign_request();" class="button" title="Send Request to Organization Administrator" />
                </td>
            </tr>
            <tr>
                <td colspan="2">
                    <asp:Label ID="lblMessage" runat="server" Visible="false" Text="" CssClass="errorsum" Width="400" />
                </td>
            </tr>
        </table>
    </div>

    <br />
</asp:Content>
