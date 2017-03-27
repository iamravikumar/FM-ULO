<%@ Control Language="c#" Inherits="Controls.MultiGrid" Codebehind="MultiGrid.ascx.cs" %>
<div id="ArtMM" visible="false" runat="server" class="dgBorder" onkeyup="C508_(event,this)" onkeydown="return C508(event,this)" style="outline:none" hideFocus="hideFocus" tabindex="0">
	<asp:textbox id="dgIndex" CssClass="Art" style="DISPLAY:none" runat="server"/>
	<table id="ArtDG" class="tblGrid" style="DISPLAY:none;width:100%;border-style:none" cellspacing="1" cellpadding="0" runat="server"
    	    onclick="dgm_click(event)" onmouseover="row_In(event)" onmouseout="row_Out(event)">
		<tr class="GridHeader" style="POSITION:relative;TOP:expression(_up(this,3).scrollTop)"/>
	</table>
	<input style="DISPLAY:none" id="btnOnCnange" type="button" runat="server"/>
</div>
<div style="DISPLAY:none;font-size:1px">.</div>
<div id="DesignView" class="GridHeader" runat="server">User Control "MultiGrid"</div>
