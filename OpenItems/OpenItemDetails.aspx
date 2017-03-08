<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/ULO/ULOMenu.master" AutoEventWireup="true" CodeFile="OpenItemDetails.aspx.cs" Inherits="GSA.OpenItems.Web.OpenItemDetails" %>
<%@ Register TagPrefix="uc" TagName="Menu" Src="~/Controls/Menu.ascx" %>
<%@ Register TagPrefix="uc" TagName="Contacts" Src="~/Controls/Contacts.ascx" %>
<%@ Register TagPrefix="uc" TagName="GetDate" Src="~/Controls/GetDate.ascx" %>
<%@ Register TagPrefix="uc" TagName="Attachments" Src="~/Controls/Attachments.ascx" %>
<asp:Content ID="Content1" ContentPlaceHolderID="title" Runat="Server">
    Open Items Details
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="head" Runat="Server">
    <!--#include virtual="../include/HTTPService.js" -->
    <!--#include virtual="../include/GridLines.inc" -->
    <!--#include virtual="include/OpenItem.js" -->
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="contentTitle" Runat="Server">
    Open Item Details
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="content" Runat="Server">
     <!-- #include virtual="../include/ErrorLabel.inc" -->
    <asp:regularexpressionvalidator id="revUDO" runat="server" CssClass="errorsum" ControlToValidate="txtUDO"
				Display="Dynamic" ErrorMessage="<br/>The UDO value is not valid. Please verify that you have entered the correct money value." 
				EnableClientScript="true" ValidationExpression="[$]?(\d*([\,]{1}\d{0,3})*)*([\.]\d{1,2})?">
	</asp:regularexpressionvalidator>
	<asp:regularexpressionvalidator id="revDO" runat="server" CssClass="errorsum" ControlToValidate="txtDO"
				Display="Dynamic" ErrorMessage="<br/>The DO value is not valid. Please verify that you have entered the correct money value." 
				EnableClientScript="true" ValidationExpression="[$]?(\d*([\,]{1}\d{0,3})*)*([\.]\d{1,2})?">
	</asp:regularexpressionvalidator>	
	
    <input type="hidden" id="txtItemID" runat="server" />
    <input type="hidden" id="txtItemStatusCode" runat="server" />
    <input type="hidden" id="txtReviewerUserID" runat="server" />    
    <input type="hidden" id="txtReassignTargetPage" runat="server" />
    <input type="hidden" id="txtReviewerEmail" runat="server" />  
    
    <div class="sectionSubTitle">    
    <table width="100%" border="0" cellspacing="5" cellpadding="0">
        <tr>
            <td style="width:230px"><span class="regBldBlueText">Reviewer: </span>
                <asp:Label ID="lblReviewer" runat="server" CssClass="regBldGreyText"/>&nbsp;&nbsp;
                <input type="image" id="btnReviewer0" name="btnReviewer0" src="../images/ICON_reassign.gif" runat="server" 
                    alt="Reassign Reviewer for Open Item" class="icAssign" visible="false" />
            </td>
            <td style="width:400px"><span class="regBldBlueText">DocNumber: </span><asp:Label ID="lblDocNumber" runat="server" CssClass="regBldGreyText" /></td>
            <td style="width:200px"><span class="regBldBlueText">Organization: </span><asp:Label ID="lblOrganization" runat="server" CssClass="regBldGreyText" /></td>
        </tr>
        <tr>
            <td><span class="regBldBlueText">Status: </span><asp:Label ID="lblStatus" runat="server" CssClass="regBldGreyText"/></td>
            <td><span class="regBldBlueText">Justification Required: </span><asp:Label ID="lblJustfReq" runat="server" CssClass="regBldRedText"/></td>
            <td><span class="regBldBlueText">OrgCode: </span><asp:Label ID="lblOrgCode" runat="server" CssClass="regBldGreyText" /></td>
        </tr>
        <tr>
            <td><span class="regBldBlueText">Valid: </span><asp:Label ID="lblValid" runat="server" CssClass="regBldRedText" /></td>
            <td><span class="regBldBlueText">Justification: </span><asp:Label ID="lblJustification" runat="server" CssClass="regBldGreyText" /></td>
            <td><span class="regBldBlueText">Due Date: </span><asp:Label ID="lblDueDate" runat="server" CssClass="regBldGreyText" /></td>
        </tr>        
    </table>
    </div>
    
    <br />
    
    <asp:Repeater ID="rpFeedback" runat="server" Visible="true" >            
        <ItemTemplate >
            <table width="100%" class="oddTD" style="border:solid 1px gray;" id="tblFeedback" runat="server">
                <tr><td colspan="5" class="hrBlueHeader">
                    <asp:Label ID="lblFeedback"  runat="server" />
                </td></tr>
                <tr valign="top" >
                    <td class="regBldText" width="100px">CO Comments: </td>
                    <td colspan="4" ><asp:Label ID="lblFdbCOComment" class="regBldText"  runat="server" /></td>
                </tr>
                <tr valign="top" >
                    <td class="regBldBlueText" width="100px">Valid: <span style="color: red;font-size: 14pt">*</span></td>
                    <td colspan="4">
                        <asp:DropDownList ID="ddlFdbValid" runat="server" Visible="false" />
                        <asp:Label ID="lblFdbValid" runat="server" Visible="false" />    
                    </td>
                </tr>
                <tr valign="top">
                    <td class="regBldBlueText">Response: <span style="color: red;font-size: 14pt">*</span></td><td colspan="3">
                    
                        <asp:TextBox ID="txtFdbResponse" runat="server" TextMode="MultiLine" Rows="2" Width="600" Visible="false" MaxLength="500" />
                        <asp:Label ID="lblFdbResponse" runat="server" Visible="false" />    
                    </td>
                    <td rowspan="2" valign="bottom" align="center" >
                        
                        <input type="button" id="btnSaveFdb" runat="server" class="button" value="save response" visible="false" />
                    </td>
                </tr>
                <tr id="trDO_UDO" runat="server">
                    <td><asp:Label ID="lblFdbUDOLabel" runat="server" Text="UDO Should Be:" CssClass="regBldBlueText" /></td>
                    <td><asp:TextBox ID="txtFdbUDO" runat="server" /></td>
                    <td><asp:Label ID="lblFdbDOLabel" runat="server" Text="DO Should Be:" CssClass="regBldBlueText" /></td>
                    <td><asp:TextBox ID="txtFdbDO" runat="server" /></td>                    
                    <td></td>
                </tr>
            </table>      
            <asp:RequiredFieldValidator ID="rfvValid" runat="server" CssClass="errorsum" ControlToValidate="ddlFdbValid"
                        Display="dynamic" ErrorMessage="<br/>Please select validation value. This field is required." 
                        EnableClientScript="true" />
            <asp:RequiredFieldValidator ID="rfvResponse" runat="server" CssClass="errorsum" ControlToValidate="txtFdbResponse"
                        Display="dynamic" ErrorMessage="<br/>Please enter your response. This field is required." 
                        EnableClientScript="true" />
            <asp:regularexpressionvalidator id="revFdbUDO" runat="server" CssClass="errorsum" ControlToValidate="txtFdbUDO"
				        Display="Dynamic" ErrorMessage="<br/>The UDO value is not valid. Please verify that you have entered the correct money value." 
				        EnableClientScript="true" >
	        </asp:regularexpressionvalidator>
	        <asp:regularexpressionvalidator id="revFdbDO" runat="server" CssClass="errorsum" ControlToValidate="txtFdbDO"
				        Display="Dynamic" ErrorMessage="<br/>The DO value is not valid. Please verify that you have entered the correct money value." 
				        EnableClientScript="true" >
	        </asp:regularexpressionvalidator>      	         
        </ItemTemplate>
        <FooterTemplate ><hr class="thickHR" /></FooterTemplate>
    </asp:Repeater>    
    
    <table  width="100%" border="0" cellspacing="0" cellpadding="0" runat="server" id="tblItemInfo" >
        <tr >
            <td  style="padding-left:10px;">
            <span class="regBldBlueText">Additional Information:</span>
            </td>
         </tr>
         <tr  >
            <td style="padding-left:10px; padding-right:5px; padding-bottom:5px;">
                <asp:TextBox runat="server" TextMode="MultiLine" Width="100%" MaxLength="300" ReadOnly="true" ID="txtItemInfo" CssClass="regText" ></asp:TextBox>
            </td>
        </tr>
    </table>
    
    <table width="100%" border="0" cellspacing="0" cellpadding="0">
   
        <tr class="oddTD">
            <td><span class="regBldBlueText">Award Number: </span><asp:Label ID="lblAwardNum" runat="server" CssClass="regBldGreyText" /></td>
            <td><span class="regBldBlueText">Total Line: </span><asp:Label ID="lblTotalLine" runat="server" CssClass="regBldGreyText" /></td>
            <td><span class="regBldBlueText">Title Field: </span><asp:Label ID="lblTitleField" runat="server" CssClass="regBldGreyText" /></td>
            <td><span class="regBldBlueText">Review Round: </span><asp:Label ID="lblFirstReview" runat="server" CssClass="regText" /></td>
        </tr>
        <tr class="evenTD">
            <td style="height: 35px"><span class="regBldBlueText">Earliest: </span><asp:Label ID="lblEarliest" runat="server" CssClass="regText" /></td>
            <td style="height: 35px"><span class="regBldBlueText">Latest: </span><asp:Label ID="lblLatest" runat="server" CssClass="regText" /></td>
            <td colspan="2" style="height: 35px"><span class="regBldBlueText">AcctPeriod: </span><asp:Label ID="lblAcctPeriod" runat="server" CssClass="regText" /></td>
        </tr>
        <tr class="oddTD">
            <td><span class="regBldBlueText">Type Of Balance: </span><asp:Label ID="lblTypeOfBalance" runat="server" CssClass="regText" /></td>
            <td><span class="regBldBlueText">Category: </span><asp:Label ID="lblCategory" runat="server" CssClass="regText" /></td>
            <td colspan="2"><span class="regBldBlueText">Estimated Accrual? </span><asp:Label ID="lblEstAccrual" runat="server" CssClass="regText" /></td>
        </tr>
        <tr class="evenTD">
            <td style="height: 55px"><span class="regBldBlueText">Last Paid: </span><asp:Label ID="lblLastPaid" runat="server" CssClass="regText" /></td>
            <td style="height: 55px"><span class="regBldBlueText">Expiration Date: </span><asp:Label ID="lblExpDate" runat="server" CssClass="regText" /></td>
            <td style="height: 55px" ><span class="regBldBlueText">Expected Completion Date: </span><span class="regBldRedText" style="font-size: 14pt">*</span></td>
            <td style="height: 55px"><uc:GetDate id="dtCompletionDate" runat="server" OnChange="onchange_date();" /></td>                    
        </tr>
        <tr class="oddTD">
            <td><span class="regBldBlueText">UDO: </span><asp:Label ID="lblUDO" runat="server" CssClass="regText" /></td>
            <td><span class="regBldBlueText">UDO Should Be: </span><span class="regBldRedText" style="font-size: 14pt">*</span>
                <asp:TextBox ID="txtUDO" runat="server" CssClass="regText" Width="80px" MaxLength="15"/>                    
            </td>
            <td><span class="regBldBlueText">DO: </span><asp:Label ID="lblDO" runat="server" CssClass="regText" /></td>
            <td><span class="regBldBlueText">DO Should Be: </span><span class="regBldRedText" style="font-size: 12pt">*</span>
                <asp:TextBox ID="txtDO" runat="server" CssClass="regText" Width="80px" MaxLength="15"/></td>
        </tr>
        <tr class="evenTD">
            <td><span class="regBldBlueText">ACCR: </span><asp:Label ID="lblACCR" runat="server" CssClass="regText" /></td>
            <td><span class="regBldBlueText">PENDPYMT: </span><asp:Label ID="lblPENDPYMT" runat="server" CssClass="regText" /></td>
            <td><span class="regBldBlueText">PYMTS_CONF: </span><asp:Label ID="lblPYMTS_CONF" runat="server" CssClass="regText" /></td>
            <td><span class="regBldBlueText">HOLDBACKS: </span><asp:Label ID="lblHOLDBACKS" runat="server" CssClass="regText" /></td>
        </tr>        
    </table>
        
    <table style="font-size: 12pt" >
        <tr>
            <td valign="top" style="width:50%" class="colorBorder"><uc:Contacts ID="ctrlContacts" runat="server" /></td>
            <td valign="top" style="width:50%" class="colorBorder"><uc:Attachments ID="ctrlAttachments" runat="server" /></td>
        </tr>
        <tr>
            <td colspan="2" Class="regBldRedText"> 
                <span style="font-size: 1.1em"><span style="font-size: 14pt">*</span> </span>These fields are required to continue to the next page.
            </td>
        </tr>
    </table>    
    <br />
    <table>
        <tr><td style="padding-left:10px;">
            <span class="regBldBlueText" style="vertical-align:top;">For general OpenItem comments only.<br />
            Validation related comments must be recorded in Validation Comments field on LineNum Detail screen.</span>
        </td></tr>
        <tr><td style="padding-left:10px;">
            <asp:TextBox ID="txtComment" runat="server" TextMode="MultiLine" Width="600px" CssClass="regText" MaxLength="200" />
        </td></tr>
    </table>
    <hr />
    
    <div class="section">
    <asp:Label ID="lblReassignLines" runat="server" CssClass="regBldGreyText" Text="To reassign reviewer please select Item Line and click here: " />
    <input type="image" id="btnReviewer" name="btnReviewer" src="../images/ICON_reassign.gif" runat="server" 
                    alt="Reassign Reviewer for selected Lines" class="icAssign" onclick="javascript:return reassign_lines();" />
    <p />
    <asp:GridView ID="gvLineNums" runat="server" CellPadding="0" AllowPaging="false" AutoGenerateColumns="false"
                AllowSorting="false" CssClass="" Width="100%" HorizontalAlign="Center" 
				CellSpacing="0" EnableViewState="true" HeaderStyle-CssClass=""
				RowStyle-CssClass="TDeven" AlternatingRowStyle-CssClass="TDodd" >
        <Columns>                            
            <asp:BoundField DataField="OItemID" Visible="false" />
            <asp:TemplateField HeaderText="Select to reassign" ItemStyle-Width="10">
                <ItemTemplate>                
                    <input type="checkbox" id="chk_reassign" runat="server" />
                    <label id="lbl_line_reassign" runat="server" visible="false" />
                </ItemTemplate>
            </asp:TemplateField>
            <asp:BoundField DataField="ItemLNum" HeaderText="Line Num" SortExpression="ItemLNum" />
            <asp:BoundField DataField="ULOOrgCode" HeaderText="OrgCode" SortExpression="ULOOrgCode" />
            <asp:BoundField DataField="ReviewerName" HeaderText="Reviewer" SortExpression="ReviewerName" />
            <asp:BoundField DataField="BA" HeaderText="BA" SortExpression="BA" />
            <asp:BoundField DataField="ProjNum" HeaderText="Project Number" SortExpression="ProjNum" />
            <asp:BoundField DataField="RWA" HeaderText="RWA" SortExpression="RWA" />
            <asp:BoundField DataField="UDO" HeaderText="UDO" SortExpression="UDO" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:$0,0}" HtmlEncode="false"/>
            <asp:BoundField DataField="VENDNAME" HeaderText="Vendor Name" SortExpression="VENDNAME" />     
            <asp:BoundField DataField="Code" HeaderText="Code" SortExpression="Code" />            
            <asp:BoundField DataField="ValidValueDescription" HeaderText="Valid Invalid" SortExpression="Valid" />            
        </Columns>
    </asp:GridView>    
    </div>
    
    <hr class="thickHR" />
    
    <table width="100%">
        <tr align="right">
            <td align="right" >
                <input type="button" id="btnCancel" value="<< Back" title="Back to the Items List" class="button" onclick="javascript:on_cancel();"/>&nbsp;&nbsp;
                <input type="button" id="btnHistory" value="History" title="Open Item History" class="button" onclick="javascript:open_history();"/>&nbsp;&nbsp;
                <asp:Button ID="btnSave" runat="server" Text="Save" ToolTip="Save" CssClass="button"/>
            </td>
        </tr>
    </table>
</asp:Content>

