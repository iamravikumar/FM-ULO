<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/ULO/ULOMenu.master" AutoEventWireup="true" CodeFile="Assignments.aspx.cs" Inherits="GSA.OpenItems.Web.Assignments" %>
<%@ Register TagPrefix="uc" TagName="CriteriaFields" Src="~/Controls/CriteriaFields.ascx" %>


<asp:Content ID="Content1" ContentPlaceHolderID="title" Runat="Server">
    Open Items Disputed Assignments
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="head" Runat="Server">
     <!--#include virtual="../include/HTTPService.js" -->
    <!--#include virtual="../include/GridLines.inc" -->
    <!--#include virtual="include/Assignments.js" -->    
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="contentTitle" Runat="Server">
    Reassignment Requests List
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="content" Runat="Server">
    <div class="section">
        
    <uc:CriteriaFields ID="ctrlCriteria" runat="server" />
    
    <!-- #include virtual="../include/ErrorLabel.inc" -->
    <br />
    <asp:Label ID="lblMessages" runat ="server" />
    <input type="hidden" id="txtReassignTargetPage" runat="server" />
    
    <div id="goBorders">
    <asp:GridView ID="gvItems" runat="server" CellPadding="0" AllowPaging="True" AutoGenerateColumns="false"
                AllowSorting="true" CssClass="" Width="100%" HorizontalAlign="Center" 
				CellSpacing="0" EnableViewState="false" HeaderStyle-CssClass="" 
				UseAccessibleHeader="true" PagerSettings-Mode="NumericFirstLast" 				
				RowStyle-CssClass="TDborder" AlternatingRowStyle-CssClass="TDborder" 				
				PageSize="30" PagerStyle-CssClass="Pages" >
				
        <Columns>                            
            <asp:BoundField DataField="OItemID" Visible="false" />
            <asp:BoundField DataField="DocNumber" HeaderText="DocNumber" SortExpression="DocNumber" />    
            <asp:BoundField DataField="ItemLNum" HeaderText="LineNum" SortExpression="ItemLNum" />    
            <asp:BoundField DataField="TotalLine" HeaderText="TotalLine" SortExpression="TotalLine" ItemStyle-HorizontalAlign="right" DataFormatString="{0:$0,0}" HtmlEncode="false"/>            
            <asp:BoundField DataField="Responsibility" HeaderText="Responsibility" SortExpression="Responsibility" />
            <asp:BoundField DataField="PrevOrgDesc" HeaderText="Previous Organization" SortExpression="PrevOrgDesc" />     
            <asp:BoundField DataField="PrevUserName" HeaderText="Previous Reviewer" SortExpression="PrevUserName" /> 
            <asp:BoundField DataField="RerouteOrgDesc" HeaderText="Suggested Organization" SortExpression="RerouteOrgDesc" />     
            <asp:BoundField DataField="RerouteUserName" HeaderText="Suggested Reviewer" SortExpression="RerouteUserName" />              
            <asp:TemplateField HeaderText="Confirm Reroute" ItemStyle-HorizontalAlign="Center" ItemStyle-VerticalAlign="Middle" ItemStyle-Width="60">
                <ItemTemplate>
                    <input type="checkbox" id="chkVerify" runat="server" />
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Modify Reroute" ItemStyle-HorizontalAlign="Center" ItemStyle-VerticalAlign="Middle" ItemStyle-Width="60">
                <ItemTemplate>
                    <img src="~/images/ICON_reassign.gif" id="btnAssign" runat="server" alt="Reroute Item" class="icAssign"/>
                </ItemTemplate>
            </asp:TemplateField>            
        </Columns>
        </asp:GridView>
    </div>
    </div>
</asp:Content>