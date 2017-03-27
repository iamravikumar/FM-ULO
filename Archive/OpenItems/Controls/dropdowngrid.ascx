<%@ Control Language="c#" Inherits="Controls.DropDownGrid" Codebehind="DropDownGrid.ascx.cs" %>
<table id="ArtDD" visible="false" class="dgBorder" style="width:100%;display:none;outline:none" tabindex="0" hideFocus="hideFocus"
        onkeyup="C508_(event,this)" onkeydown="return C508(event,this)" cellspacing="0" cellpadding="0" runat="server">
	<tr><td valign="top" style="width:100%">
            <div style="POSITION:absolute;display:none;z-index:98"><asp:textbox id="dgIndex" CssClass="Art" style="DISPLAY:none" runat="server"/>
			     <table id="ArtDG" class="tblGrid" runat="server" style="width:100%;border-style:none" title="Click to select" cellspacing="1"
			            onclick="ddg_click(event)" onmouseout="row_Out(event)" onmouseover="row_In(event)">
				    <tr class="GridHeader" style="POSITION:relative;TOP:expression(_up(this,3).scrollTop)"/>
				 </table>
				 <input style="DISPLAY: none" id="btnOnCnange" type="button" runat="server" />
			</div>
			<div><table id="ArtDG2" runat="server" style="width:100%;border-style:none" onclick="ddg_click(event)">
			        <tr></tr>
			        <tr></tr>
				 </table>
			</div>
		</td>
		<td id="tdExp" runat="server" valign="bottom">
			<img id="img" src="btnExpDDG.bmp" style="cursor:pointer;vertical-align:bottom" alt="Click to expand" onclick="ddgExp(event)" runat="server" />
		</td>
	</tr>
</table>
<div id="DesignView" class="GridHeader" runat="server">User Control "DropDownGrid"</div>
