<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/MasterPages/ULO/ULOMenu.master" Inherits="GSA.OpenItems.Web.ArchiveLoad" Codebehind="ArchiveLoad.aspx.cs" %>

<%@ Register Src="Controls/multigrid.ascx" TagName="multigrid" TagPrefix="uc3" %>
<%@ Register Src="Controls/Menu.ascx" TagName="Menu" TagPrefix="uc1" %>
<%@ Register Src="Controls/dropdowngrid.ascx" TagName="dropdowngrid" TagPrefix="uc2" %>

<asp:Content ID="Content1" ContentPlaceHolderID="title" runat="Server">
    Archive/Unarchive Review
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="head" runat="Server">
     <script type="text/jscript">
 function RemoveMessage()
 {
    //alert('!');
    document.getElementById("lblError").innerHTML = "";
 }
 function mgClick()
    { 
  //debugger 
        var btn_archive = document.getElementById("btnArchive");
        var btn_unarchive = document.getElementById("btnUnarchive");   
        gd = GetSelectedRow("mgLoads"); 
        load_status = gd[1].innerHTML;
        if(load_status.toLowerCase()=="<font color=green>active</font>")
        {
            btn_archive.style.display = "";
            btn_unarchive.style.display = "none";
        }
        else
        {
            btn_archive.style.display = "none";
            btn_unarchive.style.display = "";
        }
     }  
  </script> 
</asp:Content>
<asp:Content ID="Content5" ContentPlaceHolderID="contentTitle" runat="Server">
    Archive/Unarchive Review
</asp:Content>
<asp:Content ID="Content6" ContentPlaceHolderID="content" runat="Server">
             <table  width="100%" border="0">
            <tr>
                    <td >
                        <asp:Label ID="lblError" runat="server" CssClass="error" Text=""></asp:Label>
                    </td>
                    <td align="right">
                        <a ID="lnkHelp" target="_blank" href="docs/How_To_Archive_Review.pdf" style="color:Blue; border-style:none"  runat="server">Help</a>
                    </td>
                </tr>
        </table>
        <div class="section">
            <table width="100%" class="regBldBlueText" cellpadding="2" cellspacing="2" >
                <tr>
                    <td colspan="2">
                     </td>
                </tr>
                
                <tr height="20px">
                    <td>
                    </td>
                    <td>
                    </td>
                    
                </tr>
                
                <tr>
                    <td colspan="2">
                        <hr class="thickHR" />
                    </td>
                    

                </tr>
                               
                <tr height="20px">
                    <td>
                    </td>
                    <td >
                        Select any review from the list below and then click the "Archive" (or "Unarchive") button.
                    </td>
                    
                </tr>
                
                
                 <tr  class="oddTD" height="30px">
                    <td >
                       Review:
                    </td>
                    <td >
                        <uc3:multigrid ID="mgLoads" TblCSSClass="tblGridYellow" Width="100%" ItemCSSClass="ArtYellow" OnChange= "mgClick();" runat="server" Height = "300px" />
                   </td>

                </tr>
                
            </table>
                </div>
                
                <div id="lower_section" runat="server" class="section_lower">
                <table  width="100%" border="0" class="regBldBlueText">
                    
                    <tr>
                        <td colspan="3">
                            &nbsp;
                            <hr class="thickHR" />
                        </td>
                    </tr>
                    
                    <tr >               
                        <td>
                            <asp:Button ID="btnArchive" Width="70px" CssClass="button"  runat="server" Text="Archive" OnClick="btnArchive_Click" />
                            &nbsp;&nbsp; &nbsp;<asp:Button ID="btnUnarchive" Width="70px" CssClass="button"  runat="server" Text="Unarchive" OnClick="btnUnarchive_Click"  /></td>
                        <td>
                            &nbsp;&nbsp;
                         </td>
                        <td>
                            <input type="hidden" id="hUserID" runat="server" />
                        </td>
                    </tr>                
                </table>
            </div> 
</asp:Content>