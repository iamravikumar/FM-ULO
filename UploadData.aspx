<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/ULO/ULOMenu.master" AutoEventWireup="true" CodeFile="UploadData.aspx.cs" Inherits="GSA.OpenItems.Web.UploadData" %>
<%@ Register TagPrefix="uc" TagName="getdate_1" Src="~/Controls/getdate.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="title" runat="Server">
    Open Items - Upload Data
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="head" runat="Server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="contentTitle" runat="Server">
    Upload Data
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="content" runat="Server">
    <div class="section">

        <table width="90%">
            <tr>
                <td align="center">
                    <table width="100%" border="0" cellpadding="2" cellspacing="2">
                        <tr align="left">
                            <td style="width: 20%" class="regBldBlueText">Data Source: </td>
                            <td style="width: 60%">
                                <asp:DropDownList ID="ddlDataSource" Width="100%" runat="server" /></td>
                            <td></td>
                        </tr>
                        <tr align="left">
                            <td class="regBldBlueText">Open Items Type: </td>
                            <td>
                                <asp:DropDownList ID="ddlItemType" runat="server" Width="100%" AutoPostBack="true" /></td>
                            <td class="regBldBlueText"></td>
                        </tr>

                        <tr valign="top" id="tr_date_of_report" runat="server">
                            <td class="regBldBlueText">Date of Report:</td>
                            <td>
                                <uc:getdate_1 id="dtReportDate" runat="server" />
                            </td>
                            <td></td>
                        </tr>

                        <tr align="left">
                            <td class="regBldBlueText">Review Name: </td>
                            <td>
                                <asp:TextBox ID="txtReviewName" Width="100%" runat="server"></asp:TextBox></td>
                            <td></td>
                        </tr>
                        <tr align="left">
                            <td class="regBldBlueText">Due Date: </td>
                            <td>
                                <uc:getdate_1 id="dtDue" runat="server" />
                            </td>
                            <td></td>
                        </tr>
                        <tr align="left" style="height: 170px">
                            <td valign="bottom" colspan="3" class="regBldBlueText"><u>For Feedback file from the Central Office</u></td>
                        </tr>
                        <tr align="left">
                            <td class="regBldBlueText">Load to review: </td>
                            <td colspan="2">
                                <asp:DropDownList ID="ddlPrevLoad" Width="100%" runat="server" /></td>
                        </tr>
                        <tr align="left">
                            <td class="regBldBlueText">Review Round: </td>
                            <td colspan="2">
                                <asp:DropDownList ID="ddlRound" Width="60px" runat="server" /></td>
                        </tr>
                    </table>

                    <br />

                    <table>
                        <tr>
                            <td align="center">
                                <asp:FileUpload ID="ctrlFileUpload" runat="server" Width="400px" />
                            </td>
                        </tr>
                        <tr>
                            <td align="center">
                                <br />
                                <asp:Button ID="btnUpload" runat="server" Text="Upload File" CssClass="button" /><p />
                            </td>
                        </tr>
                        <tr>
                            <td align="center">
                                <!-- #include virtual="include/ErrorLabel.inc" -->
                            </td>
                        </tr>
                    </table>
                    <br />
                    <table width="60%">
                        <tr align="left">
                            <td>
                                <asp:Label ID="lblMsg" runat="server" CssClass="error" />
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>
        </table>

    </div>
</asp:Content>
