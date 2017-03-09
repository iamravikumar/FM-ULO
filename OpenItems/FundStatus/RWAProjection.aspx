<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Popups/PopupEnlarged.master" AutoEventWireup="true" Inherits="GSA.OpenItems.Web.FundStatus_RWAProjection" Codebehind="RWAProjection.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="title" Runat="Server">
    Fund Status RWA Projection
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="head" Runat="Server">
    <link href="../css/fund_status.css" type="text/css" rel="stylesheet" />
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="contentTitle" Runat="Server">
    RWA Projection Distribution
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="content" Runat="Server">
    <div class="section">
    
    <table width="100%" cellspacing="5">
        <tr><td><asp:Label ID="lblMain" runat="server" CssClass="lrgBldGrayText" /></td>                        
        </tr>        
        <tr><td><asp:Label ID="lblPaidDays" runat="server" CssClass="lrgBldGrayText" /></td>                        
        </tr> 
        <tr>
            <td><input type="hidden" id="lblD" runat="server" /><input type="hidden" id="lblDD" runat="server" /></td>
        </tr>
        <tr><td><asp:Label ID="lblError" runat="server" Visible="false" Text="" CssClass="errorsum" />
                <hr />
                <asp:Label ID="lblMessage" runat="server" Text="" CssClass="regBldBlueText" />
            </td>
        </tr>
        <tr><td align="center" >
                <table id="tblData" runat="server" width="420px" cellspacing="1" cellpadding="0" onmouseover="mouse('#DDDDDD')" onmouseout="mouse('')">
                    <tr class="tableCaption">
                        <td >Function</td>
                        <td>YTD Income</td>
                        <td>Projection</td>
                        <td>Over/Under</td>
                    </tr>                                    
                </table>
            </td>
        </tr>        
        <tr><td><table width="330px" align="right">
                    <tr valign="bottom" style="height:35px">
                                              
                        <td >
                            <input type="button" class="button" value="Cancel" title="Cancel" onclick="self.close();"  />
                            <input type="button" id="btnApply" runat="server" onclick="send_rezult();" style="width:200px;" class="button" value="Apply Projection to the end of FY" title="Recalculate Chart" />
                        </td>
                    </tr>                                    
                </table>
            </td>
        </tr>                
    </table>
    </div>
</asp:Content>