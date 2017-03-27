<%@ Control Language="c#" Inherits="Controls.GetDate" Codebehind="GetDate.ascx.cs" %>
<%@ Register TagPrefix="uc1" TagName="DropDownGrid" Src="DropDownGrid.ascx" %>
<div id="ArtCC" style="width:80px; font-size:10px" runat="server">
	<div id="clndr" visible="false" style="DISPLAY:none;Z-INDEX:99;POSITION:absolute" runat="server" onkeydown="return Cal508(event)">
		<table id="ArtDT" class="tGetDate" cellspacing="1" cellpadding="0" onmouseover="cal_over(event)" onclick="cal_click(event)" onmouseout="cal_out(event)" runat="server">
			<tr><td colspan="3" align="left"><uc1:dropdowngrid id="ddgMonth" CellSpacing="0" TblBorderClass="CalBorder" TblCSSClass="tblCal" ItemCSSClass="ArtCal" runat="server" ShowHeader="false" OnChange="showCal(_up(this,10))"/></td>
				<td colspan="3" align="left"><uc1:dropdowngrid id="ddgYear" CellSpacing="0" TblBorderClass="CalBorder" TblCSSClass="tblCal" ItemCSSClass="ArtCal" runat="server" ShowHeader="false" OnChange="showCal(_up(this,10))"/></td>
				<td><input id="btnClose" type="button" class="bttn" value="&Chi;" title="Close" onclick="cal_close(this)" style="height:17px;font:bold 10px Microsoft Sans Serif" /></td></tr>
			<tr class="weekdays">
				<td style="width:14%">Su</td><td style="width:14%">Mn</td><td style="width:14%">Tu</td><td style="width:14%">Wd</td><td style="width:14%">Th</td><td style="width:14%">Fr</td><td style="width:14%">Sa</td></tr>
			<tr class="dayEmpty"><td>0</td><td>0</td><td>0</td><td>0</td><td>0</td><td>0</td><td>1</td></tr>
			<tr><td>1</td><td>1</td><td>1</td><td>1</td><td>1</td><td>1</td><td>1</td></tr>
			<tr><td>1</td><td>1</td><td>1</td><td>1</td><td>1</td><td>1</td><td>1</td></tr>
			<tr><td>1</td><td>1</td><td>1</td><td>1</td><td>1</td><td>1</td><td>1</td></tr>
			<tr class="dayEmpty"><td>0</td><td>0</td><td>0</td><td>0</td><td>0</td><td>0</td><td>0</td></tr>
			<tr class="dayEmpty">
				<td>0</td><td>0</td><td>&nbsp</td><td>&nbsp</td><td>&nbsp</td>
				<td colspan="2"><input id="btnClear" type="button" title="Reset date" value="CLEAR" class="bttn" style="height:17px;font:10px Arial" onclick="cal_c(_up(this,5),'')" runat="server"/></td></tr>
		</table>
		<asp:textbox id="day" style="DISPLAY:none" runat="server"/>
	</div>
	<div style="white-space:nowrap">
		<a id="tdDate" runat="server" onkeydown="CalC508(event)" onclick="expCal(event)" style="vertical-align:bottom">
		    <asp:textbox id="lDate" CssClass="date" onfocus="_altCSS(this,'Focus')" runat="server"/></a>
		<img id="tdBttn"  src="btnExpCal.gif" style="vertical-align:top;" alt="" runat="server" onclick="expCal(event)" />
		<input id="btnOnCnange" visible="false" style="DISPLAY: none" type="button" runat="server"/>
	</div>
</div>
