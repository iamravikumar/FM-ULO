<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/ULO/ULOMenu.master" AutoEventWireup="true" CodeFile="OIBA53Review.aspx.cs" Inherits="GSA.OpenItems.Web.OIBA53Review" %>
<%@ Register Src="~/Controls/dropdowngrid.ascx" TagName="dropdowngrid" TagPrefix="uc1" %>

<%@ Register TagPrefix="uc" TagName="Menu" Src="~/Controls/Menu.ascx" %>
<%@ Register TagPrefix="uc" TagName="Contacts" Src="~/Controls/Contacts.ascx" %>
<%@ Register TagPrefix="uc" TagName="GetDate" Src="~/Controls/GetDate.ascx" %>
<%@ Register TagPrefix="uc" TagName="Attachments" Src="~/Controls/Attachments.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="title" runat="Server">
    BA53 Open Item
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="head" runat="Server">
    <meta http-equiv="pragma" content="no-cache" />
    <!--#include virtual="../include/HTTPService.js" -->
    <!--include virtual="include/LineNum.js" -->
    <!--#include virtual="../include/GetNumbers.js" -->
    <!--#include virtual="../include/GridLines.inc" -->
    <!--#include virtual="include/OpenItem.js" -->
    <%--TODO: No Jscript--%>
    <script type="text/jscript">

        //document.onload= HideCalendarBtn();

        function AlignTextBoxes() {
            //var frm = document.forms[0];
            alert("!");
            //        debugger
            //        for (i=0;i<frm.elements.length;i++)
            //        {
            //            if (frm.elements[i].type == "text")//if the type of control is textbox
            //            {
            //                
            //                frm.elements[i].style.textAlign = "right";

            //            }
            //        }
        }


        function CurrencyFormatted(control) {
            var amount = control.value;
            //alert(amount);
            var delimiter = ","; // replace comma if desired
            var a = amount.split('.', 2)
            var d = a[1];
            var i = parseInt(a[0]);
            if (isNaN(i)) { return ''; }
            var minus = '';
            if (i < 0) { minus = '-'; }
            i = Math.abs(i);
            var n = new String(i);
            var a = [];

            while (n.length > 3) {
                var nn = n.substr(n.length - 3);
                a.unshift(nn);
                n = n.substr(0, n.length - 3);
            }

            if (n.length > 0) { a.unshift(n); }
            n = a.join(delimiter);

            if (d == null) {
                amount = n;
            }
            else {
                amount = n + '.' + d;
            }
            amount = minus + amount;
            control.value = '$' + amount;
        }

        function DecimalFormatted(control) {
            var amount = control.value;
            //alert(amount);
            var delimiter = ","; // replace comma if desired
            var a = amount.split('.', 2)
            var d = a[1];
            var i = parseInt(a[0]);
            if (isNaN(i)) { return ''; }
            var minus = '';
            if (i < 0) { minus = '-'; }
            i = Math.abs(i);
            var n = new String(i);
            var a = [];

            while (n.length > 3) {
                var nn = n.substr(n.length - 3);
                a.unshift(nn);
                n = n.substr(0, n.length - 3);
            }

            if (n.length > 0) { a.unshift(n); }
            n = a.join(delimiter);

            if (d == null) {
                amount = n;
            }
            else {
                amount = n + '.' + d;
            }
            amount = minus + amount;
            control.value = amount;

        }


        function jumpScroll() {
            window.scroll(0, 900); // horizontal and vertical scroll targets
        }

        function jumpScrollUp() {
            window.scroll(0, 0); // horizontal and vertical scroll targets
        }
        function Add(a, b) {
            a = a.replace('$', '');
            b = b.replace('$', '');
            var c = a + b;
            var c = c.toFixed(2);
            document.getElementById("txtText").value = c;
        }



        function trim(str) {
            if (!str || typeof str != 'string')
                return null;
            return str.replace(/^[\s]+/, '').replace(/[\s]+$/, '').replace(/[\s]{2,}/, ' ');
        }

        function HideOther() {
            //debugger
            gd = GetSelectedRow("ddgReasonForDelay");
            var sel_value = gd[1].innerHTML;
            var sel_value_index = gd[0].innerHTML;

            //debugger
            if (trim(sel_value) == "Other") {
                document.getElementById("tr_other_reason_for_delay").style.visibility = "visible";
                document.getElementById("tr_other_reason_for_delay").style.display = "";
            }
            else {
                document.getElementById("tr_other_reason_for_delay").style.visibility = "hidden";
                document.getElementById("tr_other_reason_for_delay").style.display = "none";
            }
        }

        function HideState() // not in use now
        {
            gd = GetSelectedRow("ddgAccrualType");
            var sel_value = gd[0].innerHTML;

            //debugger
            if (trim(sel_value) == "4" || trim(sel_value) == "6") // BID or RET
            {
                document.getElementById("td_state_2").style.visibility = "visible";
            }
            else {
                document.getElementById("td_state_2").style.visibility = "hidden";
            }
        }

        function ValidInvalid() {
            gd = GetSelectedRow("ddgReasonCode");
            var sel_id = gd[0].innerHTML;

            //debugger
            if (trim(sel_id) == "1") {
                document.getElementById("lblValid").innerHTML = "Valid";
            }
            else {
                if (trim(sel_id) == "0") {
                    document.getElementById("lblValid").innerHTML = "";
                }
                else {
                    document.getElementById("lblValid").innerHTML = "Invalid";
                }
            }
        }

        function AccrTypeAction() {
            //debugger
            gdAccrType = GetSelectedRow("ddgAccrualType");
            var accr_type_sel_id = gdAccrType[0].innerHTML;

            gdActionType = GetSelectedRow("ddgActionType");
            var action_type_sel_id = gdActionType[0].innerHTML;
            var action_type_value = gdActionType[1].innerHTML;

            //debugger
            if (trim(accr_type_sel_id) == "2" || trim(accr_type_sel_id) == "3") {
                if (action_type_sel_id != 0) {
                    document.getElementById("lblAccrTypeAction").innerText = action_type_value;
                    //hdActionTypeValue
                    document.getElementById("hdActionTypeValue").innerText = action_type_value;
                    document.getElementById("lblAccr1").innerText = action_type_value;
                    document.getElementById("lblAccr2").innerText = action_type_value;
                }
                else {
                    document.getElementById("lblAccrTypeAction").innerText = "Not selected";
                }
            }
            else {
                document.getElementById("lblAccrTypeAction").innerText = "n/a";
            }
        }

        function GetHelp() {
            alert("The 'Date of Report' has been received with the BA53 load request email. \r\n\It is a last day of month of the data received. This is read-only field.");
        }

        function Copy(txt_name_1, txt_name_2) {

            //debugger
            var txt1 = txt_name_1; // target control (read-only)
            var txt2 = txt_name_2; // initial control (editable)
            document.getElementById(txt1).value = document.getElementById(txt2).value;
            //alert("!");
        }

        function HideCalendarBtn() {
            //alert("!")
            document.getElementById("calReportDate_tdBttn").style.display = "none";
        }


    </script>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="contentTitle" runat="Server">
    <asp:Label ID="lblHeaderLabel" runat="server" Text="BA53 Item Details" />
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="content" runat="Server">
         <!-- #include virtual="../include/ErrorLabel.inc" -->
    

	
    <input type="hidden" id="txtItemID" runat="server" />
    <input type="hidden" id="txtLineNo" runat="server" />
    <input type="hidden" id="txtItemStatusCode" runat="server" />
    <input type="hidden" id="txtReviewerUserID" runat="server" /> 
    <input type="hidden" id="txtReviewerEmail" runat="server" />     
    <input type="hidden" id="txtReassignTargetPage" runat="server" /> 
    <input type="hidden" id="lblOrganization" runat="server" /> 
    <input type="hidden" id="lblActionTypeID_" runat="server" /> 
    <input type="hidden" id="hdActionTypeValue" runat="server" />
    <div style="display:none"><asp:TextBox ID="lblActionTypeID" EnableViewState="true"  runat="server"  ></asp:TextBox></div>

    
    <div class="sectionSubTitle">    
    <table width="100%" border="0" cellspacing="5" cellpadding="0">
        <tr>
            <td style="width:230px; height: 19px;"><span class="regBldBlueText">Reviewer: </span>
                <asp:Label ID="lblReviewer" runat="server"  CssClass="regBldGreyText"/>&nbsp;&nbsp;
                <input type="image" id="btnReviewer0" name="btnReviewer0" src="../images/ICON_reassign.gif" runat="server" 
                    alt="Reassign Reviewer for Open Item" class="icAssign" visible="false" />
            </td>
            <td style="width:400px; height: 19px;"><span class="regBldBlueText">Doc Number: </span><asp:Label ID="lblDocNumber" runat="server"  CssClass="regBldGreyText" /></td>
            <td style="width:200px; height: 19px;"><span class="regBldBlueText">
                Sort Code:&nbsp;</span>
                <asp:Label ID="lblSortCode" Visible="true" runat="server" CssClass="regBldGreyText" /></td>
        </tr>
        <tr>
            <td style="height: 16px"><span class="regBldBlueText">Status: </span><asp:Label ID="lblStatus" runat="server"  CssClass="regBldGreyText"/></td>
            <td style="height: 16px"><span class="regBldBlueText">Accrual Type: </span><asp:Label ID="lblAccrualType" runat="server" CssClass="regBldRedText"/></td>
            <td style="height: 16px"><span class="regBldBlueText"> Due Date:
                <asp:Label ID="lblDueDate" runat="server" CssClass="regBldGreyText" /></span></td>
        </tr>
        <tr>
            <td style="height: 15px"><span class="regBldBlueText">Valid: </span><asp:Label ID="lblValid" runat="server" CssClass="regBldRedText" /></td>
            <td style="height: 15px"><span class="regBldBlueText">Accrual Type Action:
                <asp:Label ID="lblAccrTypeAction" runat="server" EnableViewState="true" CssClass="regBldGreyText"/></span></td>
            <td style="height: 15px">
                <span class="regBldBlueText" visible="false">Org. Code: 
                    <asp:Label ID="lblOrgCode" style="display:inline" CssClass="regBldGreyText"  runat="server" ></asp:Label>
                </span>
            </td>
        </tr>        
    </table>
    </div>
    
    <br />
    
        <asp:Repeater ID="rpFeedback" runat="server" Visible="true" >            
        <ItemTemplate >
             <table width="100%" class="oddTD" style="border:solid 1px gray;" id="tblFeedback" runat="server">
                <tr><td colspan="5" class="hrBlueHeader">
                    <asp:Label ID="lblFeedback" runat="server" />
                </td></tr>
                <tr >
                    <td class="regBldBlueText" width="100px">CO Comments: </td>
                    <td colspan="4"><asp:Label ID="lblFdbCOComment" runat="server" /></td>
                </tr>
                <tr >
                    <td class="regBldBlueText" width="100px">Valid: </td>
                    <td colspan="4">
                        <asp:DropDownList ID="ddlFdbValid" runat="server" Visible="false" />
                        <asp:Label ID="lblFdbValid" runat="server" Visible="false" />    
                    </td>
                </tr>
                <tr >
                    <td class="regBldBlueText">Response: </td>
                    <td colspan="3">
                        <asp:TextBox ID="txtFdbResponse" runat="server" TextMode="MultiLine" Rows="2" Width="600" Visible="false" MaxLength="500" />
                        <asp:Label ID="lblFdbResponse" runat="server" Visible="false" />    
                    </td>
                    <td rowspan="2" valign="bottom" align="center" >
                        
                        <input type="button" id="btnSaveFdb" runat="server" class="button" value="save response" visible="false" />
                    </td>
                </tr>
                <tr id="trDO_UDO" runat="server">
                    <td><asp:Label ID="lblFdbUDOLabel" runat="server" Text="UDO Should Be:" CssClass="regBldBlueText" /></td>
                    <td><asp:TextBox  MaxLength="12" ID="txtFdbUDO" runat="server" /></td>
                    <td><asp:Label ID="lblFdbDOLabel" runat="server" Text="DO Should Be:" CssClass="regBldBlueText" /></td>
                    <td><asp:TextBox   MaxLength="12"  ID="txtFdbDO" runat="server" /></td>                    
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
    <!--
    <table width="100%" border="0" cellspacing="2" cellpadding="2">
        <tr>
            <td align="center"  id="td_item1" style="cursor:hand; width:25%" class="lrgBldWhiteText"  onclick="Get_Item('1');">
                &nbsp;Item Line 1 (Valid)</td>
            <td align="center"  id="td_item2" style="cursor:hand" class="lrgBldBlackTextGrBckgrd" onclick="Get_Item('2');">
                &nbsp;Item Line 2 (Not Valid)</td>
            <td style="width:50%"></td>
        </tr>
    </table>  
    -->
    
    
    
    <table width="100%" border="0" cellspacing="1" cellpadding="1"> 
        <tr class="oddTD">
            <td style="width: 242px"><span class="regBldBlueText" style="width:25%">Lease Number: </span><asp:Label ID="lblLeaseNum"  runat="server" CssClass="regBldGreyText" /></td>
            <td style="width: 250px"><span class="regBldBlueText" style="width:23%"> BA: <asp:Label ID="lblBA" runat="server" CssClass="regBldGreyText" /></span></td>
            <td style="width: 224px"><span class="regBldBlueText" style="width:30%"><span>BBFY: 
                <asp:Label ID="lblBBFY" runat="server" CssClass="regBldGreyText" /></span></span></td>
            <td><span class="regBldBlueText" style="width:22%">UDO: <asp:Label ID="lblUDO" runat="server" CssClass="regBldGreyText" /></span></td>
        </tr>
        <tr class="evenTD">
            <td style="width: 240px;"><span class="regBldBlueText"><span>Building: <asp:Label ID="lblBuilding"   runat="server" CssClass="regBldGreyText" /></span></span></td>
            <td style="width: 254px;"><span class="regBldBlueText"> Accrual: <asp:Label ID="lblACCRUAL" runat="server" CssClass="regBldGreyText" /></span></td>
            <td style="width: 222px"><span class="regBldBlueText"> Total DO Should Be: <asp:TextBox   MaxLength="12" ID="txtTotalDOShouldBe" runat="server" CssClass="regTextAlignRight" Width="95px" onblur="extractNumber(this,2,false);CurrencyFormatted(this)" onkeyup="extractNumber(this,2,false);" onkeypress="return blockNonNumbers(this, event, true, false);"/></span></td>
            <td><span class="regBldBlueText">CE: <asp:Label ID="lblCE" runat="server" CssClass="regBldGreyText" /></span></td>
        </tr>
        <tr class="oddTD">
            <td ><span class="regBldBlueText"><span>Title: <asp:Label ID="lblTitle" runat="server" CssClass="regBldGreyText" /></span></span></td>
            <td >
                <table width="95%" border="0">
                    <tr title="The 'Date of Report' has been received with the BA53 load request email. It is a last day of month of the data received. This is read-only field." >
                        <td style="width:40%" valign="middle" class="regBldBlueText">
                            Est.Compl. Lease Date :
                        </td>
                        <td  valign="middle" style="width:95px">
                            <uc:GetDate ID="calReportDate" Width="70px"  runat="server" />
                        </td>
                        <!--
                        <td  valign="middle" align="left">
                            <img src="../images/question_mark.gif" onclick="GetHelp()" alt="" title="The 'Date of Report' has been received with the BA53 load request email. It is a last day of month of the data received. This is read-only field." />
                        </td>
                        -->
                
                    </tr>
                </table>
                </td>
            <td ><span class="regBldBlueText">OC: <asp:Label ID="lblOC" runat="server" CssClass="regBldGreyText" /></span></td>
            <td><span class="regBldBlueText"><span>FC: <asp:Label ID="lblFC" runat="server" CssClass="regBldGreyText" /></span></span></td>
        </tr>
        <tr class="evenTD">
            <td ><span class="regBldBlueText"> Total DO: <asp:Label ID="lblTotalDO" runat="server" CssClass="regTextAlignRight" /></span></td>
            <td ><span class="regBldBlueText"> Pending Pymt: <span class="regBldRedText"></span><asp:Label ID="lblPENDPYMT" runat="server" CssClass="regTextAlignRight" Width="80px" /></span></td>
            <td  ><span class="regBldBlueText">
            <asp:Label ID="lblACTG_PD" Visible="false" runat="server" CssClass="regBldGreyText" /></span></td>
            <td><span class="regBldBlueText"> </span></td>                    
        </tr>
        <tr class="oddTD">
            <td ><span class="regBldBlueText">Prior year payment history attached?</span></td>
            <td ><span class="regBldBlueText" style="color: #000000"> 
                <asp:RadioButtonList ID="rbPmtHistory" runat="server" RepeatDirection="Horizontal" Width="152px">
                    <asp:ListItem Value="1">Yes</asp:ListItem>
                    <asp:ListItem Value="0">No</asp:ListItem>
                </asp:RadioButtonList></span></td>
            <td ><span class="regBldBlueText">Does file contain support for methodology for calculation of accrual?</span></td>
            <td ><span class="regBldBlueText">
                <asp:RadioButtonList ID="rbSupportCalc" runat="server" RepeatDirection="Horizontal" Width="152px" OnSelectedIndexChanged="rbSupportCalc_SelectedIndexChanged" AutoPostBack="True">
                    <asp:ListItem Value="1">Yes</asp:ListItem>
                    <asp:ListItem Value="0">No</asp:ListItem>
                </asp:RadioButtonList></span></td>
        </tr>
        
         <tr class="evenTD">
            <td colspan="4" >
        </tr>
        
         <tr class="evenTD">
            <td colspan="4" >
                <asp:Label ID="lblReassignLines" runat="server" CssClass="regBldGreyText" Text="To reassign item to different reviewer click here: " />
                <input type="image" id="btnReviewer" name="btnReviewer" src="../images/ICON_reassign.gif" runat="server" 
                 alt="Reassign Reviewer for selected Lines" class="icAssign" onclick="javascript:return reassign_lines();" />
                 <p />
            </td>
        </tr> 
        
        <tr class="evenTD">
            <td ><span class="regBldBlueText"> </span></td>
            <td ><span class="regBldBlueText" style="color: #000000"></span></td>
            <td ><span class="regBldBlueText"> </span></td>
            <td></td>
        </tr> 
        
        <tr class="evenTD">
            <td colspan="4"><asp:Label ID="lblTypeOfLease" runat="server" CssClass="regBldGreyText" />&nbsp;
                <asp:Label ID="lblACCTLNUM" Visible="false" runat="server" CssClass="regBldGreyText" />&nbsp;
                <asp:Label ID="lblLineNumber" Visible="false"  runat="server" CssClass="regBldGreyText" />&nbsp;
                <asp:Label ID="lblVendorName" Visible="false"   runat="server" CssClass="regBldGreyText" />&nbsp;
                <asp:Label ID="lblVendorNumber" Visible="false"   runat="server" CssClass="regBldGreyText" />&nbsp;
                <asp:Label ID="lblEBFY"  Visible="false" runat="server" CssClass="regBldGreyText" /></td> 
        </tr>         
    </table>
        
    <table >
        <tr>
            <td valign="top" style="width:50%" class="colorBorder"><uc:Contacts ID="ctrlContacts" runat="server" /></td>
            <td valign="top" style="width:50%" class="colorBorder"><uc:Attachments ID="ctrlAttachments" runat="server" /></td>
        </tr>
    </table>    
    <br />
    
    
     <table width="100%" border="0" cellspacing="0" cellpadding="0">
        <tr class="evenTD" id="tr_accr_type" runat="server" style="height:40px">
            <td style="width:10%">
                <span class="regBldBlueText">
                    <asp:Label ID="lblAccrTypeLabel" runat="server" CssClass="regTextAlignRight" />
                </span>
            </td>
            
            <td  style="width:25%">
                <uc1:dropdowngrid ID="ddgAccrualType" runat="server" Width="150px" />
            </td>
            
            <td id="td_action_1"  runat="server" style="width:10%">
                <span class="regBldBlueText">
                    <asp:Label ID="lblAccrualTypeActionLabel" runat="server" CssClass="regTextAlignRight" />
                </span>
            </td>
            
            <td id="td_action_2"  runat="server" style="width:25%">
                <uc1:dropdowngrid ID="ddgActionType" EnableViewState="true" OnChange="AccrTypeAction()" runat="server" Width="150px" />
            </td>
            
            <td id="td_state_1"  runat="server" style="width:10%">
                <span class="regBldBlueText">
                    <asp:Label ID="lblState" text ="State:" runat="server" CssClass="regTextAlignRight" />
                </span>
            </td>
            
            <td  id="td_state_2"  runat="server" style="width:20%">
                <uc1:dropdowngrid ID="ddgState" runat="server" Width="150px" />
            </td>
            
        </tr> 
        

       
    </table>
    
      <hr />
      <div id="div_calc_methodology" runat="server">
       <table id="tblCalcMeth" runat="server" width="100%" border="1" cellspacing="0" cellpadding="3">  
              
            <tr class="evenTD">
                <td colspan="2"  align="center" class="lrgBldText"><span>Calculation Methodology </span></td> 
            </tr>
           <!-- common section for all types -->
            <tr class="oddTD" style="background-color:#d3d3d3" id="tr_curr_lease_inf" runat="server" > 
                <td colspan="2"  align="center"  ><span class="lrgBldText">Current Lease Information</span></td> 
            </tr>
            <!-- Types 1,2,3,4,5 -->
            <tr class="oddTD" style="height:20px;display:none;">
                <td title="Current Lease Number" style="width:75%"><span class="regBldBlueText"  style="width:25%">Lease Number: </span></td> 
                <td><span class="regBldBlueText" style="width:25%"></span><asp:TextBox ID="txtLeaseNumber"    Width="130px"  ToolTip="Current Lease Number" runat="server" CssClass="regTextAlignRight" MaxLength="30"/> </td>
            </tr>
             <!-- Type 5 -->
            <tr class="evenTD" id="tr_eff_date_action" runat="server"> 
                <td title="Effective Date of Action"><span class="regBldBlueText" style="width:25%">Effective Date of Action: </span></td>
                <td><span class="regBldBlueText" style="width:25%"></span><uc:GetDate ID="calEffDateOfAction5" runat="server" /></td>
            </tr>
  
             <!-- Types 2,5 -->
            <tr class="oddTD" id="tr_proj_no" runat="server">
                <td title="Current Project Number"><span class="regBldBlueText"  style="width:25%">Project Number: </span></td> 
                <td><span class="regBldBlueText" style="width:25%"></span><asp:TextBox ID="txtCurrProjNo"   Width="130px"  ToolTip="Current Lease Project Number" runat="server" CssClass="regTextAlignRight" MaxLength="30"/> </td>
            </tr>
            <!-- Type 4 -->
            <tr class="evenTD" id="tr_lease_ann_date" runat="server">
                <td title="Current Lease Anniversary Date"><span class="regBldBlueText" style="width:25%">Lease Anniversary Date: </span></td>
                <td><span class="regBldBlueText" style="width:25%"></span><uc:GetDate ID="calLeasAnnDate" runat="server" /></td>
            </tr>
           
            <!-- Type 5 -->
            <tr class="oddTD" id="tr_new_proj_ann_rent" runat="server">
                <td title="New Projected Annual Rent"><span class="regBldBlueText"  style="width:25%">New Projected Annual Rent: </span></td> 
                <td><span class="regBldBlueText" style="width:25%"></span><asp:TextBox  MaxLength="12"  ID="txtNewProjAnnualRent" ToolTip = "New Projected Annual Rent" onblur="extractNumber(this,2,false);CurrencyFormatted(this)" onkeyup="extractNumber(this,2,false);" onkeypress="return blockNonNumbers(this, event, true, false);" 
                     Width="130px"   runat="server" CssClass="regTextAlignRight" /> </td>
            </tr>
            <!-- Types 1,2,3 -->
            <tr class="evenTD" id="tr_lease_eff_date" runat="server">
                <td title="Current Lease Effective Date"><span class="regBldBlueText" style="width:25%">Lease Effective Date: </span></td>
                <td><span class="regBldBlueText" style="width:25%"></span><uc:GetDate ID="calLeaseEffDate" runat="server" /></td>
            </tr>
           
            <!-- Types 1,2,3,4 -->
            <tr class="oddTD" id="tr_annual_rent" runat="server">
                <td  title="Current Annual Rent"><span class="regBldBlueText" style="width:25%">Annual Rent: </span></td>
                <td><span class="regBldBlueText" style="width:25%"></span><asp:TextBox  MaxLength="12"  ID="txtAnnualRent" onblur="extractNumber(this,2,false);CurrencyFormatted(this)" onkeyup="extractNumber(this,2,false);" onkeypress="return blockNonNumbers(this, event, true, false);" 
  Width="130px"  ToolTip="Current Annual Rent"  runat="server" CssClass="regTextAlignRight" /> </td>
            </tr>
             <!-- Type 3 -->
            <tr class="evenTD"  id="tr_lease_exp_date" runat="server">
                <td title="Current Lease Expiration Date"><span class="regBldBlueText" style="width:25%">Lease Expiration Date: </span></td>
                <td><span class="regBldBlueText" style="width:25%"></span><uc:GetDate ID="calLeaseExpDate" runat="server" /></td>
                </tr>
            
            <!-- Types 1,2,3,4,5 -->
            <tr class="evenTD" id="tr_rsf" runat="server"> 
                <td  title="Current Rentable Square Feet"><span class="regBldBlueText" style="width:25%">RSF: </span></td>
                <td><span class="regBldBlueText" style="width:25%"></span><asp:TextBox   MaxLength="12"  ID="txtRSF" onblur="extractNumber(this,2,false);;DecimalFormatted(this)" onkeyup="extractNumber(this,2,false);" onkeypress="return blockNonNumbers(this, event, true, false);" 
 Width="130px"  ToolTip="Current Rentable Square Feet" runat="server" CssClass="regTextAlignRight" /> </td>
            </tr>
            
            
        </table>
        </div>
        
        <div id="div_projected_lease_info" runat="server">
        <table id="tblProjNewLeaseInf" runat="server"  width="100%" border="1" cellspacing="0" cellpadding="3">
            <!-- Only for 'Holdower Details' type (3) under Calculation Methodology, Projected New Lease Information -->   
             <tr class="evenTD" style="background-color:#d3d3d3">
                <td colspan="2" align="center" ><span class="lrgBldText">Projected New Lease Information </span></td> 
            </tr>
            <!-- Type 3 -->
            <tr class="oddTD">
                <td title="Projected New Lease Number"  style="width:75%"><span class="regBldBlueText"  style="width:25%">Projected New Lease Number: </span></td> 
                <td><span class="regBldBlueText" style="width:25%"></span><asp:TextBox ID="txtPtojNewLeaseNo"  Width="130px"  ToolTip="Projected New Lease Number" runat="server" CssClass="regText" MaxLength="30"/> </td>
                </tr>
            <!-- Type 3 -->    
            <tr class="evenTD" >
                <td title="Project Number"><span class="regBldBlueText"  style="width:25%">Projected New Project Number: </span></td> 
                <td><span class="regBldBlueText" style="width:25%"></span><asp:TextBox ID="txtProjProjectNo"   Width="130px"  ToolTip="Projected Project Number" runat="server" CssClass="regText" MaxLength="30"/> </td>
            </tr>
            <!-- Type 3 -->
            <tr class="oddTD" >
                <td title="Prospectus Number"><span class="regBldBlueText"  style="width:25%">Prospectus Number: </span></td> 
                <td><span class="regBldBlueText" style="width:25%"></span><asp:TextBox ID="txtProjProspNo"   Width="130px"  ToolTip="Prospectus Number" runat="server" CssClass="regText" MaxLength="15"/> </td>
            </tr>
            <!-- Type 3 -->
             <tr class="evenTD" id="tr4" runat="server">
                <td title="Projected Lease Effective Date"><span class="regBldBlueText" style="width:25%">Projected New Lease Effective Date: </span></td>
                <td><span class="regBldBlueText" style="width:25%"></span><uc:GetDate ID="calProjLeaseEffDate" runat="server" /></td>
            </tr>
            <!-- Type 3 -->
            <tr class="oddTD" id="tr5" runat="server">
                <td  title="Projected New Lease Annual Rent"><span class="regBldBlueText" style="width:25%">Projected New Annual Rent: </span></td>
                <td><span class="regBldBlueText" style="width:25%"></span><asp:TextBox ID="txtProjAnnualRent"  MaxLength="12"  onblur="extractNumber(this,2,false);CurrencyFormatted(this)" onkeyup="extractNumber(this,2,false);" onkeypress="return blockNonNumbers(this, event, true, false);" 
  Width="130px"  ToolTip="Projected Annual Rent"  runat="server" CssClass="regTextAlignRight" /> </td>
            </tr>
            <!-- Type 3 -->
            <tr class="evenTD" id="tr6" runat="server">
                <td  title="Projected Rentable Square Feet"><span class="regBldBlueText" style="width:25%">RSF: </span></td>
                <td><span class="regBldBlueText" style="width:25%"></span><asp:TextBox   MaxLength="12" ID="txtProjRSF"   Width="130px" onblur="DecimalFormatted(this)"  ToolTip="Projected Rentable Square Feet" runat="server" CssClass="regTextAlignRight" /> </td>
            </tr>
         </table>
         </div>
         
         <div id="div_breakout_part_year_establ_accr" runat="server">
         <table id="tblreakoutOfPartYearCostEst" runat="server" width="100%" border="1" cellspacing="0" cellpadding="3">   
            <tr class="evenTD" >
                <td colspan="3" align="center" style="background-color:#d3d3d3" class="lrgBldText"><span >Breakout of Part Year Cost of Established Accrual</span></td> 
                <td class="lrgBldWhiteText"><span></span></td>
            </tr>
            
            <tr class="oddTD" id="tr_curr_proj_ann_rent_est" runat="server">
                <td title="Current Projected Annual Rent" style="width:25%;"><span class="regBldBlueText">Current Projected Annual Rent: </span></td>
                <td style="width:25%"><span class="regBldBlueText" >&nbsp;</span><asp:Label ID="lblCurrProjAnnRentEst"  Width="135px"   ToolTip="Current Projected Annual Rent_"  runat="server" CssClass="regTextAlignRight" /></td>
                <td style="width:25%;"><span class="regBldBlueText"><asp:Label ID="lblAccr1"  Visible="false" runat="server" CssClass="regTextAlignRight" /></span></td>
                <td style="width:25%;"><span class="regBldBlueText" >&nbsp;</span></td>
               
            </tr>
            
             <tr class="oddTD" id="tr_old_proj_ann_rent_est" runat="server">
                <td ><span class="regBldBlueText">Old Projected Annual Rent</span></td>
                <td class="regBldBlueText" ><asp:TextBox ID="txtOldProjAnnRent_est"   MaxLength="12" onblur="extractNumber(this,2,false);CurrencyFormatted(this)" onkeyup="extractNumber(this,2,false);" onkeypress="return blockNonNumbers(this, event, true, false);" 
 ToolTip=""  Width="130px"   runat="server" CssClass="regTextAlignRight"  /></td>
                <td class="regBldBlueText" >&nbsp;</td>
                <td class="regBldBlueText" >&nbsp;</td>
            </tr>
            
             <tr class="evenTD" id="tr_lbl_new_proj_ann_rent_est" runat="server">
                <td title="Projected New Lease Annual Rent"><span class="regBldBlueText">New Projected Annual Rent_: </span></td>
                <td><span class="regBldBlueText" >&nbsp;</span><asp:Label ID="lblNewProjAnnRent_est" Width="135px"  ToolTip="New Projected Annual Rent"  runat="server" CssClass="regTextAlignRight" /></td>
                <td><span class="regBldBlueText" >&nbsp;</span></td>
                <td><span class="regBldBlueText" >&nbsp;</span></td>
            </tr>
            
            <tr class="evenTD" id="tr_txt_new_proj_ann_rent_est" runat="server">
                <td title="New Projected Annual Rent"><span class="regBldBlueText">New Projected Annual Rent: </span></td>
                <td><span class="regBldBlueText" >&nbsp;</span><asp:TextBox ID="txtNewProjAnnRent_est"  MaxLength="12"  onblur="extractNumber(this,2,false);CurrencyFormatted(this)" onkeyup="extractNumber(this,2,false);" onkeypress="return blockNonNumbers(this, event, true, false);" 
 ToolTip=""  Width="130px"   runat="server" CssClass="regTextAlignRight"  /></td>
                <td><span class="regBldBlueText" >&nbsp;</span></td>
                <td><span class="regBldBlueText" >&nbsp;</span></td>
            </tr>
                                   
            <tr class="oddTD" >
                <td title="Annual Increase of Action"><span class="regBldBlueText">Annual Increase of Action_:</span></td>
                <td><span class="regBldBlueText">&nbsp;</span><asp:Label ID="lblAnnIncrOfActionEst" Width="135px"  runat="server" CssClass="regTextAlignRight"/></td>
                <td><span class="regBldBlueText" >Monthly Increase:</span></td>
                <td><span class="regBldBlueText" >&nbsp;</span><asp:Label ID="lblMonthlyIncreaseEst" Width="130px" runat="server" CssClass="regTextAlignRight"/></td>
            </tr>
            
            <tr class="evenTD" >
                <td  style="width:25%" title="Effective Date of Action"><span class="regBldBlueText">Effective Date of Action:</span></td>
                <td style="width:25%"><span class="regBldBlueText" ><uc:GetDate ID="calEffDateOfActionEst" runat="server" /></span></td>
                <td style="width:25%"><span class="regBldBlueText" >Total Months:</span></td>
                <td style="width:25%"><span class="regBldBlueText" >&nbsp;</span><asp:Label ID="lblTotalMonthsEst"  Width="130px"  runat="server" CssClass="regTextAlignRight"/></td>
            </tr>
            
             <tr class="oddTD" >
                <td title="Fiscal Year End Date"><span class="regBldBlueText">Fiscal Year End Date:</span></td>
                <td><span class="regBldBlueText" ><uc:GetDate ID="calFiscalYearEndDateEst" runat="server" /></span></td>
                <td><span class="regBldBlueText">Total Months Catch-up:</span></td>
                <td><span class="regBldBlueText" >&nbsp;</span><asp:Label ID="lblTotalMonthsCatchUpEst" Width="130px"  runat="server" CssClass="regTextAlignRight"/></td>
            </tr>
            
             <tr class="evenTD" >
                <td title="Number of Days in Prorated Month"><span class="regBldBlueText" >Number of Days in Prorated Month:</span></td>
                
                <td><span class="regBldBlueText" >&nbsp;<asp:TextBox ID="txtNoDaysProratedMonthEst"   MaxLength="2"  onblur="extractNumber(this,0,false);DecimalFormatted(this);" onkeyup="extractNumber(this,0,false);" onkeypress="return blockNonNumbers(this, event, false, false);" runat="server" CssClass="regTextAlignRight" ToolTip="Number of Days in Prorated Month"/></span></td>
                 
                <td><span class="regBldBlueText" >Days Prorated:</span></td>
                <td><span class="regBldBlueText" >&nbsp;</span><asp:Label ID="lblDaysProratedEst" runat="server"  Width="130px" CssClass="regTextAlignRight"/></td>
            </tr>
            
             <tr class="oddTD" >
                <td title=""><span class="regBldBlueText">&nbsp;</span></td>
                <td><span class="regBldBlueText" >&nbsp;</span></td>
                <td><span class="regBldBlueText" >Total Day Catch-up:</span></td>
                <td><span class="regBldBlueText">&nbsp;</span><asp:Label ID="lblTotalDayCatchUpEst" Width="130px"  runat="server" CssClass="regTextAlignRight"/></td>
            </tr>
            
             <tr class="evenTD" >
                <td title=""><span class="regBldBlueText" >Fiscal Year:</span></td>
                <td><span class="regBldBlueText" >&nbsp;</span><asp:Label ID="lblFY_YY_Est"  runat="server" CssClass="regBldBlueText"/></td>
                <td><span class="regBldBlueText" >Total Catch-up:</span></td>
                <td><span class="regBldBlueText" >&nbsp;</span><asp:Label ID="lblTotalCatchUpEst" Width="130px"  runat="server" CssClass="regTextAlignRight"/></td>
            </tr>
         </table>
         </div>
         
         <div id="div_breakout_part_yaer_cost" runat="server"> <!-- accr types 1,2,3,5 (Breakout of Part Year Cost - / or --- Revised/ section)-->
         <table id="tblPartYearCostTitle" runat="server"  width="100%" border="1" cellspacing="0" cellpadding="3">   
            <tr class="evenTD" >
                <td colspan="3" align="center"  style="background-color:#d3d3d3" class="lrgBldText"><span ><asp:Label ID="lblPartYearCostTitle"  runat="server" /></span></td> 
                <td class="lrgBldWhiteText"><span></span></td> 
                
            </tr>
            
            <tr class="oddTD" id="tr_FY2" runat="server" style="display:none">
                <td title="CPI Expense 2 years prior" style="width:25%"><span class="regBldBlueText"><asp:Label ID="lblFY_2_CPIEscExp"  runat="server" CssClass="regBldBlueText"/></span></td>
                <td style="width:225px"><span class="regBldBlueText" >
                    </span><asp:TextBox ID="txtFY_2_CPIEscExp"   MaxLength="12" onblur="extractNumber(this,2,false);CurrencyFormatted(this)" onkeyup="extractNumber(this,2,false);" onkeypress="return blockNonNumbers(this, event, true, false);" 
                    Text="[FY-2] CPI Escalation Expense" Width="130px"  ToolTip="CPI Expense 2 years prior"  runat="server" CssClass="regTextAlignRight" />
                </td>
                <td  style="width:25%"><span class="regBldBlueText">&nbsp;</span></td>
                <td style="width:25%"><span class="regBldBlueText" >&nbsp;</span></td>
            </tr>
            
            <tr class="evenTD"  id="tr_FY1" runat="server"  style="display:none">
                <td title="CPI Expense 1 year prior" style="height: 31px"><span class="regBldBlueText" ><asp:Label ID="lblFY_1_CPIEscExp"  runat="server" CssClass="regBldBlueText"/></span></td>
                <td style="width: 225px; height: 31px"><span class="regBldBlueText" >
                    </span><asp:TextBox ID="txtFY_1_CPIEscExp"  MaxLength="12"  onblur="extractNumber(this,2,false);CurrencyFormatted(this)" onkeyup="extractNumber(this,2,false);" onkeypress="return blockNonNumbers(this, event, true, false);" Text="[FY-1] CPI Escalation Expense" Width="130px"   ToolTip="CPI Expense 1 years prior"  runat="server" CssClass="regTextAlignRight" />
                </td>
                <td style="height: 31px"><span class="regBldBlueText" >&nbsp;</span><asp:TextBox ID="lblFY_1_EscalationExpenseDiff" runat="server"  CssClass="textbox_as_label" /></td>
                <td style="height: 31px"><span class="regBldBlueText" >&nbsp;</span><asp:TextBox ID="lblFY_1_EscalationExpensePercIncrease" ToolTip = "Percent Increase"  runat="server" CssClass="textbox_as_label"/></td>
            </tr>
            
            <tr class="oddTD"  id="tr_FY" runat="server"  style="display:none">
                <td title="Current Projected CPI Expense"><span class="regBldBlueText" ><asp:Label ID="lblFYCPIEscProj"  runat="server" CssClass="regBldBlueText"/></span></td>
                <td style="width: 225px"><span class="regBldBlueText" >
                    </span><asp:TextBox ID="txtFYCPIEscProj"   MaxLength="12"  onblur="extractNumber(this,2,false);CurrencyFormatted(this)" onkeyup="extractNumber(this,2,false);" onkeypress="return blockNonNumbers(this, event, true, false);" Text="[Current FY] CPI Escalation Projection" Width="130px"   ToolTip="Current Projected CPI Expense"  runat="server" CssClass="regTextAlignRight"  />
            </td>
                <td><span class="regBldBlueText" >&nbsp;</span><asp:Label ID="lblFY_EscalationExpenseDiff"  ToolTip="Current Projected CPI Expense Diff" runat="server" CssClass="regTextAlignRight" /></td>
                <td><span class="regBldBlueText" >&nbsp;</span><asp:Label ID="lblFY_EscalationExpensePercIncrease"  ToolTip = "Percent Increase"  runat="server" CssClass="regTextAlignRight"/></td>
            </tr>
            
             <tr class="evenTD" id="tr_curr_proj_ann_rent" runat="server" >
                <td title="Current Projected Annual Rent" ><span class="regBldBlueText">Current Projected Annual Rent: </span></td>
                <td style="width: 225px"><span class="regBldBlueText">&nbsp;</span><asp:Label ID="lblCurrProjAnnRent" Width="130px"    ToolTip="Current Projected Annual Rent _"  runat="server" CssClass="regTextAlignRight" /></td>
                <td><span class="regBldBlueText" ><asp:Label ID="lblAccr2"  Visible="false"   runat="server" CssClass="regTextAlignRight" />&nbsp;</span></td>
                <td><span class="regBldBlueText" >&nbsp;</span></td>
               
            </tr>
             <tr class="oddTD" id="tr_lbl_new_proj_ann_rent" runat="server" >
                <td title="New Projected Annual Rent"><span class="regBldBlueText">New Projected Annual Rent__: </span></td>
                <td style="width: 225px"><span class="regBldBlueText" >&nbsp;</span><asp:Label ID="lblNewProjAnnRent" Width="130px"    ToolTip="New Projected Annual Rent"  runat="server" CssClass="regTextAlignRight" /></td>
                <td><span class="regBldBlueText" >&nbsp;</span></td>
                <td><span class="regBldBlueText" >&nbsp;</span></td>
            </tr>
            
             <tr class="evenTD" id="tr_old_proj_ann_rent" runat="server" >
                <td title="Old Projected Annual Rent _"><span class="regBldBlueText">Old Projected Annual Rent: </span></td>
                <td style="width: 225px"><span class="regBldBlueText">&nbsp;</span><asp:TextBox  MaxLength="12"  ID="txtOldProjAnnRent" onblur="extractNumber(this,2,false);CurrencyFormatted(this)" onkeyup="extractNumber(this,2,false);" onkeypress="return blockNonNumbers(this, event, true, false);" 
 Width="130px"   ToolTip="Old Projected Annual Rent>"  runat="server" CssClass="regTextAlignRight"  /></td>
                <td><span class="regBldBlueText" >&nbsp;</span></td>
                <td><span class="regBldBlueText">&nbsp;</span></td>
               
            </tr>
             <tr class="oddTD" id="tr_txt_new_proj_ann_rent" runat="server" >
                <td title="New Projected Annual Rent"><span class="regBldBlueText">New Projected Annual Rent: </span></td>
                <td style="width: 225px"><span class="regBldBlueText" >&nbsp;</span><asp:TextBox   MaxLength="12" ID="txtNewProjAnnRent" ToolTip="New Projected Annual Rent _" onblur="extractNumber(this,2,false);CurrencyFormatted(this)" onkeyup="extractNumber(this,2,false);" onkeypress="return blockNonNumbers(this, event, true, false);" 
 Width="130px"  runat="server" CssClass="regTextAlignRight"  /></td>
                <td><span class="regBldBlueText" >&nbsp;</span></td>
                <td><span class="regBldBlueText" >&nbsp;</span></td>
            </tr>
            
            <tr class="evenTD" >
                <td title="Annual Increase of Action"><span class="regBldBlueText" >Annual Increase of Action: </span></td>
                <td style="width: 225px">&nbsp;<asp:TextBox   MaxLength="12" ID="txtAnnualIncrOfAction"  Width="130px" onblur="extractNumber(this,2,false);CurrencyFormatted(this)" onkeyup="extractNumber(this,2,false);" onkeypress="return blockNonNumbers(this, event, true, false);" 
                 ToolTip="Annual Increase of Action (calculated read-only value)"  runat="server"   CssClass="regTextAlignRight" /></td>
                <td><span class="regBldBlueText" >Monthly Increase: </span></td>
                <td><span class="regBldBlueText" >&nbsp;</span><asp:Label ID="lblMonthlyIncrease" Width="130px"  runat="server" CssClass="regTextAlignRight"/></td>
            </tr>
             
            <tr class="oddTD" >
                <td  style="width:25%" title="Effective Date of Action _"><span class="regBldBlueText" >Effective Date of Action: </span></td>
                <td  style="width:225px" title="Effective Date of Action"><span class="regBldBlueText" ><uc:GetDate ID="calEffDateOfAction" runat="server" /></span></td>
                <td  style="width:25%"><span class="regBldBlueText" >Total Months: </span></td>
                <td style="width: 25%"><span class="regBldBlueText" >&nbsp;</span><asp:Label ID="lblTotalMonths"  Width="130px" runat="server" CssClass="regTextAlignRight"/></td>
            </tr>
            
             <tr class="evenTD" id="tr_f_year_end_date" runat="server"> 
                <td title="Fiscal Year End Date"><span class="regBldBlueText" >Fiscal Year End Date: </span></td>
                <td title="Fiscal Year End Date" style="width: 225px"><span class="regBldBlueText" >
                    <uc:GetDate ID="calFiscalYearEndDate" runat="server" /></span>
                </td>
                <td><span class="regBldBlueText">Total Months Catch-up: </span></td>
                <td><span class="regBldBlueText" >&nbsp;</span><asp:Label ID="lblTotalMonthsCatchUp1_2" Width="130px" runat="server" CssClass="regTextAlignRight"/></td>
            </tr>
            
            <tr class="evenTD"  id="tr_curr_audit_end_date" runat="server">
                <td title="Current Audit End Date" style="height: 43px"><span class="regBldBlueText" >Current Audit End Date: </span></td>
                <td style="width: 225px; height: 43px;" title = ""><span class="regBldBlueText" >
                    <uc:GetDate ID="calCurrAuditEndDate" runat="server" /></span>
                </td>
                <td style="height: 43px"><span class="regBldBlueText" >Total Months Catch-up:</span></td>
                <td style="height: 43px"><span class="regBldBlueText" >&nbsp;</span><asp:Label ID="lblTotalMonthsCatchUp3_5" Width="130px" runat="server" CssClass="regTextAlignRight"/></td>
            </tr>
            
            <tr class="oddTD" >
                <td title="Number of Days in Prorated Month"><span class="regBldBlueText" >Number of Days in Prorated Month: </span></td>
                <td style="width: 225px"><span class="regBldBlueText" ></span><asp:TextBox MaxLength="2" ID="txtNoDaysProratedMonth"  onblur="extractNumber(this,0,false);DecimalFormatted(this);" onkeyup="extractNumber(this,0,false);" onkeypress="return blockNonNumbers(this, event, false, false);" 
                ToolTip="Number of Days in Prorated Month"  Width="130px"   runat="server" CssClass="regTextAlignRight"  /></td>
                <td><span class="regBldBlueText" >Days Prorated:</span></td>
                <td><span class="regBldBlueText" >&nbsp;</span><asp:Label ID="lblDatesProrated" Width="130px" ToolTip="Dates Prorated." CssClass="regTextAlignRight" runat="server" /></td>
            </tr>
             
             <tr class="evenTD" >
                <td  title=""><span class="regBldBlueText">&nbsp;</span></td>
                <td title="" style="width: 225px"><span class="regBldBlueText">&nbsp;</span></td>
                <td><span class="regBldBlueText">Total Day Catch-up:</span></td>
                <td><span class="regBldBlueText" >&nbsp;</span><asp:Label ID="lblTotalDayCatchUp" Width="130px"  runat="server" CssClass="regTextAlignRight"/></td>
            </tr>
            
            <tr class="oddTD">
                <td title=""><span class="regBldBlueText" >Fiscal Year:</span></td>
                <td><span class="regBldBlueText" >&nbsp;</span><asp:Label ID="lblFY_YY" ToolTip = ""  runat="server" CssClass="regBldBlueText"/></td>
                <td><span class="regBldBlueText" >Total Catch-up:</span></td>
                <td><span class="regBldBlueText" >&nbsp;</span><asp:Label ID="lblTotalCatchUp" Width="130px" runat="server" CssClass="regTextAlignRight"/></td>
            </tr>           
       </table>
       </div>
       
       <div id="div_breakout_part_year_RET" runat="server"> 
       <!-- accr type "RET Details" ONLY (4), Breakout of Part Year Cost section FY and FY+1-->
       <table id="tblRETDetails" runat="server" width="100%"  border="1" cellspacing="0" cellpadding="3"> 
            <tr align="center">
                <td  colspan="5"  style="background-color:#d3d3d3" class="lrgBldText">Breakout of Part Year Cost
                </td>
            </tr>  
            <tr class="evenTD" >
                <td class="regBldBlueText">Current Year:</td> 
                <td  colspan="4"><uc1:dropdowngrid ID="ddgCurrFY" Width="80px" runat="server" /></td>
            </tr>
            <tr class="oddTD" style="background-color:#d3d3d3" >
                <td ><span class="lrgBldText" style="width:25%"><asp:Label ID="lblFYRETEscExp" runat="server" CssClass="regBldText"/></span></td>
                <td class="regBldBlueText" style="width:19%">Invoice Date</td>
                <td class="regBldBlueText" style="width:19%">Total of tax Bill</td>
                <td class="regBldBlueText" style="width:19%"># of days reimb</td>
                <td class="regBldBlueText" style="width:18%">Reimb $</td>
            </tr>
            
            <tr  class="evenTD">
                <td ><span class="regBldBlueText" ><asp:Label ID="lbl1stHalfTaxInvFY" runat="server" CssClass="regTextAlignRight"/></span></td>
                <td class="regBldBlueText" ><asp:TextBox ID="txt1stHalfTaxInvFY_DatePeriod"    ToolTip="" MaxLength="30"  Width="130px"   runat="server" CssClass="regText"  /></td>
                <td class="regBldBlueText"><asp:TextBox ID="txt1stHalfFY_TotalTaxBill"   MaxLength="12" onblur="extractNumber(this,2,false);CurrencyFormatted(this)" onkeyup="extractNumber(this,2,false)" onkeypress="return blockNonNumbers(this, event, true, false);"  ToolTip="Total of tax Bill"  Width="130px"   runat="server" CssClass="regTextAlignRight"  /></td>
                <td class="regBldBlueText"><asp:TextBox ID="txt1stHalfFY_NoMonthsReimb"   onkeyup="javascript:this.value=this.value.replace(/[^0-9]/g, '');"  MaxLength="3" ToolTip="# of days reimb"  Width="130px"   runat="server" CssClass="regTextAlignRight"  /></td>
                <td class="regBldBlueText" ><asp:Label ID="lbl1stHalfFY_ReimbAmt" ToolTip="Reimb"  Width="130px"   runat="server" CssClass="regTextAlignRight"/></td>
            </tr>
                   
             <tr  class="oddTD">
                <td><span class="regBldBlueText" ><asp:Label ID="lbl12ndHalfTaxInvFY" runat="server" CssClass="regTextAlignRight"/></span></td>
                <td class="regBldBlueText" ><asp:TextBox ID="txt2ndHalfTaxInvFY_DatePeriod"   ToolTip=""  MaxLength="30"   Width="130px"   runat="server" CssClass="regText"  /></td>
                <td class="regBldBlueText" ><asp:TextBox ID="txt2ndHalfFY_TotalTaxBill"  MaxLength="12"  onblur="extractNumber(this,2,false);CurrencyFormatted(this)" onkeyup="extractNumber(this,2,false);" onkeypress="return blockNonNumbers(this, event, true, false);" ToolTip=""  Width="130px"   runat="server" CssClass="regTextAlignRight"  /></td>
                <td class="regBldBlueText" ><asp:TextBox ID="txt2ndHalfFY_NoMonthsReimb"   ToolTip=""   MaxLength="3" onkeyup="javascript:this.value=this.value.replace(/[^0-9]/g, '');"  Width="130px"   runat="server" CssClass="regTextAlignRight"  /></td>
                <td class="regBldBlueText" ><asp:Label ID="lbl2ndHalfFY_ReimbAmt" runat="server" Width="130px" CssClass="regTextAlignRight"/></td>
            </tr>
            
             <tr class="evenTD" id="tr_RET_1stHalfTaxInvFY1" runat="server">
                <td ><span class="regBldBlueText" ><asp:Label ID="lbl1stHalfTaxInvFY1" ToolTip="RET_DC" runat="server" CssClass="regTextAlignRight"/></span></td>
                
                <td class="regBldBlueText" ><asp:TextBox ID="txt1stHalfTaxInvFY1_DatePeriod" ToolTip=""    MaxLength="30"   Width="130px" onblur="extractNumber(this,2,false);CurrencyFormatted(this);"   onkeyup="Copy('txt1stHalfTaxInvFY1_DatePeriod_','txt1stHalfTaxInvFY1_DatePeriod')" runat="server" CssClass="regText"  /></td>
                <td class="regBldBlueText" ><asp:TextBox ID="txt1stHalfFY1_TotalTaxBill"   MaxLength="12"  onblur="extractNumber(this,2,false);CurrencyFormatted(this);CurrencyFormatted(txt1stHalfFY1_TotalTaxBill_)" onkeyup="extractNumber(this,2,false);Copy('txt1stHalfFY1_TotalTaxBill_','txt1stHalfFY1_TotalTaxBill')" onkeypress="return blockNonNumbers(this, event, true, false);" ToolTip=""  Width="130px"   runat="server" CssClass="regTextAlignRight"  /></td>
                <td class="regBldBlueText" ><asp:TextBox ID="txt1stHalfFY1_NoMonthsReimb"  ToolTip=""  MaxLength="3" onblur="extractNumber(this,2,false);" onkeyup="javascript:this.value=this.value.replace(/[^0-9]/g, '');Copy('txt1stHalfFY1_NoMonthsReimb_','txt1stHalfFY1_NoMonthsReimb')"  Width="130px"   runat="server" CssClass="regTextAlignRight"  /></td>
                
                <td class="regBldBlueText" ><asp:Label ID="lbl1stHalfFY1_ReimbAmt"  runat="server"  Width="130px"  CssClass="regTextAlignRight"/></td>
             </tr>
            
             <tr class="oddTD">
                <td >&nbsp;<span class="lrgBldText" >Tax Bill Receipt:</span></td>
                <td class="regBldBlueText" >&nbsp;</td>
                <td class="regBldBlueText" >&nbsp;<asp:Label ID="lblFY_TaxBillReceipt_Total" Width="130px"  runat="server" CssClass="regBldBlueTextRight"/></td>
                <td class="regBldBlueText" >&nbsp;<asp:Label ID="lblFY_TaxBillReceipt_NoMonthReimb" Width="130px"   runat="server" CssClass="regBldBlueTextRight"/></td>
                <td class="regBldBlueText" >&nbsp;<asp:Label ID="lblFY_TaxBillReceipt_Reimb"  runat="server"  Width="130px" CssClass="regBldBlueTextRight"/></td>
            </tr>
            
            <tr class="evenTD" >
                <td >&nbsp;<span class="regBldBlueText" >Tax Base Year:</span></td>
                <td class="regBldBlueText" >&nbsp;</td>
                <td class="regBldBlueText" >&nbsp;</td>
                <td class="regBldBlueText" >&nbsp;</td>
                <td class="regBldBlueText" ><asp:TextBox ID="txtFY_TaxBaseYear"  MaxLength="12"  onblur="extractNumber(this,2,true);CurrencyFormatted(this)" onkeyup="extractNumber(this,2,true);" onkeypress="return blockNonNumbers(this, event, true, true);" ToolTip=""  Width="130px" runat="server" CssClass="regTextAlignRight"  /></td>
            </tr>
            
             <tr  class="oddTD">
                <td>&nbsp;<span class="regBldBlueText" >Net Amount (Invoice minus Base Year):</span></td>
                <td class="regBldBlueText" >&nbsp;</td>
                <td class="regBldBlueText" >&nbsp;</td>
                <td class="regBldBlueText" >&nbsp;</td>
                <td class="regBldBlueText" >&nbsp;<asp:Label ID="lblFY_NetAmountReimb" Width="130px"  runat="server" CssClass="regTextAlignRight"/></td>
            </tr>
            
             <tr class="evenTD">
                <td >&nbsp;<span class="regBldBlueText" >% Of Government Occupancy:</span></td>
                <td class="regBldBlueText" >&nbsp;</td>
                <td class="regBldBlueText" >&nbsp;</td>
                <td class="regBldBlueText" >&nbsp;</td>
                <td class="regBldBlueText" ><asp:TextBox ID="txtFY_PercGovOccupReimb"  MaxLength="3" onblur="extractNumber(this,3,false);CurrencyFormatted(this)" onkeyup="extractNumber(this,3,false);" onkeypress="return blockNonNumbers(this, event, true, false);"  ToolTip=""  Width="130px"   runat="server" CssClass="regTextAlignRight"  /></td>
            </tr>  
            
             <tr  class="oddTD">
                <td class="oddTD"><span class="lrgBldText" >Amount Due Lessor:</span></td>
                <td class="regBldBlueText" >&nbsp;</td>
                <td class="regBldBlueText" >&nbsp;</td>
                <td class="regBldBlueText" >&nbsp;</td>
                <td class="regBldBlueText" >&nbsp;<asp:Label ID="lblFY_AmtDueLessorReimb" Width="130px"   runat="server" CssClass="regTextAlignRight"/></td>
            </tr>
            
            <tr>
                <td class="regBldBlueText">Current Year + 1:</td> 
                <td colspan="4">&nbsp;<asp:Label ID="lblFY1" ToolTip = "Fiscal Year"  runat="server" CssClass="regTextAlignRight"/></td>

            </tr>
            
            <tr style="background-color:#d3d3d3" >
                <td ><span class="lrgBldText"><asp:Label ID="lblFY1RETEscExp" runat="server" CssClass="regBldText"/></span></td>
                <td class="regBldBlueText">Invoice Date</td>
                <td class="regBldBlueText">Total of tax Bill</td>
                <td class="regBldBlueText">
                    # of days reimb</td>
                <td class="regBldBlueText">Reimb $</td>
            </tr>
            
            <tr class="oddTD">
                <td ><span class="regBldBlueText" ><asp:Label ID="lbl1stHalfTaxInvFY1_" runat="server"  CssClass="regTextAlignRight"/></span></td>
                
                <td class="regBldBlueText" ><asp:TextBox ID="txt1stHalfTaxInvFY1_DatePeriod_" ToolTip=""    MaxLength="30" onkeyup="Copy('txt1stHalfTaxInvFY1_DatePeriod','txt1stHalfTaxInvFY1_DatePeriod_')"   Width="130px"   runat="server" CssClass="regText"  /></td>
                <td class="regBldBlueText" ><asp:TextBox ID="txt1stHalfFY1_TotalTaxBill_"  ToolTip=""    MaxLength="12" onblur="extractNumber(this,2,false);CurrencyFormatted(this);CurrencyFormatted(txt1stHalfFY1_TotalTaxBill)" onkeyup="extractNumber(this,2,false);Copy('txt1stHalfFY1_TotalTaxBill', 'txt1stHalfFY1_TotalTaxBill_')" onkeypress="return blockNonNumbers(this, event, true, false);"  Width="130px"   runat="server" CssClass="regTextAlignRight"  /></td>
                <td class="regBldBlueText" ><asp:TextBox ID="txt1stHalfFY1_NoMonthsReimb_"   MaxLength="3" ToolTip="" onkeyup="javascript:this.value=this.value.replace(/[^0-9]/g, '');Copy('txt1stHalfFY1_NoMonthsReimb', 'txt1stHalfFY1_NoMonthsReimb_')"  Width="130px"   runat="server" CssClass="regTextAlignRight"  /></td>
                
                <td class="regBldBlueText" >&nbsp;<asp:Label ID="lbl1stHalfFY1_ReimbAmt_"  runat="server" width="130px"  CssClass="regTextAlignRight"/></td>
            </tr>
            
             <tr class="evenTD">
                <td ><span class="regBldBlueText" ><asp:Label ID="lbl12ndHalfTaxInvFY1" runat="server" CssClass="regTextAlignRight"/></span></td>
                <td class="regBldBlueText" ><asp:TextBox ID="txt2ndHalfTaxInvFY1_DatePeriod" ToolTip="" MaxLength="30"   Width="130px"   runat="server" CssClass="regText"  /></td>
                <td class="regBldBlueText" ><asp:TextBox ID="txt2ndHalfFY1_TotalTaxBill"   MaxLength="12" onblur="extractNumber(this,2,false);CurrencyFormatted(this)" onkeyup="extractNumber(this,2,false);" onkeypress="return blockNonNumbers(this, event, true, false);" ToolTip=""  Width="130px"   runat="server" CssClass="regTextAlignRight"  /></td>
                <td class="regBldBlueText" ><asp:TextBox ID="txt2ndHalfFY1_NoMonthsReimb"  MaxLength="3"  onkeyup="javascript:this.value=this.value.replace(/[^0-9]/g, '');" ToolTip=""  Width="130px"   runat="server" CssClass="regTextAlignRight"  /></td>
                <td class="regBldBlueText" >&nbsp;<asp:Label ID="lbl2ndHalfFY1_ReimbAmt"  width="130px"    MaxLength="12" runat="server" CssClass="regTextAlignRight"/></td>
            </tr>
            
             <tr class="oddTD"  id="tr_RET_lbl1stHalfTaxInvFY2" runat="server">
                <td ><span class="regBldBlueText" ><asp:Label ID="lbl1stHalfTaxInvFY2" ToolTip="RET_DC_2" runat="server" CssClass="regTextAlignRight"/></span></td>
                <td class="regBldBlueText" ><asp:TextBox ID="txt1stHalfTaxInvFY2_DatePeriod" ToolTip=""  Width="130px"  MaxLength="30"   runat="server" CssClass="regText"  /></td>
                <td class="regBldBlueText" ><asp:TextBox ID="txt1stHalfFY2_TotalTaxBill" ToolTip=""  MaxLength="12"  onblur="extractNumber(this,2,false);CurrencyFormatted(this)" onkeyup="extractNumber(this,2,false);" onkeypress="return blockNonNumbers(this, event, true, false);"  Width="130px"   runat="server" CssClass="regTextAlignRight"  /></td>
                <td class="regBldBlueText" ><asp:TextBox ID="txt1stHalfFY2_NoMonthsReimb"  MaxLength="3"  onkeyup="javascript:this.value=this.value.replace(/[^0-9]/g, '');" ToolTip=""  Width="130px"   runat="server" CssClass="regTextAlignRight"  /></td>
                <td class="regBldBlueText" >&nbsp;<asp:Label ID="lbl1stHalfFY2_ReimbAmt"  width="130px"   runat="server" CssClass="regTextAlignRight"/></td>
            </tr>
            
             <tr class="evenTD">
                <td >&nbsp;<span class="lrgBldText" >Tax Bill Receipt:</span></td>
                <td class="regBldBlueText" >&nbsp;</td>
                <td class="regBldBlueText" >&nbsp;<asp:Label ID="lblFY1_TaxBillReceipt_Total"  runat="server" Width="130px" CssClass="regBldBlueTextRight"/></td>
                <td class="regBldBlueText" >&nbsp;<asp:Label ID="lblFY1_TaxBillReceipt_NoMonthReimb"  runat="server"  Width="130px" CssClass="regBldBlueTextRight"/></td>
                <td class="regBldBlueText" >&nbsp;<asp:Label ID="lblFY1_TaxBillReceipt_Reimb"  runat="server"  Width="130px" CssClass="regBldBlueTextRight"/></td>
            </tr>
            
            <tr class="oddTD">
                <td >&nbsp;<span class="regBldBlueText" >Tax Base Year:</span></td>
                <td class="regBldBlueText" >&nbsp;</td>
                <td class="regBldBlueText" >&nbsp;</td>
                <td class="regBldBlueText" >&nbsp;</td>
                <td class="regBldBlueText" ><asp:TextBox ID="txtFY1_TaxBaseYear" MaxLength="12" onblur="extractNumber(this,2,true);CurrencyFormatted(this)" onkeyup="extractNumber(this,2,true);" onkeypress="return blockNonNumbers(this, event, true, true);" ToolTip=""  Width="130px"    runat="server" CssClass="regTextAlignRight"  /></td>
            </tr>
            
             <tr class="evenTD">
                <td >&nbsp;<span class="regBldBlueText" >Net Amount (Invoice minus Base Year):</span></td>
                <td class="regBldBlueText" >&nbsp;</td>
                <td class="regBldBlueText" >&nbsp;</td>
                <td class="regBldBlueText" >&nbsp;</td>
                <td class="regBldBlueText" >&nbsp;<asp:Label ID="lblFY1_NetAmountReimb"  width="130px"  runat="server" CssClass="regTextAlignRight"/></td>
            </tr>
            
             <tr class="oddTD">
                <td >&nbsp;<span class="regBldBlueText" >% Of Government Occupancy:</span></td>
                <td class="regBldBlueText" >&nbsp;</td>
                <td class="regBldBlueText" >&nbsp;</td>
                <td class="regBldBlueText" >&nbsp;</td>
                <td class="regBldBlueText" ><asp:TextBox ID="txtFY1_PercGovOccup"  MaxLength="3" ToolTip=""  onblur="extractNumber(this,3,false);" onkeyup="extractNumber(this,3,false);" onkeypress="return blockNonNumbers(this, event, true, false);"  Width="130px"    runat="server" CssClass="regTextAlignRight"  /></td>
            </tr>
            
             <tr class="evenTD">
                <td >&nbsp;<span class="lrgBldText" >Amount Due Lessor:</span></td>
                <td class="regBldBlueText" >&nbsp;</td>
                <td class="regBldBlueText" >&nbsp;</td>
                <td class="regBldBlueText" >&nbsp;</td>
                <td class="regBldBlueText" >&nbsp;<asp:Label ID="lblFY1_AmtDueLessor"  width="130px"  runat="server" CssClass="regTextAlignRight"/></td>
            </tr>
            
            <tr>
                <td  colspan="5">&nbsp;<span class="lrgBldText" ></span></td>
            </tr>
            
             <tr style="background-color:#d3d3d3" >
                <td >&nbsp;</td>
                <td class="regBldBlueText">&nbsp;</td>
                <td class="regBldBlueText">Accrual</td>
                <td class="regBldBlueText">% Increase></td>
                <td class="regBldBlueText">Net Increase</td>
                
                
            </tr>
            
            <tr>
                <td >&nbsp;<span class="lrgBldText" ><asp:Label ID="lblFY2_RETEscPtojTitile"  runat="server" CssClass="regTextAlignRight"/></span></td>
                <td class="regBldBlueText" >&nbsp;</td>
                <td class="regBldBlueText" >&nbsp;<asp:Label   ID="txtFY2_RETEscProjAccrual"   ToolTip="Calculated Value"  Width="130px"   runat="server" CssClass="regBldBlueTextRight"  /></td>
                <td class="regBldBlueText" ><asp:Label ID="txtFY2_RETEscProjPercIncrease"  ToolTip="Calculated Value"  Width="130px"   runat="server" CssClass="regBldBlueTextRight"  /></td>
                <td class="regBldBlueText" >&nbsp;<asp:Label ID="lblFY2_RETEscProjNetIncrease" ToolTip="Net Increase" onblur="extractNumber(this,3,false);" onkeyup="extractNumber(this,3,false);" onkeypress="return blockNonNumbers(this, event, true, false);"  Width="130px"  runat="server" CssClass="regBldBlueTextRight"/></td>
            </tr>
            
             <tr>
                <td  >&nbsp;<span class="lrgBldText" ><asp:Label ID="lblFY2_RETEscPtojRevTitile"  runat="server" CssClass="regTextAlignRight"/></span></td>
                <td class="regBldBlueText"  >&nbsp;</td>
                
                 <td class="regBldBlueText"  ><asp:TextBox ID="txtFY2_RETEscProjAccrualRev"  MaxLength="12"  onblur="extractNumber(this,2,false);CurrencyFormatted(this)" onkeyup="extractNumber(this,2,false)" onkeypress="return blockNonNumbers(this, event, true, false);"  Width="130px"  runat="server" CssClass="regBldBlueTextRight"/></td>
                 <td class="regBldBlueText"  ><asp:TextBox ID="txtFY2_RETEscProjPercIncreaseRev"  MaxLength="12"  ToolTip=""  Width="130px"  onblur="extractNumber(this,2,false);CurrencyFormatted(this)" onkeyup="extractNumber(this,2,false)" onkeypress="return blockNonNumbers(this, event, true, false);"  runat="server" CssClass="regBldBlueTextRight"  /></td>             
                 <td class="regBldBlueText"  ><asp:TextBox ID="lblFY2_RETEscProjNetIncreaseRev"  MaxLength="12"  onblur="extractNumber(this,2,false);" onkeyup="extractNumber(this,2,false);CurrencyFormatted(this)" onkeypress="return blockNonNumbers(this, event, true, false);"  Width="130px"  runat="server" CssClass="regBldBlueTextRight"/></td>
                
            </tr>
                        
       </table>
       </div>
       
       
        <hr />
       
       <div id="div_Reviewer_Reason_Code" runat="server" class="section">
        <table width="100%" border="1" cellspacing="0" cellpadding="3" id="Table1"  runat="server"> 
            <tr class="evenTD">
                <td  style="width:25%"><span class="lrgBldText">Team Code:</span></td>
                <td  style="width:75%"  colspan="3">
                    <uc1:dropdowngrid ID="ddgTeamCode" runat="server"  Width="100px"/>
                </td>
                
            </tr>
            
            <tr class="evenTD">
                <td  style="width:25%"><span class="lrgBldText">Reviewer Reason Code:</span>
               </td>
                <td  style="width:75%"  colspan="3">
                    <uc1:dropdowngrid ID="ddgReasonCode"  OnChange="ValidInvalid();"   runat="server" Width="400px" />
                </td>
                
            </tr>
            
            <tr class="evenTD" id="div_tr_reason_for_delay" runat="server">
                <td  style="width:25%"><span class="lrgBldText">Reason For Delay:</span></td>
                <td  style="width:75%"  colspan="3">
                    <table width="100%" >
                        <tr valign="middle">
                            <td style="width:30%; text-align:left">
                                <uc1:dropdowngrid ID="ddgReasonForDelay" OnChange="HideOther()"  runat="server" Width="200px" />
                            </td>
                            <td  style="width:80%; text-align:left" >
                                
                            </td>
                        </tr>
                        
                        <tr id="tr_other_reason_for_delay" runat="server">
                            <td colspan="2">Description(s):&nbsp;
                                <asp:TextBox ID="txtOtherReasonForDelay"   Width="70%" MaxLength="200"  runat="server" CssClass="regText"/></td>
                        </tr>
                        
                    </table>
                </td>
                
            </tr>
            
            <tr class="evenTD">
                <td  style="width:25%"><span class="lrgBldText">Remarks: </span><br />(mandatory entry if the Reviewer Reason Code is valid)
                </td>
                <td  colspan="3" style="width:75%">
                    <asp:TextBox ID="txtRemarks" Width="100%" Rows="4" runat="server" MaxLength="5000" TextMode="MultiLine"></asp:TextBox>
               </td>
            </tr>
        </table>
        </div> 
        <table width="100%" border="1" cellspacing="0" cellpadding="4"   runat="server">
            <tr class="oddTD">
                <td class="lrgBldWhiteText">Title:</td>
                <td class="lrgBldWhiteText">Name:</td>
                <td class="lrgBldWhiteText">Date:</td>
            </tr>
            <tr class="oddTD">
                <td valign="middle" class="regBldBlueText" style="width:20%">REA Budget Analyst</td>
                <td valign="middle" class="regBldBlueText" style="width:40%"><asp:TextBox ID="txtREABudgetAnalyst" Width="100%"  MaxLength="60" ToolTip="Enter REA Budget Analyst Full Name"  runat="server" CssClass="regText"/></td>
                <td valign="middle" class="regBldBlueText" style="width:40%;"><uc:GetDate ID="calREABudgetAnalystDate" runat="server" /></td>
            </tr>
           
            <tr class="oddTD">
                <td valign="middle" class="regBldBlueText">CFO Budget Analyst</td>
                <td  valign="middle" class="regBldBlueText"><asp:TextBox ID="txtCFOBudgetAnalyst"  Width="100%"   MaxLength="60" ToolTip="Enter CFO Budget Analyst Full Name"  runat="server" CssClass="regText"/></td>
                <td  valign="middle" class="regBldBlueText"><uc:GetDate ID="calCFOBudgetAnalystDate" runat="server" /></td>
            </tr>
        </table>
                  
    <hr />
    
    
    
    <hr class="thickHR" />
    
    <table  id="tbl_btns" runat="server" width="100%" >
        <tr>
            <td class="lrgBldText">
                <span style="color: red">Attention: </span>
                To reconfirm data based on your input, click "Recalculate"; this allows you to review the form prior to submittal.<br />Click "Save" when you are ready to submit. The "Save"
                button automatically recalculates all fields.            
            </td>
        </tr>
    
        <tr align="right">
            <td align="right" >
                <input type="button" runat="server" id="btnCancel" value="<< Back" title="Back to the Items List" class="button" onclick="javascript:on_cancel();"/>&nbsp;&nbsp;
                <input type="button" id="btnHistory" value="History" title="Open Item History"   class="button" onclick="javascript:open_history();"/>&nbsp;&nbsp;
                <asp:Button ID="btnRecalculate" runat="server" Text="Recalculate" ToolTip="Recalculate field values" CssClass="button"  OnClick="btnRecalculate_Click"/>
                <asp:Button ID="btnSave" runat="server" Text="Save" ToolTip="Recalculate field values and save the form" CssClass="button" OnClick="btnSave_Click"/>
            </td>
        </tr>
        
    </table>
</asp:Content>
