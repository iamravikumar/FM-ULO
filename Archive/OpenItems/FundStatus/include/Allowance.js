function data_to_excel()
{	var popup=window.open("FundReportExcel.aspx?type=6&FY="+_e("ddlFiscalYear").value, "FundsDataToExcel",
			"width=600,height=600,menubar=no,status=no,resizable=yes,scrollbars=yes,toolbar=yes,top=200,left=200")
	popup.focus()
 return false
}
var month_arr = new Array()
month_arr = "October,November,December,January,February,March,April,May,June,July,August,September".split(',')

function mouse(b)
{	var s=window.event.srcElement
	while(s!=null && s.tagName!="TR") s=s.parentNode;
	if(s==null) return
	if(s.className=="reportRow")
		s.style.backgroundColor = b
}
function _e(o){return document.getElementById(o)}
function _son1(o){o=o.firstChild;return o.nodeType==1?o:o.nextSibling}
function _dwn(o,u){while(u--) o=_son1(o);return o}
function _up(o,u){while(u--) o=o.parentNode;return o}
function extractNumber(o,dp,aN,Max)
{var p=o.value
 var sR0='[0-9]*'
 if(dp>0)
 	sR0+='\\.?[0-9]{0,'+dp+'}';
 else if(dp<0)
	sR0+='\\.?[0-9]*';
 sR0=aN?'^-?'+sR0:'^'+sR0
 sR0=sR0+'$'
 var R0=new RegExp(sR0)
 if(!R0.test(p))
 {	var sR1='[^0-9'+(dp!=0?'.':'')+(aN?'-':'')+']'
	var R1=new RegExp(sR1,'g')
	p=p.replace(R1,'')
	if(aN)
	{	var hasNegative=p.length>0&&p.charAt(0)=='-'
		var R2=/-/g
		p=p.replace(R2,'')
		if(hasNegative)
			p='-'+p
	}
	if(dp!=0)
	{	var R3=/\./g;
		var R3Array=R3.exec(p);
		if(R3Array!=null)
		{	var R3r=p.substring(R3Array.index+R3Array[0].length)
			R3r=R3r.replace(R3,'')
			R3r=dp>0?R3r.substring(0,dp):R3r
			p=p.substring(0,R3Array.index)+'.'+ R3r
		}
	}
 }
 while(Math.abs(p)>Math.abs(Max+1))
	p=parseInt(p/10);
 if(o.value!=p)
	o.value=p
}
function blockNonNumbers(o,e,aD,aN)
{	var p=o.value
	var key=e.keyCode
	if(isNaN(key)||e.ctrlKey)
		return true
	key=String.fromCharCode(key)
	var reg=/\d/
	return (aN?key=='-'&&p.indexOf('-')==-1:false)||(aD?key=='.'&&p.indexOf('.')==-1:false)||reg.test(key)
}
function culc_Icon() { return "<input src='../images/ShowLines.gif' type='image' alt='Recalculate' onclick='return recalculate(this);' />" }

