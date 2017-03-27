function data_to_excel()
{	var s=document.location.search
	if(s.length>0)
		s=s.substring(s.indexOf('org='))
	var popup=window.open("FundReportExcel.aspx?type=7&FY="+_e("ddlFiscalYear").value+"&"+s, "FundsDataToExcel",
                "width=600,height=600,menubar=no,status=no,resizable=yes,scrollbars=yes,toolbar=yes,top=200,left=200")
	popup.focus()
 return false
}
function _e(o){return document.getElementById(o)}
function _son1(o){o=o.firstChild;return o.nodeType==1?o:o.nextSibling}
function _son2(o){return _nxt(_son1(o))}
function _nxt(o){o=o.nextSibling;return o==null||o.nodeType==1?o:o.nextSibling}
function mouse(b)
{	var s=window.event.srcElement
	while(s!=null && s.tagName!="TR") s=s.parentNode;
	if(s==null) return
	if(s.className=="reportRow")
		s.style.backgroundColor=b
}
function to_org(fy,bl,org)
{	
    location.href="OrgAllowance.aspx?fy="+fy+"&bl="+bl+"&org="+org
}
function to_bl(fy,bl)
{	
    location.href="OrgAllowance.aspx?fy="+fy+"&bl="+bl
}
var _Month
function open_distribution(fy,blc,org,bm,t, bl)
{	_Month=bm;
	var popup=window.open("AllowanceDistribution.aspx?org="+org+"&blc="+blc+"&fy="+fy+"&bm="+bm+"&t="+t+"&bl="+escape(bl), "FundAllowance",
			"width=530,height=750,menubar=no,status=no,resizable=yes,scrollbars=yes,toolbar=no,top=50,left=200")
	popup.focus()
}
function formatMoney(m)
{	m=Math.round(m)
	var s=String(m)
	var objRegExp=new RegExp('(-?[0-9]+)([0-9]{3})')
	while(objRegExp.test(s))
		s=s.replace(objRegExp,'$1,$2');
	return '$'+s
}
function GetRezult(s,f)
{	var ss=s.split(";")
	var rr=_e("tblChart").rows;
	var c=(parseFloat(_Month)+2)%12*2+1
	var allow=new Array(12)
	for(var i=0;i<12;i++)
	{	allow[i]=rr[1].cells[i*2+2].innerHTML.replace(',','').replace(',','').replace(',','').replace('$','')
	}
	for(var i=2;i<rr.length;i++)
	{	var rTot=0; var k=0
		for(var j=1;j<24;)
		{	if(c==j||(j>c&&f>0))
				rr[i].cells[j].innerHTML=ss[i-2]
			var m=allow[k++]*rr[i].cells[j++].innerHTML/100
			rTot+=m
			rr[i].cells[j++].innerHTML=formatMoney(m)
		}
		rr[i].cells[25].innerHTML=formatMoney(rTot)
	}
	_e("btnCancel").disabled=_e("btnSave").disabled=false
}
function save_changes()
{	var r=""
	var rr=_e("tblChart").rows
	//debugger
	for (var i=2;i< rr.length-1;i++)
	{	
	    if(rr[i].cells[0].children.length>1)
			r+=_son2(rr[i].cells[0]).innerHTML
		else
			r+=rr[i].cells[0].title //title - takes group code
			//r+=rr[i].cells[0].innerHTML - takes name
		for (var j=1;j<24;j++)
			r+=";"+rr[i].cells[j++].innerHTML
		r+="|"
	}
	_e("Result").value=r.substr(0,r.length-1)
	return true
}
function Redistribute()
{	var r=""
	var rr=_e("tblChart").rows
	for (var i=2;i< rr.length-1;i++)
	{	var c=rr[i].cells[0]
		if(_son1(c).checked)
			r+=_son2(c).innerHTML+","
	}
	if(r.length>0)
	{	_e("Result").value=r.substr(0,r.length-1)
		return true
	}
	_e("lblError").innerText="Select one or more organizations."
	return false
}
function Page_OnLoad()
{	/*var d=_e("divChart"); var s=d.style
	s.overflow=s.height=""
	if (d.offsetWidth>=900)
	{	s.overflow="auto"
		s.width="900px"
		s.height=d.offsetHeight+20
	}*/
}

