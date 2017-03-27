var A=0
function _e(o){return document.getElementById(o)}
function _son1(o){o=o.firstChild;return o.nodeType==1?o:o.nextSibling}
function _up(o,u){while(u--) o=o.parentNode;return o}
function mouse(b)
{	var s=window.event.srcElement
	while(s!=null&&s.tagName!="TR") s=s.parentNode;
	if(s==null) return
	if(s.className=="tRow"||s.className=="tRowAlt")
		s.style.backgroundColor=b
}
function formatMoney(m)
{	var s=String(Math.round(m))
	var oRegExp = new RegExp('(-?[0-9]+)([0-9]{3})')
	while(oRegExp.test(s))
		s=s.replace(oRegExp,'$1,$2');
	return '$'+s
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
 if(aN&&A<0&&p>0)
	p=-p
 if(p=="-")
	p=""
 if(o.value!=p)
	o.value=p
 if(o.value!=o.alt)
 {	o.alt=o.value
	recalculate(o)
 }
}
function recalculate(o)
{	var rr=_up(o,3).rows
	var cnt=rr.length-2
	var total_P=0
	var total_M=0
	var ri=_up(o,2).rowIndex
	if(o.parentNode.cellIndex==1)
	{	for(var i=1; i<cnt; i++)
		{	var j=_son1(rr[i].cells[1]).value
			var P=j==""?0:parseFloat(j)
			var M=Math.round(A*P/100)
			if(ri==i)
			{	var t=_son1(rr[i].cells[2])
				t.alt=t.value=M
			}
			total_P += P
			total_M += M
		}
	}
	else
	{	for(var i=1; i<cnt; i++)
		{	var j=_son1(rr[i].cells[2]).value
			var M=j==""?0:parseInt(j)
			var P=A==0?0:Math.round(M*1000000/A)/10000
			if(ri==i)
			{	var t=_son1(rr[i].cells[1])
				t.alt=t.value=P
			}
			total_P += P
			total_M += M
		}
	}
	rr[cnt].cells[1].innerHTML=Math.round(total_P*10000)/10000+'%'
	rr[cnt++].cells[2].innerHTML=formatMoney(total_M)
	total_P=Math.round((100-total_P)*10000)/10000
	rr[cnt].cells[1].innerHTML=total_P+'%'
	rr[cnt].cells[1].style.color=total_P<0?"red":""
	rr[cnt].cells[2].innerHTML=formatMoney(A-total_M)
	rr[cnt].cells[2].style.color=(A>=0&&A<total_M)||(A<0&&A>total_M)?"red":""
}
function send_rezult()
{	//get new values:
	var tbl = _e("tblData")
	var rows_count = tbl.rows.length;
	var str_func = "";
	var str_percent = "";
	for(var i=1; i<(rows_count-2); i++)
	{	//function name:
		str_func = str_func + tbl.rows[i].cells[0].innerText + ","
		//percent value:
		 str_percent = str_percent + tbl.rows[i].cells[1].firstChild.value + ","
	}
	str_func = str_func.substring(0,str_func.length-1)
	str_percent = str_percent.substring(0,str_percent.length-1)
	var org = _e("lblOrganization").innerText
	var fiscal_year = _e("lblFiscalYear").innerText
	var month = _e("lblMonth").innerText
	//build the XML
	var obj_temp_xml = get_xml_doc_object()
	var obj_xml_parent = append_xml_element(obj_temp_xml, "update_pgvxx")
	//add params:
	var obj_xml_node = add_xml_element(obj_temp_xml,obj_xml_parent, "params")
	add_xml_attribute (obj_temp_xml,obj_xml_node,"org",org)
	add_xml_attribute (obj_temp_xml,obj_xml_node,"fy",fiscal_year)
	add_xml_attribute (obj_temp_xml,obj_xml_node,"bm",month)
	add_xml_attribute (obj_temp_xml,obj_xml_node,"func_arr",str_func)
	add_xml_attribute (obj_temp_xml,obj_xml_node,"percent_arr",str_percent)
	//alert("before send xml: " + obj_temp_xml.xml)
	if (send_server_request(obj_temp_xml))
	{  
		opener.reload_report()
		self.close()
	}        
	return false
}
function send_server_request(obj_params_xml)
{	var obj_return_xml = load_asp_xml("HTTPFunds.aspx", obj_params_xml)
	var str_err_msg = ""
	if (obj_return_xml)
	{	if (obj_return_xml.documentElement.nodeName  == 'ok')
			return true
		try
		{	var obj_xml_node = obj_return_xml.documentElement.firstChild
			str_err_msg = obj_xml_node.attributes.getNamedItem("msg").value
		}
		catch(e){}
		//alert("Error in saving data." + "\n" + str_err_msg)
		alert(str_err_msg)
		return false
	}
}
function Page_OnLoad()
{	A=parseFloat(_e("lblAllowance").innerHTML.replace(',','').replace(',','').replace('$',''))
	var rr=_e('tblData').rows
	var cnt=rr.length-2
	for(var i=1; i<cnt; i++)
	{	var t=_son1(rr[i].cells[1])
		t.alt=t.value
		recalculate(t)
	}
}
