<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Popups/PopupEnlarged.master" AutoEventWireup="true" Inherits="GSA.OpenItems.Web.FundsEntryData" Codebehind="FundsEntryData.aspx.cs" %>
<%@ Register Src="../Controls/multigrid.ascx" TagName="multigrid" TagPrefix="uc2" %>
<asp:Content ID="Content1" ContentPlaceHolderID="title" runat="Server">
    Fund Status Report Data
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="head" runat="Server">
    <link href="../css/fund_status.css" type="text/css" rel="stylesheet" />
    <!--#include virtual="../include/HTTPService.js" -->
    <!--#include virtual="../include/GetNumbers.js" -->
    <script type="text/javascript">
        document.getElementById("lblReportGroupCode").style.display = "none";
    </script>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="contentTitle" runat="Server">
    <asp:Label ID="lblTitle" runat="server" />
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="content" runat="Server">
    <div class="section">

        <table style="width: 100%" cellspacing="5">
            <tr>
                <td>
                    <asp:Label ID="lblOrganization" runat="server" CssClass="lrgBldGrayText" />
                    <input type="hidden" id="txtReloadReport" runat="server" />
                </td>
                <td>
                    <asp:Label ID="lblMonth" runat="server" CssClass="lrgBldGrayText" />
                    <input type="hidden" id="txtBookMonth" runat="server" />
                </td>
                <td>
                    <label class="lrgBldGrayText">Fiscal Year:</label>&nbsp;&nbsp;
                <asp:Label ID="lblFiscalYear" runat="server" CssClass="lrgBldGrayText" />
                </td>
                <td rowspan="2" align="right">
                    <input type="submit" value="Export to Excel" onclick="return data_to_excel();" title="Export to Excel" class="button2" />
                </td>
            </tr>
            <tr>
                <td>
                    <asp:Label ID="lblReportGroup" runat="server" CssClass="lrgBldGrayText" />
                </td>
                <td>
                    <asp:Label ID="lblReportGroupCode" runat="server" CssClass="lrgBldGrayText" Visible="true" />
                </td>
                <td>
                    <label class="lrgBldGrayText">Amount:</label>&nbsp;&nbsp;            
                <asp:Label ID="lblAmount" runat="server" CssClass="lrgBldText" />
                </td>
            </tr>
            <tr>
                <td colspan="4">
                    <asp:Label ID="lblError" runat="server" Visible="false" Text="" CssClass="errorsum" />
                    <hr />
                    <asp:Label ID="lblMessage" runat="server" Text="" CssClass="regBldBlueText" />
                </td>
            </tr>
            <tr>
                <td colspan="4">
                    <table width="100%" id="tblData" style="text-align: center" runat="server" border="1" onmouseover="mouse('#DDDDDD')" onmouseout="mouse('')">
                        <tr class="reportHeaderBlue">
                            <td style="width: 10%" colspan="2">Edit</td>
                            <td style="width: 15%">DocNumber</td>
                            <td style="width: 12%">$ Amount</td>
                            <td style="width: 63%">Explanation</td>
                        </tr>
                    </table>
                </td>
            </tr>
            <tr>
                <td colspan="4" align="right" valign="middle" style="height: 40px">
                    <input type="button" id="btnClose" class="button" value="Close" title="Close" onclick="close_window();" />
                </td>
            </tr>
        </table>
    </div>
</asp:Content>

