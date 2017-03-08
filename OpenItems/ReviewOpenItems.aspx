<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/ULO/ULOMenu.master" AutoEventWireup="true" CodeFile="ReviewOpenItems.aspx.cs" Inherits="GSA.OpenItems.Web.ReviewOpenItems" %>

<%@ Register TagPrefix="uc" TagName="Menu" Src="~/Controls/Menu.ascx" %>
<%@ Register TagPrefix="uc" TagName="CriteriaFields" Src="~/Controls/CriteriaFields.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="title" runat="Server">
    Open Items List Review
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="head" runat="Server">
    <!--#include virtual="../include/HTTPService.js" -->
    <!--#include virtual="../include/GridLines.inc" -->
    <!--#include virtual="include/ReviewItems.js" -->
    <style type="text/css">
        .gvclass table th {
            text-align: left;
        }
    </style>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="contentTitle" runat="Server">
    <asp:Label ID="lblHeaderLabel" runat="server" Text="Open Items List" />
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="content" runat="Server">
    <div class="section">

        <uc:CriteriaFields ID="ctrlCriteria" runat="server" />

        <!-- #include virtual="../include/ErrorLabel.inc" -->
        <br />
        <input type="hidden" id="txtReassignTargetPage" runat="server" />

        <table width="100%">
            <tr>
                <td align="left">
                    <asp:Label ID="lblMessages" runat="server" /></td>
            </tr>
            <tr>
                <td align="left" style="padding-bottom: 5px;">
                    <label id="lblEmail" runat="server" visible="false" class="regBldGreyText">To send automated group email to notify OI reviewers about their assignments please click here </label>
                    <asp:Button ID="btnEmail" runat="server" Visible="false" Text="Send Email"
                        CssClass="button" ToolTip="Send Email Notification to Reviewers of OI Assignments" />
                </td>
            </tr>
        </table>

        <div class="gvclass">
            <asp:GridView ID="gvItems" runat="server" CellPadding="0" AllowPaging="True" AutoGenerateColumns="false"
                AllowSorting="true" Width="100%"
                CellSpacing="0" EnableViewState="false"
                UseAccessibleHeader="true" PagerSettings-Mode="NumericFirstLast" FooterStyle-BorderStyle="solid" FooterStyle-BorderColor="white"
                RowStyle-CssClass="TDeven" AlternatingRowStyle-CssClass="TDodd" PageSize="30" PagerStyle-CssClass="Pages">
                <Columns>
                    <asp:BoundField DataField="OItemID" Visible="False" />

                    <asp:BoundField DataField="DocNumber" HeaderText="Doc Number" SortExpression="DocNumber">
                        <ItemStyle HorizontalAlign="Left" BorderColor="Silver" BorderStyle="Solid" BorderWidth="1px" />
                    </asp:BoundField>

                    <asp:BoundField DataField="LinesCnt" HeaderText="# Of Lines" SortExpression="LinesCnt">
                        <ItemStyle Width="20px" HorizontalAlign="Center" BorderColor="Silver" BorderStyle="Solid" BorderWidth="1px" />
                    </asp:BoundField>

                    <asp:BoundField DataField="Title" HeaderText="Title" SortExpression="Title">
                        <ItemStyle HorizontalAlign="Left" BorderColor="Silver" BorderStyle="Solid" BorderWidth="1px" />
                    </asp:BoundField>

                    <asp:BoundField DataField="TotalLine" HeaderText="Total Line" SortExpression="TotalLine" DataFormatString="{0:$0,0}" HtmlEncode="False">
                        <ItemStyle HorizontalAlign="Right" BorderColor="Silver" BorderStyle="Solid" BorderWidth="1px" />
                    </asp:BoundField>

                    <asp:BoundField DataField="AwardNumber" HeaderText="Award Number" SortExpression="AwardNumber">
                        <ItemStyle Width="60px" BorderColor="Silver" BorderStyle="Solid" BorderWidth="1px" />
                    </asp:BoundField>

                    <asp:BoundField DataField="ContractingOfficer" HeaderText="Contr.Officer by CO" SortExpression="ContractingOfficer">
                        <ItemStyle Width="100px" BorderColor="Silver" BorderStyle="Solid" BorderWidth="1px" />
                    </asp:BoundField>

                    <asp:BoundField DataField="ResponsibleOrg" HeaderText="Org." SortExpression="ResponsibleOrg">
                        <ItemStyle BorderColor="Silver" BorderStyle="Solid" BorderWidth="1px" />
                    </asp:BoundField>

                    <asp:BoundField DataField="ReviewerName" HeaderText="Reviewer" SortExpression="ReviewerUserID">
                        <ItemStyle BorderColor="Silver" BorderStyle="Solid" BorderWidth="1px" />
                    </asp:BoundField>

                    <asp:TemplateField HeaderText="Confirm Assignment">
                        <ItemTemplate>
                            <input type="checkbox" id="chkVerify" runat="server" />
                        </ItemTemplate>
                        <ItemStyle HorizontalAlign="Center" VerticalAlign="Middle" Width="60px" BorderColor="Silver" BorderStyle="Solid" BorderWidth="1px" />
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Reassign Reroute">
                        <ItemTemplate>
                            <img src="~/images/ICON_reassign.gif" id="btnAssign" runat="server" alt="Reassign or Reroute Item" class="icAssign" />
                        </ItemTemplate>
                        <ItemStyle HorizontalAlign="Center" VerticalAlign="Middle" Width="60px" BorderColor="Silver" BorderStyle="Solid" BorderWidth="1px" />
                    </asp:TemplateField>

                    <asp:BoundField DataField="StatusDescription" HeaderText="Status" SortExpression="Status">
                        <ItemStyle BorderColor="Silver" BorderStyle="Solid" BorderWidth="1px" />
                    </asp:BoundField>

                    <asp:BoundField DataField="ValidValueDescription" HeaderText="Valid Invalid" SortExpression="Valid">
                        <ItemStyle Width="60px" HorizontalAlign="Center" BorderColor="Silver" BorderStyle="Solid" BorderWidth="1px" />
                    </asp:BoundField>
                </Columns>
                <PagerSettings Mode="NumericFirstLast" />
                <RowStyle BorderColor="Maroon" BorderStyle="Double" BorderWidth="1px" CssClass="TDeven" />
                <PagerStyle CssClass="Pages" />
                <AlternatingRowStyle CssClass="TDodd" />
                <FooterStyle BorderColor="White" BorderStyle="Solid" />
                <HeaderStyle BorderColor="White" CssClass="HeaderStyle" BorderStyle="Solid" BorderWidth="1px" />
            </asp:GridView>
        </div>

    </div>
</asp:Content>

