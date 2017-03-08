<%@ Control Language="C#" AutoEventWireup="true" CodeFile="OrgUsers.ascx.cs" Inherits="GSA.OpenItems.Web.Controls_OrgUsers" %>
<table width="100%" cellspacing="4">
    <tr>
        <td style="width: 150px">
            <asp:Label ID="lblOrganization" runat="server" CssClass="regBldText" Text="Suggested Organization:" />
        </td>
        <td><asp:DropDownList ID="ddlOrganizations" runat="server" Width="200px" AutoPostBack="true" /></td>
    </tr>
    <tr>
        <td >
            <asp:Label ID="lblUsers" runat="server" Text= "Suggested Reviewer:" CssClass = "regBldText" />
        </td>
        <td><asp:DropDownList ID="ddlUsers" runat="server" Width="200px" AutoPostBack="true" /></td>
    </tr>
</table> 