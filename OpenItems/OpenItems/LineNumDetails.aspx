<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/ULO/ULOMenu.master" AutoEventWireup="true" Inherits="GSA.OpenItems.Web.LineNumDetails" Codebehind="LineNumDetails.aspx.cs" %>
<%@ Register TagPrefix="uc" TagName="attachments" Src="~/Controls/Attachments.ascx" %>
<%@ Register TagPrefix="uc" TagName="contacts" Src="~/Controls/Contacts.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="title" runat="Server">
    Open Items - LineNum Details
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="head" runat="Server">
    <!--#include virtual="../include/HTTPService.js" -->
    <!--#include virtual="include/LineNum.js" -->
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="contentTitle" runat="Server">
    <asp:Label ID="lblHeaderLabel" runat="server" Text="LineNum Details" />
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="content" runat="Server">
    <!-- #include virtual="../include/ErrorLabel.inc" -->
    <input type="hidden" id="txtItemID" runat="server" />
    <input type="hidden" id="txtCodeValidation" runat="server" />
    <input type="hidden" id="txtCodeList" runat="server" />
    <input type="hidden" id="txtCodeDescList" runat="server" />
    <input type="hidden" id="txtDisplayAddOn" runat="server" />
    <input type="hidden" id="txtAddOnDescList" runat="server" />
    <input type="hidden" id="txtReviewerEmail" runat="server" />
    <div class="sectionSubTitle">
        <table width="100%" border="0" cellspacing="5" cellpadding="0">
            <tr>
                <td>
                    <span class="regBldBlueText">Reviewer: </span>
                    <asp:Label ID="lblReviewer" runat="server" CssClass="regBldGreyText" /></td>
                <td>
                    <span class="regBldBlueText">DocNumber: </span>
                    <asp:Label ID="lblDocNumber" runat="server" CssClass="regBldGreyText" /></td>
                <td>
                    <span class="regBldBlueText">Organization: </span>
                    <asp:Label ID="lblOrganization" runat="server" CssClass="regBldGreyText" /></td>
            </tr>
            <tr>
                <td>
                    <span class="regBldBlueText">Status: </span>
                    <asp:Label ID="lblStatus" runat="server" CssClass="regBldGreyText" /></td>
                <td>
                    <span class="regBldBlueText">LineNum: </span>
                    <asp:Label ID="lblLineNum" runat="server" CssClass="regBldGreyText" />
                    <asp:Label ID="lblLineOrgCode" runat="server" CssClass="regBldGreyText" Visible="false" />
                </td>
                <td>
                    <span class="regBldBlueText">OrgCode: </span>
                    <asp:Label ID="lblOrgCode" runat="server" CssClass="regBldGreyText" /></td>
            </tr>
            <tr>
                <td>
                    <span class="regBldBlueText">Valid: </span>
                    <asp:Label ID="lblValid" runat="server" CssClass="regBldRedText" /></td>
                <td>
                    <span class="regBldBlueText">Justification Required: </span>
                    <asp:Label ID="lblJustfReq" runat="server" CssClass="regBldRedText" /></td>
                <td>
                    <span class="regBldBlueText">Due Date: </span>
                    <asp:Label ID="lblDueDate" runat="server" CssClass="regBldGreyText" /></td>
            </tr>
            <tr>
                <td colspan="3">
                    <asp:Label ID="lblDeobligatedInfo" runat="server" CssClass="regBldBlueText" Text="Deobligated on: "
                        Visible="true" /><asp:Label ID="lblDeobligatedDate" runat="server" CssClass="regBldGreyText" /></td>
            </tr>
        </table>
    </div>
    <br />

    <table width="100%" border="0" cellspacing="0" cellpadding="0" runat="server" id="tblLineInfo">
        <tr>
            <td style="padding-left: 10px;">
                <span class="regBldBlueText">Additional Information:</span>
            </td>
        </tr>
        <tr>
            <td style="padding-left: 10px; padding-right: 5px; padding-bottom: 5px;">
                <asp:TextBox runat="server" TextMode="MultiLine" Width="100%" MaxLength="300" ReadOnly="true"
                    ID="txtLineInfo" CssClass="regText"></asp:TextBox>
            </td>
        </tr>
    </table>

    <table width="100%" border="0" cellspacing="0" cellpadding="0">
        <tr class="oddTD">
            <td>
                <span class="regBldBlueText">Award Number: </span>
                <asp:Label ID="lblAwardNum" runat="server" CssClass="regBldGreyText" /></td>
            <td>
                <span class="regBldBlueText">Total Line (OpenItem): </span>
                <asp:Label ID="lblTotalLineItem" runat="server" CssClass="regBldGreyText" /></td>
            <td>
                <span class="regBldBlueText">Total Line (LineNum): </span>
                <asp:Label ID="lblTotalLineLine" runat="server" CssClass="regBldGreyText" /></td>
            <td>
                <span class="regBldBlueText">Title Field: </span>
                <asp:Label ID="lblTitleField" runat="server" CssClass="regBldGreyText" /></td>
        </tr>
        <tr class="evenTD">
            <td>
                <span class="regBldBlueText">Project Number: </span>
                <asp:Label ID="lblProjNum" runat="server" CssClass="regText" /></td>
            <td>
                <span class="regBldBlueText">BA: </span>
                <asp:Label ID="lblBA" runat="server" CssClass="regText" /></td>
            <td>
                <span class="regBldBlueText">AGRE_BLDG: </span>
                <asp:Label ID="lblAGRE_BLDG" runat="server" CssClass="regText" /></td>
            <td>
                <span class="regBldBlueText">RWA: </span>
                <asp:TextBox ID="txtRWA" runat="server" CssClass="regText" MaxLength="7" /></td>
        </tr>
        <tr class="oddTD">
            <td>
                <span class="regBldBlueText">Building Number: </span>
                <asp:Label ID="lblBuilding" runat="server" CssClass="regText" /></td>
            <td>
                <span class="regBldBlueText">Vendor CD: </span>
                <asp:Label ID="lblVendCD" runat="server" CssClass="regText" /></td>
            <td colspan="2">
                <span class="regBldBlueText">Vendor Name: </span>
                <asp:Label ID="lblVendName" runat="server" CssClass="regText" /></td>
        </tr>
        <tr class="evenTD">
            <td>
                <span class="regBldBlueText">Earliest: </span>
                <asp:Label ID="lblEarliest" runat="server" CssClass="regText" /></td>
            <td>
                <span class="regBldBlueText">Latest: </span>
                <asp:Label ID="lblLatest" runat="server" CssClass="regText" /></td>
            <td>
                <span class="regBldBlueText">AcctPeriod: </span>
                <asp:Label ID="lblAcctPeriod" runat="server" CssClass="regText" /></td>
            <td>
                <span class="regBldBlueText">ACTG_PD: </span>
                <asp:Label ID="lblACTG_PD" runat="server" CssClass="regText" /></td>
        </tr>
        <tr class="oddTD">
            <td>
                <span class="regBldBlueText">Last Paid (Open Item): </span>
                <asp:Label ID="lblLastPaid" runat="server" CssClass="regText" /></td>
            <td>
                <span class="regBldBlueText">Expiration Date: </span>
                <asp:Label ID="lblExpDate" runat="server" CssClass="regText" /></td>
            <td colspan="2">
                <span class="regBldBlueText">Expected Completion Date: </span>
                <asp:Label ID="lblCompletionDate" runat="server" CssClass="regText" /></td>
        </tr>
        <tr class="evenTD">
            <td>
                <span class="regBldBlueText">Last Paid (LineNum): </span>
                <asp:Label ID="lblLastPaidLine" runat="server" CssClass="regText" /></td>
            <td>
                <span class="regBldBlueText">BBFY: </span>
                <asp:Label ID="lblBBFY" runat="server" CssClass="regText" /></td>
            <td colspan="2">
                <span class="regBldBlueText">EBFY: </span>
                <asp:Label ID="lblEBFY" runat="server" CssClass="regText" /></td>
        </tr>
        <tr class="oddTD">
            <td>
                <span class="regBldBlueText">UDO (Open Item): </span>
                <asp:Label ID="lblUDO" runat="server" CssClass="regText" /></td>
            <td>
                <span class="regBldBlueText">UDO Should Be: </span><span class="regBldRedText">*</span>
                <asp:Label ID="lblUDOShouldBe" runat="server" CssClass="regText" /></td>
            <td>
                <span class="regBldBlueText">DO (Open Item): </span>
                <asp:Label ID="lblDO" runat="server" CssClass="regText" /></td>
            <td>
                <span class="regBldBlueText">DO Should Be: </span><span class="regBldRedText">*</span>
                <asp:Label ID="lblDOShouldBe" runat="server" CssClass="regText" /></td>
        </tr>
        <tr class="evenTD">
            <td>
                <span class="regBldBlueText">UDO (LineNum): </span>
                <asp:Label ID="lblUDOLine" runat="server" CssClass="regText" /></td>
            <td>
                <span class="regBldBlueText">FC: </span>
                <asp:Label ID="lblFC" runat="server" CssClass="regText" /></td>
            <td>
                <span class="regBldBlueText">OC: </span>
                <asp:Label ID="lblOC" runat="server" CssClass="regText" /></td>
            <td>
                <span class="regBldBlueText">CE: </span>
                <asp:Label ID="lblCE" runat="server" CssClass="regText" /></td>
        </tr>
        <tr class="oddTD">
            <td>
                <span class="regBldBlueText">ACCR (Open Item): </span>
                <asp:Label ID="lblACCR" runat="server" CssClass="regText" /></td>
            <td>
                <span class="regBldBlueText">PENDPYMT (Open Item): </span>
                <asp:Label ID="lblPENDPYMT" runat="server" CssClass="regText" /></td>
            <td>
                <span class="regBldBlueText">PYMTS_CONF (Open Item): </span>
                <asp:Label ID="lblPYMTS_CONF" runat="server" CssClass="regText" /></td>
            <td>
                <span class="regBldBlueText">HOLDBACKS (Open Item): </span>
                <asp:Label ID="lblHOLDBACKS" runat="server" CssClass="regText" /></td>
        </tr>
        <tr class="evenTD">
            <td>
                <span class="regBldBlueText">ACCR (LineNum): </span>
                <asp:Label ID="lblACCRLine" runat="server" CssClass="regText" /></td>
            <td>
                <span class="regBldBlueText">PENDPYMT (LineNum): </span>
                <asp:Label ID="lblPENDPYMTLine" runat="server" CssClass="regText" /></td>
            <td>
                <span class="regBldBlueText">PYMTS_CONF (LineNum): </span>
                <asp:Label ID="lblPYMTS_CONFLine" runat="server" CssClass="regText" /></td>
            <td>
                <span class="regBldBlueText">HOLDBACKS (LineNum): </span>
                <asp:Label ID="lblHOLDBACKSLine" runat="server" CssClass="regText" /></td>
        </tr>
    </table>
    <hr />
    <table>
        <tr>
            <td valign="top" style="width: 50%" class="colorBorder">
                <uc:contacts id="ctrlContacts" runat="server" />
            </td>
            <td valign="top" style="width: 50%" class="colorBorder">
                <uc:attachments id="ctrlAttachments" runat="server" />
            </td>
        </tr>
    </table>
    <hr />
    <div class="section">
        <table width="100%">
            <tr>
                <td colspan="2">
                    <span class="lrgBldText">Current LineNum Decisions:</span></td>
            </tr>
            <tr>
                <td colspan="2" style="height: 50px">
                    <hr />
                    <asp:Label ID="lblMsgValidation" runat="server" CssClass="regBldRedText" Visible="false" />
                </td>
            </tr>
            <tr>
                <td style="padding-left: 10px; width: 70px;">
                    <span class="regBldBlueText">Valid: </span><span class="regBldRedText">*</span></td>
                <td>
                    <asp:DropDownList ID="ddlValid" runat="server" CssClass="regText" /></td>
            </tr>
            <tr>
                <td style="padding-left: 10px; width: 70px;">
                    <span class="regBldBlueText">Validation Comment: </span>
                </td>
                <td>
                    <asp:TextBox ID="txtComment" runat="server" TextMode="MultiLine" Width="700px" CssClass="regText"
                        Rows="3" MaxLength="500" /></td>
            </tr>
            <tr style="height: 4px;">
                <td colspan="2">
                    <hr />
                </td>
            </tr>
            <tr>
                <td style="padding-left: 10px;">
                    <span class="regBldBlueText">Code: </span><span class="regBldRedText">*</span></td>
                <td>
                    <asp:DropDownList ID="ddlCode" runat="server" CssClass="regText" />&nbsp;
                                    <asp:Label ID="lblCode" runat="server" CssClass="regBldGreyText" /></td>
            </tr>
            <tr>
                <td style="padding-left: 10px;">
                    <span class="regBldBlueText">Code Comment: </span>
                </td>
                <td>
                    <asp:TextBox ID="txtCodeComment" runat="server" CssClass="regText" Width="450px"
                        MaxLength="200" /></td>
            </tr>
            <tr style="height: 4px;">
                <td colspan="2">
                    <hr />
                </td>
            </tr>

            <%-- <tr>
                                <td style="padding-left: 10px;">
                                    <span class="regBldBlueText">Justification: </span><span class="regBldRedText">*</span></td>
                                <td>
                                    <asp:DropDownList ID="ddlJustification" runat="server" CssClass="regText" />
                                    <br />
                                    <asp:Label ID="lblJustAddOnDesc" runat="server" CssClass="regBldGreyText" />&nbsp;&nbsp;
                                    <asp:TextBox ID="txtJustAddOn" runat="server" CssClass="regText" MaxLength="50" />
                                </td>
                            </tr> --%>
            <tr>
                <td colspan="2">
                    <span class="regBldBlueText">Justification: </span><span class="regBldRedText">*</span>
                </td>
            </tr>
            <tr>
                <td colspan="2">
                    <table>
                        <tr>
                            <td valign="top" style="width: 60%" class="colorBorder">
                                <asp:RadioButtonList name="testRBL" ID="rblJustification" runat="server" CssClass="regText" Width="100%">
                                </asp:RadioButtonList>
                                <br />

                            </td>
                            <td valign="top" style="width: 40%" class="colorBorder">
                                <asp:Label ID="lblJustificationExplanation" runat="server" CssClass="regBldGreyText"></asp:Label>
                                <br />
                                <asp:Label ID="lblJustAddInfo" runat="server" CssClass="regBldBlueText">Additional Information:</asp:Label>
                                <asp:TextBox runat="server" ID="txtAddJustification" CssClass="regText" MaxLength="50"></asp:TextBox>
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>

            <tr>
                <td style="padding-left: 10px;">
                    <asp:Label ID="lblJustOther" runat="server" Text="Other: " CssClass="regBldBlueText" /></td>
                <td>
                    <asp:TextBox ID="txtJustOther" runat="server" CssClass="regText" Width="450px" MaxLength="100" /></td>
            </tr>
        </table>
    </div>

    <hr class="thickHR" />
    <table width="100%">
        <tr align="right">
            <td align="right">
                <input type="button" id="btnCancel" value="<< Back" title="Back to the Open Item"
                    class="button" runat="server" />&nbsp;&nbsp;
                                <input type="button" id="btnHistory" value="History" title="Open Item History" class="button"
                                    onclick="javascript: open_history();" />&nbsp;&nbsp;
                                <asp:Button ID="btnSave" runat="server" Text="Save" ToolTip="Save" CssClass="button" />
            </td>
        </tr>
    </table>
</asp:Content>

