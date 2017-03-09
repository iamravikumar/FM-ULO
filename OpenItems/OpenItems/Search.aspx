<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/ULO/ULOMenu.master" AutoEventWireup="true" Inherits="GSA.OpenItems.Web.Search" Codebehind="Search.aspx.cs" %>
<%@ Register TagPrefix="uc" TagName="Menu" Src="~/Controls/Menu.ascx" %>
<%@ Register TagPrefix="uc" TagName="CriteriaFields" Src="~/Controls/CriteriaFields.ascx" %>
<asp:Content ID="Content1" ContentPlaceHolderID="title" runat="Server">
    Open Items Search
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="head" runat="Server">
    <!--#include virtual="../include/HTTPService.js" -->
    <!--#include virtual="../include/GridLines.inc" -->
    <!--#include virtual="include/Search.js" -->
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="contentTitle" runat="Server">
    Search Open Items
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="content" runat="Server">
    <div class="section">
        <uc:criteriafields id="ctrlCriteria" runat="server" />
        <!-- #include virtual="../include/ErrorLabel.inc" -->
        <br />
        <asp:Label ID="lblMessages" runat="server" />
        <input type="hidden" id="txtReassignTargetPage" runat="server" />
        <input type="hidden" id="txtAllSelected" runat="server" />
        <table width="100%">
            <tr>
                <td align="left" style="padding-bottom: 5px;">
                    <label id="lblAssign" runat="server" visible="false" class="regBldGreyText">
                        Please select Items for group assignment and click here:
                    </label>
                    <img src="~/images/ICON_reassign.gif" id="btnAssign" runat="server" visible="false"
                        alt="Reassign or Reroute Items" class="icAssign" />
                </td>
                <td align="right" id="excelTD" visible="false" runat="server">
                    <img alt="Convert to Excel" style="cursor: hand" runat="server" id="excelImage"
                        src="~/images/Excel-Icon.jpg" />
                </td>
            </tr>
        </table>
        <asp:GridView ID="gvItems" runat="server" CellPadding="0" AllowPaging="True" AutoGenerateColumns="false"
            AllowSorting="true" CssClass="" Width="100%" HorizontalAlign="Center" CellSpacing="0"
            EnableViewState="false" HeaderStyle-CssClass="" UseAccessibleHeader="true" PagerSettings-Mode="NumericFirstLast"
            RowStyle-CssClass="TDeven" AlternatingRowStyle-CssClass="TDodd" PageSize="20"
            PagerStyle-CssClass="Pages" OnDataBound="gvItems_DataBound">
            <Columns>
                <asp:BoundField DataField="OItemID" Visible="false" />
                <asp:TemplateField ItemStyle-HorizontalAlign="Center" ItemStyle-VerticalAlign="Middle"
                    ItemStyle-Width="40" Visible="false">
                    <HeaderTemplate>
                        <input type="button" class="button" id="btnSelect" value="Assign" title="click to select all Items"
                            onclick="javascript: select_all();" />
                    </HeaderTemplate>
                    <ItemTemplate>
                        <input type="checkbox" id="chkAssign" runat="server" />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField DataField="LoadDate" HeaderText="LoadDate" SortExpression="LoadDate"
                    DataFormatString="{0:MMM dd, yyyy}" HtmlEncode="false" />
                <asp:BoundField DataField="DocNumber" HeaderText="DocNumber" SortExpression="DocNumber" />
                <asp:BoundField DataField="ItemLNum" HeaderText="Line" SortExpression="ItemLNum" />
                <asp:BoundField DataField="BA" HeaderText="BA" SortExpression="BA" />
                <asp:BoundField DataField="ProjNum" HeaderText="ProjectNumber" SortExpression="ProjNum" />
                <asp:BoundField DataField="AwardNumber" HeaderText="AwardNumber" SortExpression="AwardNumber" />
                <asp:BoundField DataField="ContractingOfficer" HeaderText="Contr.Officer by CO" SortExpression="ContractingOfficer" />
                <asp:BoundField DataField="Organization" HeaderText="Organization" SortExpression="Organization" />
                <asp:BoundField DataField="ReviewerName" HeaderText="Reviewer" SortExpression="ReviewerUserID" />
                <asp:BoundField DataField="StatusDescription" HeaderText="Status" SortExpression="Status" />
                <asp:BoundField DataField="ValidValueDescription" HeaderText="Valid Invalid" SortExpression="Valid"
                    ItemStyle-Width="60" />
            </Columns>
        </asp:GridView>
        <div style="width: 100%; overflow-x: scroll; position: relative" id="divValSearch" runat="server" visible="false">
            <asp:GridView ID="gvTest" runat="server" AllowSorting="true" AllowPaging="true" PageSize="20" AutoGenerateColumns="true"
                HeaderStyle-CssClass=""
                RowStyle-CssClass="valHistTDeven" AlternatingRowStyle-CssClass="valHistTDodd" OnRowCreated="gvTest_RowCreated"
                PagerStyle-CssClass="Pages" OnSorting="gvTest_Sorting" OnPageIndexChanging="gvTest_PageIndexChanging" OnDataBound="gvTest_DataBound">
            </asp:GridView>
        </div>
    </div>

</asp:Content>

