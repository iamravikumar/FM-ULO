<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/ULO/ULOMenu.master" AutoEventWireup="true" CodeFile="Allowance.aspx.cs" Inherits="GSA.OpenItems.Web.Allowance" %>


<asp:Content ID="Content1" ContentPlaceHolderID="title" Runat="Server">
    <!--Originally had BA61 Fund Status as Application Title -->
    Fund Allowance
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="head" Runat="Server">
    <link href="../css/fund_status.css" type="text/css" rel="stylesheet" />
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="contentTitle" Runat="Server">
    NCR Fund Allowance
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="content" Runat="Server">
    <div class="section">
                        <table width ="100%" id="tblLabels" runat="server" >        
                            <tr>
                                <td class="regBldText" style="width:80px">Fiscal Year:</td>
                                <td><asp:DropDownList ID="ddlFiscalYear" runat="server" AutoPostBack="true" Width="120px"/></td> 
                                <td align="right">
                                    <input type="submit"  value="Export to Excel" onclick="return data_to_excel();" title="Export to Excel" class="button2" />
                                </td>
                            </tr>
                        </table>
                        <hr />
                        <asp:Label ID="lblMessage" runat="server" Text="" CssClass="regBldBlueText"  /><br />
                        <!-- #include virtual="../include/ErrorLabel.inc" -->
                        <asp:Label ID="lblChartMsg" runat="server" CssClass="regBldGreyText" />
                        <div id="divChart">
                            <table id="tblChart" runat="server" width="100%" class="reportTable" onmouseover="mouse('#DDDDDD')" onmouseout="mouse('')" >
                                <tbody>
                                </tbody>
                            </table>
                        </div>
                        <input runat="server" id="Result" style="display:none" />
                        <table width="100%" id="tblButtons" runat="server" >
                            <tr align="center" >
                                <td align="center" style="padding:8px">                        
                                    <input type="button" id="btnCancel" value="Cancel" class="button2" disabled="disabled" onclick="location.href=location.href;" />
                                    <span style="width:20px">&nbsp;</span>
                                    <asp:button id="btnSave" runat="server" CssClass="button2" Enabled="false" Text="Save Changes" onclick="btnSave_ServerClick" />                 
                                    <span style="width:20px">&nbsp;</span>
                                    <input type="button" id="btnAddNew" value="Add Allowance" class="button2" onclick="return add_allowance();" />
                                </td>
                            </tr>        
                        </table>
                    </div> 
</asp:Content>