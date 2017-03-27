<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/ULO/ULOMenu.master" AutoEventWireup="true" Inherits="GSA.OpenItems.Web.Justification" Codebehind="Justification.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="title" Runat="Server">
    Manage  Justifications
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="head" Runat="Server">
     <script type="text/jscript">
 function ShowAddField()
 { 
 //debugger
      
    var chk =  document.getElementById("chkAddField");
    var tr = document.getElementById("tr_txtAddFieldValue");
    var txt = document.getElementById("txtAddFieldValue");
    var j = document.getElementById("txtJustType");
    var h = document.getElementById("hJustTypeCode");
    
    if(chk.nextSibling.innerHTML == "Add Additional field")
    {

        tr.style.display = "inline"; 
        chk.nextSibling.innerHTML = "Remove Additional field";
        txt.value = ""; 
        
    }
    else
    {
        if(trim(j.value) != "")
        {
            if(h.value=="0") // new Justification
            {
                var returnValue= confirm('Are you sure you do not need additional field for new Justification?'); 
            }
            else // existing Justification
            {   
                var returnValue= confirm('Are you sure you do not need additional field for current Justification?'); 
            }
        	
	        if(returnValue)            
	        { 
                tr.style.display = "none"; 
                chk.nextSibling.innerHTML = "Add Additional field";
                chk.checked = false; 
                txt.value = "";            
                return true;                                    
	        } 
	        else
	        {
	            tr.style.display = "inline"; 
                chk.nextSibling.innerHTML = "Remove Additional field"; 
                chk.checked = false;  
	            return false;
	        }  
	    }
	    else
	    {
            tr.style.display = "none"; 
            chk.nextSibling.innerHTML = "Add Additional field";
            chk.checked = false; 
            txt.value = ""; 
	    }
	       
    }
    
    chk.checked = false;
 }
 
function trim(stringToTrim) 
{
    return stringToTrim.replace(/^\s+|\s+$/g,"");
}
 </script> 
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="contentTitle" Runat="Server">
    Manage  Justifications
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="content" Runat="Server">
     <div class="section">
                        <table width="90%" border="0" class="regBldBlueText">
                <tr>
                    <td >
                        <asp:Label ID="lblError" runat="server" CssClass="error" Text=""></asp:Label>
                    </td>
                    <td align="right">
                        <a ID="lnkHelp" target="_blank" href="docs/How_To_Manage_ULO_Justification.pdf" style="color:Blue; border-style:none"  runat="server">Help</a>
                    </td>
                </tr>
                <tr>
                    <td colspan="2">
                        <asp:RadioButtonList ID="rbEditOrNew" AutoPostBack="true" runat="server" RepeatDirection="Horizontal" Width="100%" OnSelectedIndexChanged="rbEditOrNew_SelectedIndexChanged">
                            <asp:ListItem Value="0">Create New Justification</asp:ListItem>
                            <asp:ListItem Value="1">Edit Existing Justification</asp:ListItem>
                        </asp:RadioButtonList></td>
                </tr>
                <tr>
                    <td colspan="2">
                        <hr class="thinHR" />
                    </td>

                </tr>
                
                <tr   id="tr_ddlJustTypes" runat="server">
                    <td style="width:30%">Select Justification:
                    </td>
                    <td>
                        <asp:DropDownList ID="ddlJustTypes" runat="server" AutoPostBack="True" OnSelectedIndexChanged="ddlJustTypes_SelectedIndexChanged"
                            Width="100%">
                        </asp:DropDownList></td>
                </tr>
                <tr style=" height:30px">
                    <td style="width:30%">
                    </td>
                    <td></td>
                </tr>
                
                <tr id="tr_txtJustTypes" runat="server">
                    <td>
                        Justification:
                    </td>
                    <td>
                        <asp:TextBox ID="txtJustType" runat="server" CssClass="enabled_textbox" MaxLength="100"
                            Width="100%" ></asp:TextBox>
                    </td>
                </tr>
                <tr>
                    <td colspan="2">
                        <hr  class="thinHR_2" />
                    </td>

                </tr>
               
                    <tr id="tr_chkAddField" runat="server" >
                        <td colspan="2">
                            <asp:CheckBox ID="chkAddField"  Onclick="ShowAddField()"  runat="server" Text="Add Additional field" Width="200px" />
                        </td>
                    </tr>
                     <tr style=" height:20px" >
                        <td ></td>
                        <td></td>
                    </tr>
                    <tr id="tr_txtAddFieldValue"   style="display:none" runat="server" >
                        <td >
                            Additional Field Description:</td>
                        <td>
                            <asp:TextBox ID="txtAddFieldValue" runat="server" CssClass="enabled_textbox" MaxLength="200"
                                Width="98%" Rows="3" TextMode="MultiLine"></asp:TextBox>
                        </td>
                    </tr>
                
                <tr>
                    <td >
                    </td>
                    <td>
                    </td>
                </tr>
               
                <tr>
                    <td colspan="2" id="tr_line2" runat="server">
                        &nbsp;
                        <hr class="thinHR" />
                    </td>
                </tr>
                <tr>
                    <td style="width:30%">
                        <asp:Button ID="btnSave" runat="server" CssClass="button" OnClick="btnSave_Click"
                             Text="Save" Width="70px" /></td>
                    <td>
                        <input id="hJustTypeCode" runat="server" type="hidden" /></td>
                </tr>
                
            </table>
        </div>
</asp:Content>