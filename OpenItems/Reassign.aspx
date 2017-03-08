<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Popups/Popup.master" AutoEventWireup="true" CodeFile="Reassign.aspx.cs" Inherits="GSA.OpenItems.Web.Reassign" %>
<%@ Register TagPrefix="uc" TagName="UsersOrganizations" Src="~/Controls/OrgUsers.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="title" Runat="Server">
    Open Items - Reassign Reviewer
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="head" Runat="Server">
     <!--#include virtual="../include/HTTPService.js" -->
    <!--#include virtual="include/Reassign.js" -->
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="contentTitle" Runat="Server">
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="content" Runat="Server">        
        <div class="sectionTitle">Reassign Open Item</div>
        <div class="section">
        
        <input type="hidden" id="txtItemID" runat="server" />
        <input type="hidden" id="txtLines" runat="server" value="0" />
        <input type="hidden" id="txtCurrentReviewer" runat="server" />
        <input type="hidden" id="txtRequestID" runat="server" />
        <input type="hidden" id="txtGroupAssign" runat="server" />
        <input type="hidden" id="txtOrgCode" runat="server" />
        
        <asp:Label ID="lblGroupAssign" runat="server" CssClass="regBldText" Visible="false" 
            Text="Group Reassignment for all selected Open Items"/>
        <p />
        
        <table width="100%" cellspacing="4" id="tblLabels" runat="server">
        <tr>
            <td style="width:150px"><label class="regBldText">Document Number: </label></td>
            <td ><asp:Label ID="lblDocNumber" runat="server" CssClass="lrgBldText" /></td>
        </tr>
        <tr>
            <td ><label class="regBldText">Lines: </label></td>
            <td ><asp:Label ID="lblLines" runat="server" CssClass="lrgBldText" /></td>
        </tr>
        <tr>
            <td ><label class="regBldText">Current Organization: </label></td>
            <td ><asp:Label ID="lblOrgCode" runat="server" CssClass="lrgBldText" /></td>
        </tr>
        <tr>
            <td ><label class="regBldText">Current Reviewer: </label></td>
            <td ><asp:Label ID="lblPrevReviewer" runat="server" CssClass="lrgBldText" /></td>
        </tr>
        </table>
        
        <table width="100%" cellspacing="4">
        <tr>
            <td style="width:150px"><label class="regBldText">Assign To: </label></td>
            <td ><asp:DropDownList ID="ddlAssign" runat="server" Width="200px" /></td>
        </tr>
        <tr>
            <td ><label class="regBldText">Comment: </label></td>
            <td >
                <textarea id="txtCommentAssign" rows="3" cols="43" runat="server" ></textarea>
            </td>
        </tr>
        <tr>
            <td colspan="2" align="right">
                <input type="button" id="btnCancelA" value="Cancel" onclick="return on_cancel();" class="button" title="Cancel"/>&nbsp;&nbsp;
                <input type="button" id="btnAssign" runat="server" value="Assign Item" onclick="return on_assign();" class="button" title="Reassign Open Item"/>
            </td>            
        </tr>
        <tr>
            <td colspan="2">
                <asp:Label ID="lblMessage" runat="server" Visible="false" Text="" CssClass="errorsum" Width="400"/>            
            </td>
        </tr>
        </table>                
        </div>
        
        <br />        
        
        <div class="sectionTitle">
            <asp:Label ID="lblRerouteLabel" runat="server" Text="Reroute Open Item to Budget Division" />
        </div>
        <div class="section">
        <asp:Label ID="lblRerouteMsg" runat="server" CssClass="regBldGreyText" Visible="false" />
        <uc:UsersOrganizations ID="ctrlOrgUsers" runat="server" />
        <table width="100%" cellspacing="4" id="tblRerouteCtrls" runat="server" >        
        <tr>
            <td style="width:150px"><label class="regBldText">Comment: </label></td>
            <td>
                <textarea id="txtCommentReroute" rows="3" cols="43" runat="server" ></textarea>
            </td>
        </tr>
        <tr>
            <td colspan="2" align="right">
                <input type="button" id="btnCancelR" value="Cancel" onclick="return on_cancel();" class="button" title="Cancel"/>&nbsp;&nbsp;
                <input type="button" id="btnReroute" value="Send To BD Admin" onclick="return on_reroute();" class="button" title="Reroute Open Item to Budget Division"/>
            </td>
        </tr>
        </table>                
        </div>
                        
</asp:Content>