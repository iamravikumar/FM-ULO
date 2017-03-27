<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Popups/PopupEnlarged.master" AutoEventWireup="true" Inherits="GSA.OpenItems.Web.OpenItemHistory" Codebehind="OpenItemHistory.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="title" runat="Server">
    Open Item History -
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="head" runat="Server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="contentTitle" runat="Server">
    <asp:Label ID="lblMainTitle" runat="server" Text="Open Item History" />
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="content" runat="Server">
    <div class="section">

        <table width="100%" cellspacing="4">
            <tr>
                <td style="width: 150px">
                    <label class="regBldText">Document Number: </label>
                </td>
                <td>
                    <asp:Label ID="lblDocNumber" runat="server" CssClass="lrgBldText" /></td>
            </tr>
            <tr style="display: none;">
                <td style="width: 150px">
                    <label class="regBldText">Current Reviewer: </label>
                </td>
                <td>
                    <asp:Label ID="lblReviewer" runat="server" CssClass="lrgBldText" /></td>
            </tr>
            <tr>
                <td colspan="2">
                    <asp:Label ID="lblMessage" runat="server" Visible="false" Text="" CssClass="errorsum" Width="400" />
                </td>
            </tr>
        </table>
    </div>

    <asp:DataList ID="dlLoads" runat="server" Width="100%">
        <ItemTemplate>
            <tr>
                <td style="width: 80px">
                    <label class="regBldText">Review Info: </label>
                </td>
                <td>
                    <asp:Label ID="lblLoadInfo" runat="server" CssClass="regBldGreyText" /></td>
            </tr>
            <tr>
                <td colspan="2">
                    <asp:Label ID="lblLoadMsg" runat="server" Text="" CssClass="regBldGreyText" Width="400" />
                </td>
            </tr>
            <tr>
                <td colspan="2">
                    <asp:GridView ID="gvHistory" runat="server" CellPadding="0" AllowPaging="false" AutoGenerateColumns="false"
                        AllowSorting="false" CssClass="" Width="100%" HorizontalAlign="Center"
                        CellSpacing="0" EnableViewState="false" HeaderStyle-CssClass=""
                        UseAccessibleHeader="true" RowStyle-CssClass="TDeven" AlternatingRowStyle-CssClass="TDodd">
                        <Columns>
                            <asp:BoundField DataField="ActionDate" HeaderText="Date" DataFormatString="{0:MMM dd, yyyy  h:mm:ss tt}" HtmlEncode="false" />
                            <asp:BoundField DataField="UpdateUsername" HeaderText="Performed By" />
                            <asp:TemplateField HeaderText="Action">
                                <ItemTemplate>
                                    <asp:Label ID="lblAction" runat="server" />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Details">
                                <ItemTemplate>
                                    <asp:Label ID="lblInfo" runat="server" />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="Comments" HeaderText="Comments" />
                            <asp:BoundField DataField="ULOOrganization" HeaderText="ULO Organization" />
                            <asp:TemplateField HeaderText="Validation">
                                <ItemTemplate>
                                    <asp:Label ID="lblValid" runat="server" />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="StatusDescription" HeaderText="Item Status" />

                        </Columns>
                    </asp:GridView>
                    <br />
                </td>
            </tr>
        </ItemTemplate>
        <SeparatorTemplate>
            <tr>
                <td colspan="2">
                    <hr />
                </td>
            </tr>
        </SeparatorTemplate>
    </asp:DataList>

    <table width="100%">
        <tr>
            <td align="right">
                <input type="button" id="btnClose" value="Close" onclick="javascript: self.close();" class="button" title="Close" />
            </td>
        </tr>
    </table>
</asp:Content>

