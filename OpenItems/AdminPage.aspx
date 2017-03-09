<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/ULO/ULOMenu.master" AutoEventWireup="true" Inherits="GSA.OpenItems.Web.AdminPage" Codebehind="AdminPage.aspx.cs" %>
<%@ Register TagPrefix="uc" TagName="Menu" Src="~/Controls/Menu.ascx" %>


<asp:Content ID="Content1" ContentPlaceHolderID="title" runat="Server">
    Admin page
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="head" runat="Server">
</asp:Content>
<asp:Content ID="Content5" ContentPlaceHolderID="contentTitle" runat="Server">
    Admin Page
</asp:Content>
<asp:Content ID="Content6" ContentPlaceHolderID="content" runat="Server">
    <div class="section">
            <table width="100%" border="0"  cellpadding="3" cellspacing="3">
                <tr valign="top" height="40">
                    <td>
                        <asp:HyperLink ID="lnkNewUrer" CssClass="blue_link_12" runat="server" ForeColor="#085067" NavigateUrl="~/Users.aspx?mode=admin">Manage Users</asp:HyperLink>
                    </td>
                </tr>
               
                <tr  valign="top"  height="40">
                    <td>
                        <asp:HyperLink ID="lnkArchive"   CssClass="blue_link_12" runat="server" ForeColor="#085067" NavigateUrl="~/ArchiveLoad.aspx">Archive/Unarchive Review</asp:HyperLink>
                    </td>
                </tr>
                <tr valign="top"  height="40">
                    <td>
                        <asp:HyperLink ID="lnkSendReminder" runat="server" CssClass="blue_link_12" ForeColor="#085067"
                            NavigateUrl="~/SendULOEmail.aspx">Send ULO Email</asp:HyperLink></td>
                </tr>
                
                <tr  valign="top"  height="40" style="display:">
                    <td>
                        <asp:HyperLink ID="lnkAttachmentType"   CssClass="blue_link_12" runat="server" ForeColor="#085067" NavigateUrl="~/AttachmentType.aspx">Manage Attachment Types </asp:HyperLink>
                    </td>
                </tr>
                
                <tr  valign="top"  height="40" style="display:">
                    <td>
                        <asp:HyperLink ID="lnkJustification"   CssClass="blue_link_12" runat="server" ForeColor="#085067" NavigateUrl="~/Justification.aspx">Manage Justifications </asp:HyperLink>
                    </td>
                </tr>
                
                 <tr  style="display:none" valign="top"  height="40">
                    <td>
                        <asp:HyperLink ID="lnkNewOrg"   CssClass="blue_link_12" runat="server" ForeColor="#085067" NavigateUrl="~/Organizations.aspx">Manage Organizations</asp:HyperLink>
                    </td>
                </tr>
                
                
                <tr>
                    <td></td>
                </tr>
                <tr>
                    <td style="height: 31px"></td>
                </tr>
                <tr>
                    <td>
                    </td>
                </tr>
            </table>
        </div>
        <div class="section_lower">
            <table width="100%" border="0" height="200px" cellpadding="3" cellspacing="3">
                <tr>
                    <td></td>
                </tr>
            </table>
        </div>     
</asp:Content>

