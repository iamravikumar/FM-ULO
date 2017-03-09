<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/ULO/ULOMenu.master" AutoEventWireup="true" Inherits="GSA.OpenItems.Web.SendULOEmail" Codebehind="SendULOEmail.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="title" runat="Server">
    ULO Email
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="head" runat="Server">
    <script type="text/jscript">

        function ExecuteConfirm() {
            //debugger

            var returnValue = confirm('Are you sure you want to send email ?');


            if (returnValue) {
                return true;
            }
            else {
                return false;
            }
        }

        function RemoveMessage() {
            document.getElementById("lblError").innerHTML = "";
        }

        function ClearSendTo() {
            document.getElementById("txtSendTo").value = "";
        }

        function DisplayToField() {
            //debugger
            // alert('!');
            var row_custom_email = document.getElementById("tr_custom_email_1");
            var combo_recipients = document.getElementById("ddlRecipients");
            var row_send_to = document.getElementById("tr_custom_email_2");
            var send_to = document.getElementById("txtSendTo");

            if (combo_recipients.selectedIndex == "5") {
                row_custom_email.style.display = "";
                row_send_to.style.display = "";
            }
            else {
                row_custom_email.style.display = "none";
                row_send_to.style.display = "none";
                send_to.value = "";
            }
        }

        function AddEmail() {
            //debugger
            var combo_users = document.getElementById("ddlULOUsers");
            var send_to = document.getElementById("txtSendTo");

            if (send_to.value == "") {
                send_to.value = combo_users.value;
            }
            else {
                send_to.value = send_to.value + "," + combo_users.value;
            }
            document.getElementById("txtSendTo").value = send_to.value;
        }

        function trim(str) {
            return str.replace(/^[\s]+/, '').replace(/[\s]+$/, '').replace(/[\s]{2,}/, ' ');
        }

        function _valid() {

            var b_error = false;
            var sError = "";

            //debugger
            var combo_rec = document.getElementById("ddlRecipients");
            var ulo_users = document.getElementById("ddlULOUsers");
            var rec = trim(combo_rec.options[combo_rec.selectedIndex].value);
            var users = trim(ulo_users.options[ulo_users.selectedIndex].value);
            //alert(users);

            var send_to = document.getElementById("txtSendTo");// txtSubject txtBody
            var subject = document.getElementById("txtSubject");
            var body = document.getElementById("txtBody");



            if (combo_rec.selectedIndex == 0) {
                sError += "'Recipient Group' is not selected<br>";
            }

            if (combo_rec.selectedIndex == 5)// manual entry
            {
                if (users == "- select -") {
                    sError += "'ULO Users' are not selected<br>";
                }

                if (trim(send_to.value) == "") {
                    sError += "'Send To ' field is empty. Please add email address using the 'ULO Users' combo<br>";
                }
            }

            if (trim(body.value) == "") {
                sError += "Please enter 'Body' text<br>";
            }

            if (trim(subject.value) == "") {
                sError += "Please enter 'Subject' text<br>";
            }

            if (sError != "") {
                document.getElementById("lblError").innerHTML = sError;
                return false;
            }
            else {
                //return true;
            }

            var returnValue = confirm('Are you sure you want to send email?');


            if (returnValue) {
                return true;
            }
            else {
                return false;
            }

            //return true;

        }

    </script>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="contentTitle" runat="Server">
    Send ULO Emails
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="content" runat="Server">
    <div class="section">
        <table width="100%" border="0">
            <tr>
                <td>
                    <asp:Label ID="lblError" runat="server" CssClass="error" Text=""></asp:Label>
                </td>
                <td align="right">
                    <a id="lnkHelp" target="_blank" href="docs/How_To_Send_ULO_Email.pdf" style="color: Blue; border-style: none" runat="server">Help</a>
                </td>
            </tr>
        </table>
        <div class="section1">
            <table width="100%" border="0" class="regBldBlueText" cellpadding="0" cellspacing="0">
                <tr>
                    <td style="width: 15%;"></td>
                    <td style="width: 80%;"></td>
                    <td style="width: 5%;"></td>
                </tr>

                <tr>
                    <td colspan="3">
                        <hr class="thickHR" />
                    </td>
                </tr>

                <tr>
                    <td colspan="3">
                        <asp:RadioButtonList ID="rbEmailType" runat="server" RepeatDirection="Horizontal" AutoPostBack="True" OnSelectedIndexChanged="rbEmailType_SelectedIndexChanged">
                            <asp:ListItem Value="1">Send Due Date Reminder</asp:ListItem>
                            <asp:ListItem Value="2">Send Custom Email</asp:ListItem>
                        </asp:RadioButtonList></td>
                </tr>



                <asp:Panel runat="server" Visible="true" ID="panDueReminder">
                    <tr class="evenTD">
                        <td colspan="3">To send 'Due Date Reminder' email select an active review from the combo box below:
                        </td>
                    </tr>

                    <tr class="oddTD" style="height: 30px;">
                        <td>Review:</td>
                        <td colspan="2">
                            <asp:DropDownList ID="ddlLoads" CssClass="regBldBlueText" Width="67%" runat="server" OnSelectedIndexChanged="ddlLoads_SelectedIndexChanged" AutoPostBack="True">
                            </asp:DropDownList>
                        </td>

                    </tr>

                    <tr valign="middle" style="height: 40px">
                        <td colspan="3">
                            <asp:Label ID="lblDueDateMessage" runat="server" CssClass="error" Text=""></asp:Label></td>
                    </tr>

                    <tr class="evenTD" style="height: 50px;">
                        <td colspan="3">
                            <asp:Button ID="btnSendDueDateReminder" Width="115px" ToolTip="Send email reminder" CssClass="button" runat="server" Text="Send Reminder" OnClick="btnSendDueDateReminder_Click" /></td>
                    </tr>
                </asp:Panel>


                <tr>
                    <td colspan="3">
                        <hr class="thickHR" />
                    </td>
                </tr>

                <asp:Panel runat="server" Visible="true" ID="panCustomEmail">
                    <tr class="evenTD">
                        <td>Select Recipient<br />
                            Group:
                        </td>
                        <td colspan="2">
                            <asp:DropDownList ID="ddlRecipients" AutoPostBack="true" OnSelectedIndexChanged="ddlRecipients_SelectedIndexChanged" CssClass="regText" runat="server" Width="25%">
                                <asp:ListItem Selected="True" Value="0">- Select -</asp:ListItem>
                                <asp:ListItem Value="1">ULO Reviewers</asp:ListItem>
                                <asp:ListItem Value="2">ULO Organization Admins</asp:ListItem>
                                <asp:ListItem Value="3">ULO Budget Division Admins</asp:ListItem>
                                <asp:ListItem Value="4">All ULO Users</asp:ListItem>
                                <asp:ListItem Value="5">Manual Entry</asp:ListItem>
                            </asp:DropDownList>
                        </td>
                    </tr>
                    <tr style="height: 20px">
                        <td colspan="3"></td>
                    </tr>

                    <tr>
                        <td colspan="3">
                            <input type="hidden" id="hMessageHeader" runat="server" />
                        </td>
                    </tr>


                    <tr valign="middle" id="tr_custom_email_1" runat="server" class="evenTD" style="display: none;">
                        <td>ULO Users:</td>
                        <td colspan="2">
                            <asp:DropDownList ID="ddlULOUsers" runat="server" Width="25%"></asp:DropDownList>
                        </td>
                    </tr>
                    <tr style="height: 20px">
                        <td colspan="3"></td>
                    </tr>

                    <tr valign="middle" id="tr_custom_email_2" runat="server" class="evenTD" style="display: none;">
                        <td>Send To:
                        </td>
                        <td valign="middle" colspan="2" ondblclick="ClearSendTo()" title="Double-click to clear this field">
                            <asp:TextBox ID="txtSendTo" onkeydown="event.returnValue=false;" TextMode="MultiLine" Rows="2" runat="server" CssClass="disabled_textbox" Width="99%"></asp:TextBox>
                        </td>
                    </tr>

                    <tr style="height: 20px">
                        <td colspan="3"></td>
                    </tr>

                    <tr class="evenTD">
                        <td>Subject:</td>
                        <td colspan="2">
                            <asp:TextBox ID="txtSubject" runat="server" Rows="1" TextMode="MultiLine" Width="99%"></asp:TextBox>
                        </td>
                    </tr>
                    <tr style="height: 20px">
                        <td colspan="3"></td>
                    </tr>

                    <tr class="evenTD" valign="top">
                        <td valign="top">Body:
                        </td>
                        <td colspan="2">
                            <asp:TextBox ID="txtBody" runat="server" Rows="20" TextMode="MultiLine" Width="99%"></asp:TextBox>
                        </td>
                    </tr>

                    <tr>
                        <td colspan="3" style="height: 20px;"></td>
                    </tr>

                    <tr>
                        <td colspan="3" style="height: 20px;"></td>
                    </tr>

                    <tr id="tr_graphics" runat="server">
                        <td></td>
                        <td colspan="2">If you'd like to have part of the body text in <b>Bold</b>, please copy and paste it in the
                        text field below separated by '^' character:
                        <asp:TextBox ID="txtGraphicsBold" TextMode="MultiLine" Rows="3" Font-Bold="true" runat="server" Width="99%"></asp:TextBox><br />
                            <br />
                            If you'd like to have part of the body text in <font color='red'>Red</font>color, please copy and it them in the
                        text field below separated by '^' character:
                        <asp:TextBox ID="txtGraphicsRed" TextMode="MultiLine" Rows="3" ForeColor="red" runat="server" Width="99%"></asp:TextBox><br />
                            <br />
                            If you'd like to have part of the body text in <font color='Blue'>Blue</font>color, please please copy and paste it in the
                        text field below separated by '^' character:
                        <asp:TextBox ID="txtGraphicsBlue" TextMode="MultiLine" Rows="3" ForeColor="blue" runat="server" Width="99%"></asp:TextBox><br />
                            <br />
                            If you'd like to have part of the body text in <font color='green'>Green</font>color, please copy and paste it in the
                        text field below separated by '^' character:
                        <asp:TextBox ID="txtGraphicsGreen" TextMode="MultiLine" Rows="3" ForeColor="green" runat="server" Width="99%"></asp:TextBox><br />
                            <br />
                            <asp:Button ID="btnApplyGraphics" Visible="false" Width="80px" ToolTip="Show Body Text in HTML format" CssClass="button" runat="server" Text="Apply Graphics" OnClick="btnApplyGraphics_Click" />
                        </td>
                    </tr>
                    <tr class="evenTD">
                        <td></td>
                        <td colspan="2">
                            <asp:Button ID="btnSendEmail" Width="80px" ToolTip="Send email" CssClass="button" runat="server" Text="Send Email" OnClientClick="return _valid()" OnClick="btnSendEmail_Click" />
                        </td>
                    </tr>


                </asp:Panel>

            </table>
        </div>

        <div id="lower_section" runat="server" class="section_lower">
            <table width="100%" border="0" class="regBldBlueText">

                <tr>
                    <td style="width: 20%">&nbsp;
                    </td>
                    <td style="width: 50%">&nbsp;&nbsp;
                            <input type="hidden" id="hUserID" runat="server" />
                        &nbsp; &nbsp;<input type="hidden" id="txtLoadDate" runat="server" /></td>
                    <td>
                        <input type="hidden" id="txtDueDate" runat="server" /></td>
                </tr>
            </table>
        </div>
    </div>
</asp:Content>
