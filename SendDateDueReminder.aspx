<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/ULO/ULOMenu.master" AutoEventWireup="true" CodeFile="SendDateDueReminder.aspx.cs" Inherits="GSA.OpenItems.Web.SendDateDueReminder" %>

<asp:Content ID="Content1" ContentPlaceHolderID="title" runat="Server">
    Admin page
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="head" runat="Server">
    <script type="text/jscript">
        function RemoveMessage() {
            //alert('!');
            document.getElementById("lblError").innerHTML = "";
        }

    </script>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="contentTitle" runat="Server">
    Send ULO Emails
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="content" runat="Server">
    <table width="100%" border="0">
        <tr>
            <td width="100%">
                <asp:Label ID="lblError" CssClass="error" runat="server" Text=""></asp:Label></td>
        </tr>
    </table>
    <div class="section1">
        <table width="100%" style="height: 100%" border="0" class="regBldBlueText" cellpadding="0" cellspacing="0">
            <tr>
                <td colspan="3"></td>
            </tr>


            <tr>
                <td colspan="3">
                    <hr class="thickHR" />
                </td>
            </tr>


            <tr class="evenTD" height="40px">
                <td colspan="3" style="width: 100%">Send Date Due Reminder</td>
            </tr>

            <tr class="oddTD" height="30px">
                <td style="width: 4px">Review:</td>
                <td>
                    <asp:DropDownList ID="ddlLoads" onChange="RemoveMessage();" CssClass="regBldBlueText" Width="100%" runat="server" OnSelectedIndexChanged="ddlLoads_SelectedIndexChanged" AutoPostBack="True">
                    </asp:DropDownList>
                </td>
                <td>&nbsp;
                </td>
            </tr>


            <tr class="evenTD" height="50px">
                <td colspan="3" style="width: 100%">
                    <asp:Button ID="btnReset" Width="70px" CssClass="button" runat="server" Text="Reset" OnClick="btnReset_Click" />&nbsp;
                        &nbsp;<asp:Button ID="btnSend" Width="115px" ToolTip="Send email reminder" CssClass="button" runat="server" Text="Send Reminder" OnClick="btnSend_Click" /></td>
            </tr>
            <tr>
                <td colspan="3">
                    <hr class="thickHR" />
                </td>
            </tr>

            <tr class="evenTD" height="40px">
                <td colspan="3" style="width: 100%">Send Custom Email To All Org Admins</td>
            </tr>

            <tr class="evenTD" height="50px">
                <td style="width: 40px; height: 50px;">Subject:</td>
                <td colspan="2" style="height: 50px">
                    <textarea id="txtSubject" runat="server" style="width: 96%; height: 50px"></textarea></td>


            </tr>
            <tr class="evenTD" height="250px">
                <td style="width: 40px">Body:</td>
                <td colspan="2">
                    <textarea id="txtBody" runat="server" style="width: 97%; height: 229px"></textarea></td>


            </tr>


        </table>
    </div>

    <div id="lower_section" runat="server" class="section_lower">
        <table width="100%" border="0" class="regBldBlueText">

            <tr>
                <td colspan="3">&nbsp;
                            <asp:Button ID="btnSendEmailToOrgAdnins" Width="193px" ToolTip="Send email to OrgA Adnins" CssClass="button" runat="server" Text="Send email to OrgA Adnins" OnClick="btnSendEmailToOrgAdnins_Click" />
                    <hr class="thickHR" />
                </td>
            </tr>

            <tr>
                <td width="20%">&nbsp;
                </td>
                <td width="50%">&nbsp;&nbsp;
                            <input type="hidden" id="hUserID" runat="server" />
                    <input type="hidden" id="txtLoadDate" runat="server" /></td>
                <td>
                    <input type="hidden" id="txtDueDate" runat="server" /></td>
            </tr>
        </table>
    </div>
</asp:Content>
