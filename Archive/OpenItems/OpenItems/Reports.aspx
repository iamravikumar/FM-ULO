<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/ULO/ULOMenu.master" AutoEventWireup="true" Inherits="GSA.OpenItems.Web.Reports" Codebehind="Reports.aspx.cs" %>
<%@ Register TagPrefix="uc" TagName="Menu" Src="~/Controls/Menu.ascx" %>
<%@ Register TagPrefix="uc" TagName="CriteriaFields" Src="~/Controls/CriteriaFields.ascx"  %>

<asp:Content ID="Content1" ContentPlaceHolderID="title" Runat="Server">
    Open Items Report List
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="contentTitle" Runat="Server">
    Open Items Report List
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="content" Runat="Server">
      <div class="section">
    
    <uc:CriteriaFields ID="ctrlCriteria" runat="server" />            
    
    <!-- #include virtual="../include/ErrorLabel.inc" -->
    
    <table width="100%" cellpadding="10" >
        <tr><td class="lrgBldText">Please Select Report: 
            <asp:Label ID="lblErrorBA53"  ForeColor="red" Font-Bold="false" runat="server" Text="" Width="699px"></asp:Label> </td></tr>
        <tr><td>
            <input type="button" id="btnDARA" value="DARA by Line" style="width:200px;" class="button" runat="server" />
        </td></tr>
          <tr><td>
            <input type="button" id="btnDARAByDocNum" value="DARA by Document" style="width:200px;" class="button" runat="server" />
        </td></tr>
        <tr><td>
            <input type="button" id="btnTotalDaily" value="Total Daily" style="width:200px;" class="button" runat="server" />
        </td></tr>        
        <tr><td>
            <input type="button" id="btnTotalUniverse" value="Total Universe" style="width:200px;" class="button" runat="server" />
        </td></tr>
        <tr><td>
            <input type="button" id="btnCOReport" value="CO Report" style="width:200px;" class="button" runat="server" />
        </td></tr>                
         <tr><td>
            <input type="button" id="btnValidationByLine" value="Validation By Line" style="width:200px;" class="button" runat="server" />
        </td></tr>
        <tr><td>
            <input type="button" id="btnDocuments" value="Documents" style="width:200px;" class="button" runat="server" />
        </td></tr>
        
    </table>
    
    
    </div>
</asp:Content>

