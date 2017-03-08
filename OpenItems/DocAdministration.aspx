<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/ULO/ULOMenu.master" AutoEventWireup="true" CodeFile="DocAdministration.aspx.cs" Inherits="GSA.OpenItems.Web.DocAdministration" %>
<%@ Register TagPrefix="uc" TagName="CriteriaFields" Src="~/Controls/CriteriaFields.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="title" Runat="Server">
    Open Items Documents Administration
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="head" Runat="Server">
     <!--#include virtual="../include/HTTPService.js" -->
    <!--#include virtual="../include/GridLines.inc" -->    
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="contentTitle" Runat="Server">
    Open Items Documents Administration
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="content" Runat="Server">
     <div class="section">
        
    <uc:CriteriaFields ID="ctrlCriteria" runat="server" />
    
    <!-- #include virtual="../include/ErrorLabel.inc" -->
    <br />    
    
    <table width="100%">
        <tr><td align="left"><asp:Label ID="lblMessages" runat ="server" /></td></tr>            
    </table>    
    
    <asp:GridView ID="gvItems" runat="server" CellPadding="0" AllowPaging="True" AutoGenerateColumns="false"
                AllowSorting="true" CssClass="" Width="100%" HorizontalAlign="Center" 
				CellSpacing="0" EnableViewState="false" HeaderStyle-CssClass="" 
				UseAccessibleHeader="true" PagerSettings-Mode="NumericFirstLast" 
				RowStyle-CssClass="TDeven" AlternatingRowStyle-CssClass="TDodd" PageSize="30" PagerStyle-CssClass="Pages" >
        <Columns>                            
            <asp:BoundField DataField="DocNumber" HeaderText="DocNumber" SortExpression="DocNumber" />
            <asp:BoundField DataField="Title" HeaderText="Title" SortExpression="Title" />
            <asp:BoundField DataField="TotalLine" HeaderText="TotalLine" SortExpression="TotalLine" ItemStyle-HorizontalAlign="right" DataFormatString="{0:$0,0}" HtmlEncode="false"/>
            <asp:BoundField DataField="AwardNumber" HeaderText="AwardNumber" SortExpression="AwardNumber" />            
            <asp:BoundField DataField="AllDocs" HeaderText="All Documents" SortExpression="AllDocs" ItemStyle-HorizontalAlign="Center" />       
            <asp:BoundField DataField="Relevant" HeaderText="New Documents (this load)" SortExpression="Relevant" ItemStyle-HorizontalAlign="Center"/>
            <asp:BoundField DataField="SentDocs" HeaderText="Submitted to CO" SortExpression="SentDocs" ItemStyle-HorizontalAlign="Center"/>
            <asp:BoundField DataField="DocsToSend" HeaderText="Not Yet Submitted" SortExpression="DocsToSend" ItemStyle-HorizontalAlign="Center"/>            
        </Columns>
        </asp:GridView>

    </div>
</asp:Content>

