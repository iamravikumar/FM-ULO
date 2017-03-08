<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/ULO/ULOMenu.master" AutoEventWireup="true" CodeFile="FSReferDocs.aspx.cs" Inherits="GSA.OpenItems.Web.FundStatus_FSReferDocs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="title" runat="Server">
    
<!--Originally had BA61 Fund Status as Application Title -->
    Funds Status Reference Documents
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="head" runat="Server">
    <link href="../css/fund_status.css" type="text/css" rel="stylesheet" />
    <script type="text/javascript">
        function pop(w) {
            var popup = window.open(w, null, "width=600,height=600,menubar=no,status=no,resizable=yes,scrollbars=yes,toolbar=yes,top=200,left=200")
            popup.focus()
            return false
        }
    </script>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="contentTitle" runat="Server">
    Funds Status Reference Documents
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="content" runat="Server">
    <div class="section">
                  
    <table width ="100%">
        <tr><td ><!-- #include virtual="../include/ErrorLabel.inc" --></td></tr>
        <tr><td><asp:Label ID="lblMessages" runat ="server" CssClass="regBldGreyText" /></td></tr>        
    </table>        
    <br />
    
    <table width="100%" cellpadding="10" >
        <tr><td>
            <input type="button" value="Functions Summary" style="width:200px;" class="button" onclick="pop('include/Function Code Summary.xls');" />
        </td></tr>
        <tr><td>
            <input type="button" value="FY Paid Days" style="width:200px;" class="button" onclick="pop('include/Paid Days.xls');" />
        </td></tr>                       
    </table>
    
    </div> 
</asp:Content>
