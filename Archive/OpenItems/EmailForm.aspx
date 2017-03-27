<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Popups/Popup.master" AutoEventWireup="true" Inherits="GSA.OpenItems.Web.EmailForm" Codebehind="EmailForm.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="title" Runat="Server">
    Open Items - Email
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="head" Runat="Server">
     <script type ="text/javascript" >
    
    var email_field = "";
    var email_list_txtTo = "";
    var email_list_txtCc = "";      
    if (document.getElementById("txtTo"))
    {
        if (document.getElementById("txtTo").value != "")
            email_list_txtTo = document.getElementById("txtTo").value + ",";
    }
    
    function display_comments(ctrl)
    {
        if (ctrl.checked)
            document.getElementById("txtBody").innerText = document.getElementById("txtComments").value;
        else
            document.getElementById("txtBody").innerText = "";                    
    }               
    
    function add_email(txtName)
    {
        email_field = txtName;
        
        var popup = window.open("SearchPerson.aspx?mode=email", "OpenItemsForEmail", 
            "width=600,height=400,menubar=no,status=no,resizable=yes,scrollbars=yes,toolbar=no,top=400,left=450");    
        
        popup.focus();
        
        return false;
    }
    
    function insert_email_address(email)
    {                       
        if (email_field == "txtTo")
        {
            email_list_txtTo = email_list_txtTo + email + ",";
            document.getElementById(email_field).innerText = email_list_txtTo;
        }
        else
        {
            email_list_txtCc = email_list_txtCc + email + ",";
            document.getElementById(email_field).innerText = email_list_txtCc;
        }                        
    }
    </script>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="contentTitle" Runat="Server">
    Send Email
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="content" Runat="Server">
    <table width="100%" cellspacing="5">
        <tr>
            <td class="regBldText" valign="top" >
                <input type="button" id="btnTo" runat="server" value="To" title="Search Email Address" onclick="javascript:return add_email('txtTo');" style="width: 70px"/>
                <asp:Label ID="lblTo" runat="server" Text="To:" Visible="false" />
            </td>
            <td valign="top" >
                <asp:TextBox ID="txtTo" runat="server" CssClass="" MaxLength="600" Width="455px" />
                <asp:DropDownList ID="ddlTo" runat="server" Visible="false" Width="455px" />    
            </td>
        </tr>
        <tr>
            <td class="regBldText" valign="top" >
                <input type="button" id="btnCc" value="Cc" title="Search Email Address" onclick="javascript:add_email('txtCc');" style="width: 70px"/>
            </td>
            <td valign="top" ><asp:TextBox ID="txtCc" runat="server" CssClass="" MaxLength="600" Width="455px" /></td>
        </tr>
        <tr>
            <td class="regBldText" valign="top" >Subject:</td>
            <td valign="top" ><asp:TextBox ID="txtSubject" runat="server" CssClass="" TextMode="multiline" Rows="2" MaxLength="200" Width="455px" /></td>
        </tr>
        <tr id="trAttachments" runat="server" >
            <td class="regBldText" valign="top" >Attachments:</td>
            <td>
                <table id="tblAtt" runat="server" width="100%" >
                    <tbody>
                    </tbody>
                </table>
            </td>            
        </tr>
        <tr id="trCheckbox" runat="server" >
            <td ><input type="hidden" id="txtComments" runat="server"  /></td>
            <td valign="top" ><input type="checkbox" id="chkComments" onclick="return display_comments(this);" /><span class="regBldText">Include default content</span></td>
        </tr>
        <tr>
            <td class="regBldText" valign="top" >Body:</td>
            <td ><asp:TextBox ID="txtBody" runat="server" CssClass="" TextMode="MultiLine" MaxLength="800" Width="455px" Rows="7"/></td>
        </tr>
        <tr>            
            <td colspan="2" align="right" valign="top" >
                <input type="button" id="btnCancel" class="button" value="Cancel" title="Cancel" onclick="javascript:self.close();" />&nbsp;&nbsp;&nbsp;
                <asp:Button ID="btnSendEmail" runat="server" CssClass="button" Text="Send Email" ToolTip="Send Email" />
            </td>
        </tr>        
        <tr><td><asp:Label ID="lblMessage" runat="server" Visible="false" Text="" CssClass="errorsum" Width="400"/></td></tr>
    </table>
</asp:Content>