function add_allowance()
{	var tbl = _e("tblChart")
	var new_tr = tbl.insertRow(tbl.rows.length-1)
	new_tr.className="reportRow"
	for (var i = 0; i < tbl.rows[0].cells.length; i++)
	    new_tr.insertCell(i)
	for (var i = 4; i < tbl.rows[1].cells.length; i++)
	    new_tr.cells[i].className="reportAmount"
	new_tr.cells[0].style.backgroundColor= "#F0EFE8"
	new_tr.cells[0].innerHTML = culc_Icon()
	var s="<input type='text' style='width:90px;font-size:12px;'"
	s+="onmouseout='extractNumber(this,0,false,9999999999);'"
    s+="onkeyup='extractNumber(this,0,false,9999999999);'"
    s+="onkeypress='return blockNonNumbers(this,event,false,false);' />"
	new_tr.cells[1].innerHTML = s
	new_tr.cells[2].innerHTML = build_select_element("October")
	new_tr.cells[3].innerHTML = build_select_element("October")
	new_tr.cells[0].className="reportTotal"
	new_tr.cells[1].className="reportCaption3"
	new_tr.cells[2].className="reportCaption2"
	new_tr.cells[3].className="reportCaption2"
	_e("btnCancel").disabled=false
	_e("btnAddNew").style.display="none"
	Allowance_OnLoad()
	return false
}
function edit_row(ctrl)
{	var ctrl_row = _up(ctrl,2)
	var rr=ctrl_row.parentNode.rows
	for (var i = 1; i < rr.length-1; i++)
	{	if(_dwn(rr[i],2).alt=="Recalculate")
			recalculate(_dwn(rr[i],2))
	}
	var selected1 = ctrl_row.cells[2].innerHTML
	ctrl_row.cells[2].innerHTML = build_select_element(selected1)
	var selected2 = ctrl_row.cells[3].innerHTML
	ctrl_row.cells[3].innerHTML = build_select_element(selected2)
	//remove 'edit' button from this row:
	ctrl_row.cells[0].innerHTML = culc_Icon()
	_e("btnCancel").disabled = false
	_e("btnAddNew").disabled="disabled"
	_e("btnSave").disabled="disabled"
	Allowance_OnLoad()
	return false;
} 
function build_select_element(selected_value)
{	var select_string ="<select>"
	for (var i = 0; i < month_arr.length; i++)
	{	select_string = select_string + "<option "
		if (month_arr[i].toUpperCase() == selected_value.toUpperCase())
			select_string = select_string + "selected"
		select_string = select_string + ">" + month_arr[i] + "</option>"
	}
	select_string = select_string + "</select>"
	return select_string
}
function toNum(a)
{ a=parseFloat(a.replace(',','').replace(',','').replace(',','').replace('$',''))
  return a?a:0
}
function recalculate(ctrl)
{	var ctrl_row = _up(ctrl,2)
	//clear prev calculated distribution if it had a place
	for (var i = 4; i < ctrl_row.cells.length; i++)
		ctrl_row.cells[i].innerHTML = ""

	var amount
	var x = _son1(ctrl_row.cells[1])
	if (x)
		amount = formatMoney(x.value)//get the value from the input textbox:
	else
		amount = ctrl_row.cells[1].innerHTML
    var a = amount
	amount = toNum(amount)
	var sel_from_ctrl = _son1(ctrl_row.cells[2])
	var sel_to_ctrl = _son1(ctrl_row.cells[3])
	var m_from = sel_from_ctrl.options[sel_from_ctrl.selectedIndex].text
	var m_to = sel_to_ctrl.options[sel_to_ctrl.selectedIndex].text
	var first_month = 0
	var second_month = 0
	for (var i = 0; i < month_arr.length; i++)
	{	if (month_arr[i].toUpperCase() == m_from.toUpperCase())
			first_month = i
		if (month_arr[i].toUpperCase() == m_to.toUpperCase())
			second_month = i
	}
	var count = parseFloat(second_month - first_month)
	if (count < 0)
	{	alert("Please select valid distribution interval.")
		return false
	}
	amount = amount / ++count
	var str_amount = formatMoney(amount)
	for (var i = first_month; i <= second_month; i++)
	{	//keep the delta of 4 first cells!
		ctrl_row.cells[i+4].innerHTML = str_amount
	}
	ctrl_row.cells[0].innerHTML = "<input src='../images/note.gif' type='image' alt='Rearrange monthly amount' onclick='return edit_row(this);' />"
	ctrl_row.cells[1].innerHTML = a
	ctrl_row.cells[2].innerHTML = m_from
	ctrl_row.cells[3].innerHTML = m_to
	var rr = ctrl_row.parentElement.rows
	for (var j = 1; j < rr[0].cells.length; j++)
	{
		if(j==2||j==3)
			continue
		a=0
		for (var i = 1; i < rr.length-1; i++)
			a+=toNum(rr[i].cells[j].innerHTML)
		rr[rr.length-1].cells[j].innerHTML=a>0?formatMoney(a):""
	}
	_e("btnCancel").disabled = false
	_e("btnSave").disabled = false
	_e("btnAddNew").disabled = false
	Allowance_OnLoad()
	return false
}
function save_changes()
{	var r=""
	var t=_e("tblChart").rows
	for (var i = 1; i < t.length-1; i++)
	{	var c=t[i].cells
		r += -t[i].tabIndex+";"+c[1].innerHTML+";"+c[2].innerHTML+";"+c[3].innerHTML+"|"
	}
	_e("Result").value=r.substring(0,r.length-1)
	return true
} 
function formatMoney(m)
{	m = Math.round(m)
	var s = String(m)
	var objRegExp= new RegExp('(-?[0-9]+)([0-9]{3})')
	while(objRegExp.test(s))
		s = s.replace(objRegExp,'$1,$2');
	return '$'+s
}
function Allowance_OnLoad()//adjust tables width
{	var d=_e("divChart"); var s=d.style
	s.overflow=s.height=""
	if (d.offsetWidth>=900)
	{	s.overflow="auto"
		s.width="900px"
		s.height=d.offsetHeight+20
	}
}
