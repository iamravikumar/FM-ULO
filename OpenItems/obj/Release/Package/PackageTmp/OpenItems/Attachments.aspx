<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Popups/PopupEnlarged.master" AutoEventWireup="true" Inherits="GSA.OpenItems.Web.Attachments" Codebehind="Attachments.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="title" runat="Server">
    Open Item - Attachments
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="head" runat="Server">
    <!--#include virtual="../include/HTTPService.js" -->
    <!--#include virtual="include/Attachments.js" -->
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="contentTitle" runat="Server">
    <table width="100%">
        <tr valign="top">
            <td>Open Item Attachments.</td>
            <td><span class="regBldBlueText">DocNumber : </span></td>
            <td>
                <asp:Label ID="lblDocNumber" runat="server" CssClass="lrgBldGrayText" /></td>
            <td><span class="regBldBlueText">Reviewer : </span></td>
            <td id="tdReviewer" runat="server" class="lrgBldGrayText"></td>
        </tr>
    </table>
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="content" runat="Server">
      <div class="sectionTitle">
                 
        <table width="100%" >
            <tr valign="top" >
                <td>Open Item Attachments.</td>
                <td><span class="regBldBlueText" >DocNumber : </span></td>
                <td><asp:Label ID="Label1" runat="server" CssClass="lrgBldGrayText" /></td>
                <td><span class="regBldBlueText" >Reviewer : </span></td>
                <td id="td1" runat="server" class="lrgBldGrayText" ></td>                
            </tr>            
        </table>
        </div>                                            
        
        <table id="tblInfo" width="100%" runat="server" class="greyBorder">
            <tr class="TDeven"><td colspan="2" align="left" class="regBldBlueText" style="height:25px;">
            Based on the Pegasys document type (<asp:Label ID="lblPegasysDocType" runat="server" CssClass="lrgBldGrayText" /> - the first two letters of the Pegasys Document Number) please provide the following documentation
            <asp:Label ID="lblPegasysDocTypeNote" runat="server" CssClass="regBlackText" /></td></tr>            
            <tr >
                <td style="width:50%"><span class="regBldBlueText">To Support Total Order Balances:</span></td>
                <td style="width:50%"><span class="regBldBlueText">To Support Delivered Order Balance:</span></td>    
            </tr>
            <tr >
                <td><asp:Label ID="lblRequiredForTotal" runat="server" CssClass="regText" Text="" /></td>
                <td><asp:Label ID="lblRequiredForUDO" runat="server" CssClass="regText" Text="" /></td>
            </tr>
        </table>
        
        <p />
        <table>
            <tr>
                <td></td>
            </tr>
        </table>
        
        <table width="100%" cellpadding="10" class="greyBorder">
            <tr>
                <td style="width: 70%;" valign="top" class="">
                    <span class="lrgBldText" >Document List</span>
                    <input type="hidden" runat="server" id="txtDocID" value="" />
                    <input type="hidden" runat="server" id="txtPropertyArray" value="" />
                    <input type="hidden" runat="server" id="txtDisplayFlag" value="" />
                    <input type="hidden" runat="server" id="txtReloadOpener" value="" />
                    
                    <div id="divDocs" >
                    
                    <asp:GridView ID="gvDocs" runat="server" CellPadding="0" AllowPaging="false" AutoGenerateColumns="false"
                                AllowSorting="false" CssClass="" Width="100%" 
				                CellSpacing="0" EnableViewState="false" HeaderStyle-CssClass="th"
				                UseAccessibleHeader="true" RowStyle-CssClass="TDeven" AlternatingRowStyle-CssClass="TDodd" >
                        <Columns>    
                            <asp:TemplateField HeaderText="Edit" ItemStyle-Width="15" ItemStyle-HorizontalAlign="Center" >
                                <ItemTemplate>
                                    <input type="image" id="btnEdit" title="edit attachment" runat="server" src="../images/btn_edit.gif" />                                    
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Send To CO" ItemStyle-Width="50" ItemStyle-HorizontalAlign="Center">
                                <ItemTemplate>                
                                    <input type="checkbox" id="chkEmail" runat="server" title="Select Document to Send To Central Office" />
                                </ItemTemplate>
                            </asp:TemplateField>                                               
                            <asp:BoundField DataField="LastEmailDate" HeaderText="Last Sent To CO" DataFormatString="{0:MM/dd/yyyy}" HtmlEncode="false"/>  
                            
                            <asp:TemplateField HeaderText="Send To SME" ItemStyle-Width="50" ItemStyle-HorizontalAlign="Center">
                                <ItemTemplate>                
                                    <input type="checkbox" id="chkRevEmail" runat="server" title="Select Document to Send To Subject Matter Expert" />
                                </ItemTemplate>
                            </asp:TemplateField>                                               
                            <asp:BoundField DataField="LastRevisionEmail" HeaderText="Last Sent To SME" DataFormatString="{0:MM/dd/yyyy}" HtmlEncode="false"/>
                            <asp:TemplateField HeaderText="Approved By SME" ItemStyle-Width="50" ItemStyle-HorizontalAlign="Center">
                                <ItemTemplate>                
                                    <input type="checkbox" id="chkApproved" runat="server" title="Document Approved By Subject Matter Expert" />
                                </ItemTemplate>
                            </asp:TemplateField> 
                                
                            <asp:TemplateField HeaderText="File Name" >
                                <ItemTemplate>                
                                    <a id="lnkView" runat="server" href="" ><img id="imgIcon" src="../images/btn_view_file.gif" alt="" title='view file' style="border:0;vertical-align:middle"/><asp:Label ID="lblDocName" runat="server" CssClass="regBldGreyText" Text="" /></a>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="UploadDate" HeaderText="Uploaded" DataFormatString="{0:MM/dd/yyyy}" HtmlEncode="false"/>
                            <asp:BoundField DataField="Organization" HeaderText="ORG" ItemStyle-HorizontalAlign="center"  />
                            <asp:BoundField DataField="DocTypeName" HeaderText="Document Type" />
                        </Columns>
                    </asp:GridView>
                    
                    </div>
                    
                    <p />
                                                         
                    <asp:FileUpload ID="ctrlUpload" runat="server" Width="350px" />
                    <asp:Button ID="btnUpload" runat="server" Text ="Add Attachment" ToolTip ="Add New Attachment" CssClass="button" />  
                    <br /> <p />
                     <asp:Label ID="Label2" runat="server" CssClass="regBldBlueText" Text="Files with 'doc', 'xls', 'txt', 'pdf', 'jpeg', 'gif' or 'bmp' extensions are compatible with the ULO system."></asp:Label>
                    <br /><br />
                    <asp:Label ID="lblNote"  CssClass="regBldBlueText" runat="server" Text="Please don't upload files with the following extentions:<br>'docx', 'xlsx', 'tif', 'tiff', 'zip'."></asp:Label>                                             
                   
                    </td>
                
                <td style="width: 30%" valign="top" class="">
            
                    <table id="tblDocProperties" width="100%" style="height:100%" cellspacing="6" >
                        <tr><td colspan="2" align="left" ><span class="lrgBldText" >File/Document Properties</span></td></tr>
                        <tr><td colspan="2" ><p /></td></tr>
                        <tr>
                            <td><span class="regBldBlueText">File Name: </span></td>
                            <td><asp:Label ID="lblFileName" runat="server" CssClass="regText" Text="" /></td>
                        </tr>
                        <tr>
                            <td><span class="regBldBlueText">Upload Date: </span></td>
                            <td><asp:Label ID="lblUploadDate" runat="server" CssClass="regText" Text="" /></td>
                        </tr>
                        <tr>
                            <td><span class="regBldBlueText">Uploaded By: </span></td>
                            <td><asp:Label ID="lblUploadUser" runat="server" CssClass="regText" Text="" /></td>
                        </tr>
                        <tr>
                            <td><span class="regBldBlueText">Document Type: </span></td>
                            <td><asp:Label ID="lblDocType" runat="server" CssClass="regText" Text="" />
                                <asp:ListBox ID="lstDocTypes" runat="server" CssClass="regText" Rows="9" />
                            </td>
                        </tr>                        
                        <tr>
                            <td><span class="regBldBlueText">Associated LineNum: </span></td>
                            <td><asp:Label ID="lblLineNum" runat="server" CssClass="regText" Text="" />
                                <asp:TextBox ID="txtLineNum" runat="server" CssClass="regText" MaxLength="100" />                                
                            </td>
                        </tr>
                        <tr><td ><span class="regBldBlueText">Comments: </span></td>
                            <td >
                                <asp:Label ID="lblComments" runat="server" CssClass="regText" />
                                <asp:TextBox ID="txtComments" runat="server" CssClass="regText" TextMode="MultiLine" Rows="3" Columns="45" />
                            </td>
                        </tr>
                        <tr>
                            <td><asp:Label ID="lblEmailDateLabel" runat="server" CssClass="regBldBlueText" Text="Last Sent Email To CO: " /></td>
                            <td><asp:Label ID="lblEmailDate" runat="server" CssClass="regText" /></td>
                        </tr>                        
                        
                        <tr><td colspan="2" align="right" valign="bottom" >
                            <asp:Button ID="btnSave" runat="server" Text="Save" CssClass="button" ToolTip="Save Document Properties"  />
                            &nbsp;&nbsp;
                            <asp:Button ID="btnDelete" runat="server" Text="Delete" CssClass="button" ToolTip="Delete Attachment" BorderColor="Red" />
                            &nbsp;&nbsp;       
                            <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="button" ToolTip="Cancel" />                            
                        </td></tr>
                    </table>
                    
                </td>
            </tr>                          
        </table>
                
        <asp:Label ID="lblMessage" runat="server" Visible="false" Text="" CssClass="errorsum" Width="400"/>
        
        <br />
        <table width="100%" >
            <tr>
                <td align="right" valign="bottom" >        
                    <input type="button" id="btnHistory" value="History" title="Attachments History" class="button" onclick="javascript:open_history();"/>&nbsp;&nbsp;            
                    <input type="button" id="btnSendSMEEmail" runat="server" value="Send Email to SME" onclick="javascript:send_email('sme');" class="button" title="Send Email to Subject Matter Expert"/>&nbsp;&nbsp;
                    <input type="button" id="btnSendCOEmail" runat="server" value="Send Email to Central Office" onclick="javascript:send_email('co');" class="button" title="Send Email to Central Office"/>&nbsp;&nbsp;
                    <input type="button" id="btnClose" value="Close" onclick="javascript:window_close();" class="button" title="Close"/>                
                </td>
            </tr>
        </table>
    
</asp:Content>

