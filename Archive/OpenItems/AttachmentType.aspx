<%@ Page Language="C#" MasterPageFile="~/MasterPages/ULO/ULOMenu.master" AutoEventWireup="true" Inherits="GSA.OpenItems.Web.AttachmentType" Codebehind="AttachmentType.aspx.cs" %>

<%@ Register Src="Controls/Menu.ascx" TagName="Menu" TagPrefix="uc1" %>

<asp:Content ID="Content1" ContentPlaceHolderID="title" runat="Server">
    Manage Attachment Types
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="head" runat="Server">
</asp:Content>
<asp:Content ID="Content5" ContentPlaceHolderID="contentTitle" runat="Server">
    Manage Attachment Types
</asp:Content>
<asp:Content ID="Content6" ContentPlaceHolderID="content" runat="Server">
    <div class="section">
        <table width="60%" border="0" class="regBldBlueText">
            <tr>
                <td>
                    <asp:Label ID="lblError" runat="server" CssClass="error" Text=""></asp:Label>
                </td>
                <td align="right">
                    <a id="lnkHelp" target="_blank" href="docs/How_To_Manage_ULO_Attachment_Types.pdf" style="color: Blue; border-style: none" runat="server">Help</a>
                </td>
            </tr>
            <tr>
                <td colspan="2">
                    <asp:RadioButtonList ID="rbEditOrNew" AutoPostBack="true" runat="server" RepeatDirection="Horizontal" Width="100%" OnSelectedIndexChanged="rbEditOrNew_SelectedIndexChanged">
                        <asp:ListItem Value="0">Create New Attachment Type</asp:ListItem>
                        <asp:ListItem Value="1">Edit Existing Attachment Type</asp:ListItem>
                    </asp:RadioButtonList></td>
            </tr>
            <tr>
                <td colspan="2">
                    <hr class="thinHR" />
                </td>

            </tr>

            <tr id="tr_ddlAttTypes" runat="server">
                <td style="width: 30%">Select Attachment Type:
                </td>
                <td>
                    <asp:DropDownList ID="ddlAttTypes" runat="server" AutoPostBack="True" OnSelectedIndexChanged="ddlAttTypes_SelectedIndexChanged"
                        Width="100%">
                    </asp:DropDownList></td>
            </tr>
            <tr style="height: 30px">
                <td style="width: 30%"></td>
                <td></td>
            </tr>

            <tr id="tr_txtAttTypes" runat="server">
                <td>Attachment Type:</td>
                <td>
                    <asp:TextBox ID="txtAttType" runat="server" CssClass="enabled_textbox" MaxLength="100"
                        Width="98%"></asp:TextBox>
                </td>
            </tr>

            <tr>
                <td colspan="2" id="tr_line2" runat="server">&nbsp;
                        <hr class="thinHR" />
                </td>
            </tr>
            <tr>
                <td style="width: 30%">
                    <asp:Button ID="btnSave" runat="server" CssClass="button" OnClick="btnSave_Click"
                        Text="Save" Width="70px" /></td>
                <td>
                    <input id="hDocTypeCode" runat="server" type="hidden" /></td>
            </tr>

        </table>
    </div>
</asp:Content>
