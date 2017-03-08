<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/ULO/ULOMenu.master" AutoEventWireup="true" CodeFile="FundsReview.aspx.cs" Inherits="GSA.OpenItems.Web.FundsReview" %>
<%@ Register TagPrefix="uc" TagName="FundsCriteria" Src="~/Controls/FundsCriteria.ascx" %>


<asp:Content ID="Content1" ContentPlaceHolderID="title" Runat="Server">
    <!--Originally had BA61 Fund Status as Application Title -->
    Funds Review
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="head" Runat="Server">
    <link href="../css/fund_status.css" type="text/css" rel="stylesheet" />
    <!--#include virtual="../include/HTTPService.js" -->
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="contentTitle" Runat="Server">
    Funds Review
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="content" Runat="Server">
     <div class="section">
        
    <uc:FundsCriteria ID="ctrlCriteria" runat="server" />
    
    <table width ="100%">
        <tr><td><!-- #include virtual="../include/ErrorLabel.inc" --></td></tr>
        <tr><td><asp:Label ID="lblMessages" runat ="server" CssClass="regBldGreyText" /></td></tr>        
    </table>        
    <br />           
    
    <p />
    <div id="divTable" >
        <table id="tblData" runat="server" onmouseover="ChangeClass(event,'tableLinkHover')" onmouseout="ChangeClass(event,'tableLink')" />        
    </div>
    
    </div> 
</asp:Content>

