<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Popups/PopupEnlarged.master" AutoEventWireup="true" CodeFile="AllowanceDistribution.aspx.cs" Inherits="GSA.OpenItems.Web.AllowanceDistribution" %>

<asp:Content ID="Content1" ContentPlaceHolderID="title" runat="Server">
    Fund Allowance Distribution
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="head" runat="Server">
    <link href="../css/fund_status.css" type="text/css" rel="stylesheet" />
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="contentTitle" runat="Server">
    Allowance Distribution
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="content" runat="Server">
    <div class="section">

        <table width="100%" cellspacing="5">
            <tr>
                <td>
                    <asp:Label ID="lblMain" runat="server" CssClass="lrgBldGrayText" /></td>
            </tr>
            <tr>
                <td>
                    <asp:Label ID="lblAllText" runat="server" Text="Allowance Amount:" CssClass="lrgBldGrayText" />&nbsp;&nbsp;            
                <asp:Label ID="lblAllowance" runat="server" CssClass="lrgBldText" />
                </td>
            </tr>
            <tr>
                <td>
                    <asp:Label ID="lblError" runat="server" Visible="false" Text="" CssClass="errorsum" />
                    <hr />
                    <asp:Label ID="lblMessage" runat="server" Text="" CssClass="regBldBlueText" />
                </td>
            </tr>
            <tr>
                <td align="center">
                    <table id="tblData" runat="server" width="420px" bgcolor="9999CC" cellspacing="1" cellpadding="0" onmouseover="mouse('#DDDDDD')" onmouseout="mouse('')">
                        <tr class="tableCaption">
                            <td>Organization</td>
                            <td>Percent</td>
                            <td>Amount</td>
                        </tr>
                    </table>
                </td>
            </tr>
            <tr>
                <td>
                    <table width="330px" align="center" id="tblButtons" runat="server">
                        <tr valign="bottom" style="height: 35px">
                            <td align="left">
                                <input type="button" class="button" value="Cancel" title="Cancel" onclick="self.close();" />
                            </td>
                            <td align="right">
                                <input type="button" onclick="send_rezult(0);" style="width: 120px" class="button" value="Apply for one month" title="Recalculate Chart" />
                            </td>
                            <td align="right">
                                <input type="button" onclick="send_rezult(1);" style="width: 120px" class="button" value="Apply to end of year" title="Recalculate Chart" />
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>
        </table>
    </div>
</asp:Content>
