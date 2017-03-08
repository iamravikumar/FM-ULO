<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/ULO/ULOMenu.master" AutoEventWireup="true" CodeFile="Deobligation.aspx.cs" Inherits="GSA.OpenItems.Web.Deobligation" %>
<%@ Register TagPrefix="uc" TagName="CriteriaFields" Src="~/Controls/CriteriaFields.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="title" Runat="Server">
    Certify Deobligation
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="head" Runat="Server">
     <!--#include virtual="../include/HTTPService.js" -->
    <!--#include virtual="../include/GridLines.inc" -->
    <!--#include virtual="include/Deobligation.js" -->    
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="contentTitle" Runat="Server">
    Invalid Open Items List
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="content" Runat="Server">
          <div class="section">
        
     <uc:CriteriaFields ID="ctrlCriteria" runat="server" />
    
    <!-- #include virtual="../include/ErrorLabel.inc" -->
    <br />
        <asp:Label ID="lblMessages" runat ="server" />

    <asp:GridView ID="gvItems" runat="server" CellPadding="0" AllowPaging="True" AutoGenerateColumns="false"
                AllowSorting="true" CssClass="" Width="100%" HorizontalAlign="Center" 
				CellSpacing="0" EnableViewState="false" HeaderStyle-CssClass=""
				UseAccessibleHeader="true" PagerSettings-Mode="NumericFirstLast" 
				RowStyle-CssClass="TDeven" AlternatingRowStyle-CssClass="TDodd" PageSize="30" PagerStyle-CssClass="Pages" >
        <Columns>                            
            <asp:BoundField DataField="OItemID" Visible="false" />
            <asp:BoundField DataField="DocNumber" HeaderText="DocNumber" SortExpression="DocNumber" />
            <asp:BoundField DataField="ItemLNum" HeaderText="LineN" />
            <asp:BoundField DataField="ULOOrganization" HeaderText="ULO Organization" SortExpression="ULOOrganization" />
            <asp:BoundField DataField="COOrganization" HeaderText="Original Organization" SortExpression="COOrganization" />
            <asp:BoundField DataField="BA" HeaderText="BA" SortExpression="BA" />
            <asp:TemplateField HeaderText="Deobligated" ItemStyle-HorizontalAlign="Center" ItemStyle-VerticalAlign="Middle" >
                <ItemTemplate>                
                    <input type="checkbox" id="chkDeobligate" runat="server" />
                </ItemTemplate>
            </asp:TemplateField>
            <asp:BoundField DataField="cert_date" HeaderText="Certify Date" SortExpression="cert_date" DataFormatString="{0:MMM dd, yyyy}" HtmlEncode="false"/>                 
        </Columns>
        </asp:GridView>

    </div>
</asp:Content>

