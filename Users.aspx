<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/ULO/ULOMenu.master" AutoEventWireup="true" CodeFile="Users.aspx.cs" Inherits="GSA.OpenItems.Web.Users" %>
<%@ Register TagPrefix="uc3" TagName="multigrid" Src="~/Controls/multigrid.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="title" runat="Server">
    Manage Users
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="head" runat="Server">
    <script type="text/javascript">

        function GetHelp(s) {
            if (s == "phone") {
                alert("This field is optional.\r\n\r\n\Please use the following phone formats:\r\n\(XXX)XXX-XXXX\r\n\XXX-XXX-XXXX\r\n\XXX.XXX.XXXX\r\n\XXX XXX XXXX\r\n\XXXXXXXXXX\r\n\r\n\Brackets and hyphens are optional.");
            }
            else if (s == "password") {
                alert("FOR NEW USERS\r\n\This field is optional. If left blank, generic password will be inserted.\r\n\r\n\FOR EXISTING USERS\r\n\If left blank, the previous password will be preserved. Enter a new password if needed.");
            }
            else {
            }
        }


        function ExecuteConfirm() {
            //debugger

            var returnValue = confirm('Are you sure you want to save the record and send email to the user?');


            if (returnValue) {
                return true;
            }
            else {
                return false;
            }
        }

        function _valid() {

            var b_error = false;
            var sError = "";




            var e = document.getElementById("ddlOrg"); // select element  //ddlUsers
            var org = e.options[e.selectedIndex].value;

            var view_mode = document.getElementsByName("rbListNewOld");

            if (view_mode[1].checked || view_mode[3].checked) {
                var u = document.getElementById("ddlUsers");
                var user = u.options[u.selectedIndex].value;
                if (view_mode[1].checked || view_mode[3].checked)  // new NCR user, or existing ULO user
                {
                    if (user == "- select -") {
                        sError += "'User' is not selected<br>";
                    }
                }
            }
            //alert(sError);


            if (org == "- select -") {
                sError += "'Organization' is not selected<br>";
            }
            // alert(sError);

            if (GetSelectedIndex("mgUserRole") < 0) {
                sError = sError + "'User Role' is not selected<br>";
            }

            //debugger


            if (view_mode[2].checked)  // new non-NCR user
            {
                if (trim(document.getElementById("txtFirstName").value) == "") {
                    sError += "Please enter 'First Name'<br>";
                }

                if (trim(document.getElementById("txtLastName").value) == "") {
                    sError += "Please enter 'Last Name'<br>";
                }

                var phone = document.getElementById("txtPhone").value;

                if (trim(phone) != "") {
                    if (validatePhoneNumber(phone)) {
                    }
                    else {
                        sError += "Invalid Phone Number format entered<br>";
                    }
                }
            }

            if (trim(document.getElementById("txtEmail").value) == "") // start email validation
            {
                sError += "'GSA Email Address' is not entered<br>";
            }
            else {
                var email = trim(document.getElementById('txtEmail').value);
                //alert(email);

                var filter = /^([a-zA-Z0-9_\.\-])+\@(([a-zA-Z0-9\-])+\.)+([a-zA-Z0-9]{2,4})+$/;


                if (!filter.test(email)) {
                    sError += "'GSA Email Address' is not valid'<br>";
                }
                else // valid
                {
                    var sentence = email.toLowerCase();


                    if (sentence.indexOf("@gsa.gov") != -1) // missing the '@gsa.gov' part of the GSA adde
                    {
                        if (sentence.length > 8) {
                            gsa_gov = sentence.substr(sentence.length - 8);
                            //alert (gsa_gov);
                            if (gsa_gov.toLowerCase() != "@gsa.gov") {
                                sError += "'GSA Email Address' is not valid. '<br>";
                            }
                        }
                        else {
                            sError += "'GSA Email Address' is not valid. '<br>";
                        }
                    }
                    else {
                        //alert("gsa.gov is NOT found !!!")
                        sError += "'GSA Email Address' is not valid..'<br>";
                    }
                }
            } // end email validation

            //debugger
            if (sError != "") {
                document.getElementById("lblError").innerHTML = sError;
                return false;
            }
            else {
                //return true;
            }

            var returnValue = confirm('Are you sure you want to save the record and send email to the user?');


            if (returnValue) {
                return true;
            }
            else {
                return false;
            }

            //return true;

        }

        function validatePhoneNumber(elementValue) {
            var phoneNumberPattern = /^\(?(\d{3})\)?[- ]?(\d{3})[- ]?(\d{4})$/;
            return phoneNumberPattern.test(elementValue);
        }

        function trim(stringToTrim) {
            return stringToTrim.replace(/^\s+|\s+$/g, "");
        }

    </script>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="contentTitle" runat="Server">
    <asp:Label ID="lblTitle" runat="server" Text="Manage Users"></asp:Label>
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="content" runat="Server">
    <table width="100%" border="0">
        <tr>
            <td>
                <asp:Label ID="lblError" runat="server" CssClass="error" Text=""></asp:Label>
            </td>
            <td align="right">
                <a id="lnkHelp" target="_blank" href="docs/How_To_Add_New_ULO_User.pdf" style="color: Blue; border-style: none" runat="server">Help</a>
            </td>
        </tr>
    </table>
    <div class="section">
        <table width="60%" border="0" class="regBldBlueText" cellpadding="1" cellspacing="1">
            <tr>
                <td colspan="2" id="td_rb" runat="server">
                    <asp:RadioButtonList ID="rbListNewOld" runat="server" RepeatDirection="Horizontal" Width="100%" AutoPostBack="True" OnSelectedIndexChanged="rbListNewOld_SelectedIndexChanged">
                        <asp:ListItem Selected="True" Value="0">Add New NCR User</asp:ListItem>
                        <asp:ListItem Value="1">Add New Non-NCR User</asp:ListItem>
                        <asp:ListItem Value="2">Edit Existing ULO User</asp:ListItem>

                    </asp:RadioButtonList></td>

            </tr>

            <tr height="20px">
                <td></td>
                <td></td>
            </tr>

            <tr>
                <td colspan="2">
                    <hr class="thickHR" />
                </td>

            </tr>
            <tr id="tr_user" runat="server" class="oddTD" height="30px" style="display: none;">
                <td width="50%">&nbsp;User <span style="font-size: 1.3em; color: red">*</span>:</td>
                <td width="50%">
                    <asp:DropDownList ID="ddlUsers" Width="100%" runat="server" OnSelectedIndexChanged="ddlUsers_SelectedIndexChanged" AutoPostBack="True">
                    </asp:DropDownList>
                </td>

            </tr>

            <tr class="evenTD" height="30px">
                <td width="50%">&nbsp;First Name <span style="font-size: 1.3em; color: red">*</span>:</td>
                <td width="50%">
                    <asp:TextBox ID="txtFirstName" runat="server" Width="98%" CssClass="disabled_textbox"></asp:TextBox>
                </td>

            </tr>

            <tr class="oddTD" height="30px">
                <td>&nbsp;Last Name <span style="font-size: 1.3em; color: red">*</span>:</td>
                <td>
                    <asp:TextBox ID="txtLastName" runat="server" Width="98%" CssClass="disabled_textbox"></asp:TextBox>
                </td>

            </tr>

            <tr class="evenTD" height="30px">
                <td>&nbsp;MI:</td>
                <td>
                    <asp:TextBox ID="txtMI" runat="server" Width="98%" ToolTip="optional field" CssClass="disabled_textbox"></asp:TextBox>
                </td>

            </tr>



            <tr class="oddTD" height="30px">
                <td>&nbsp;Organization <span style="font-size: 1.3em; color: red">*</span>:</td>
                <td>
                    <asp:DropDownList ID="ddlOrg" Width="100%" runat="server">
                    </asp:DropDownList>
                </td>

            </tr>

            <tr class="evenTD" height="30px">
                <td>&nbsp;Phone (optional):&nbsp;<img id="phone_help" runat="server" src="images/question_mark.gif" style="cursor: pointer; width: 9px; height: 9px" onclick="GetHelp('phone')" alt="" title="phone formats" /></td>
                <td>
                    <asp:TextBox ID="txtPhone" MaxLength="20" runat="server" Width="98%" ToolTip="optional field" CssClass="enabled_textbox"></asp:TextBox>
                </td>

            </tr>


            <tr class="oddTD" height="30px" id="tr_user_role" runat="server">
                <td>&nbsp;User Role <span style="font-size: 1.3em; color: red">*</span>:</td>
                <td>
                    <uc3:multigrid id="mgUserRole" tblcssclass="tblGridYellow" itemcssclass="ArtYellow" width="100%" runat="server" />
                </td>

            </tr>
            <tr class="evenTD" height="30px">
                <td>&nbsp;GSA Email Address <span style="font-size: 1.3em; color: red">*</span>:</td>
                <td>
                    <asp:TextBox ID="txtEmail" runat="server" Width="98%" CssClass="disabled_textbox"></asp:TextBox>
                </td>

            </tr>

            <tr class="oddTD" height="30px" style="display: none">
                <td>&nbsp;Default Application:</td>
                <td>
                    <asp:DropDownList ID="ddlDefAppl" CssClass="regBldBlueText" Width="100%" runat="server">
                        <asp:ListItem Selected="True" Value="1">ULO</asp:ListItem>
                        <asp:ListItem Value="2">Fund Status</asp:ListItem>
                    </asp:DropDownList>
                </td>

            </tr>

            <tr class="oddTD" height="30px">
                <td>&nbsp;User Status:</td>
                <td style="height: 25px">
                    <asp:DropDownList ID="ddlActive" CssClass="regBldBlueText" Width="100%" runat="server">
                        <asp:ListItem Selected="True" Value="1">Active</asp:ListItem>
                        <asp:ListItem Value="0">Inactive</asp:ListItem>
                    </asp:DropDownList>
                </td>
            </tr>

            <tr class="evenTD" height="30px" id="tr_password" runat="server">
                <td>&nbsp;Password:&nbsp;
                    <img id="pswd_help" runat="server" src="images/question_mark.gif" style="cursor: pointer; width: 9px; height: 9px;" onclick="GetHelp('password')" alt="" title="click for password help" /></td>
                <td>
                    <asp:TextBox ID="txtPassword" runat="server" Text="pswd" ToolTip="Type new if needed" CssClass="regBldBlueText" Width="98%" TextMode="Password"></asp:TextBox>
                </td>
            </tr>

        </table>
    </div>

    <div id="lower_section" runat="server" class="section_lower">
        <table width="60%" border="0" class="regBldBlueText">

            <tr id="tr_note" runat="server">
                <td colspan="2">All fields marked with an asterisk (<span style="font-size: 1.3em; color: red">*</span>) are required.
                            <hr class="thickHR" />
                </td>
            </tr>

            <tr align="right">
                <td style="height: 29px">
                    <input type="hidden" id="hUserID" runat="server" />
                    <input type="hidden" id="hPassword" runat="server" />
                </td>
                <td style="height: 29px">
                    <asp:Button ID="btnReset" Width="70px" CssClass="button" runat="server" Text="Reset" OnClick="btnReset_Click" />&nbsp;
                            <asp:Button ID="btnSave" Width="70px" CssClass="button" runat="server" Text="Save" OnClientClick="return _valid();" OnClick="btnSave_Click" />
                </td>
            </tr>
        </table>
    </div>
</asp:Content>
