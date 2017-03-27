function _e(o){return document.getElementById(o)}

function data_to_excel()
{	var popup=window.open("FundReportExcel.aspx"+document.location.search+"&type=8", "FundsDataToExcel",
			"width=600,height=600,menubar=no,status=no,resizable=yes,scrollbars=yes,toolbar=yes,top=200,left=200")
	popup.focus()
 return false
}
function Expand(p)
{	if(p.title=="Expand")
	{	p.innerHTML="<font color='green'>Hide Details</font>"
		p.title="Hide"
		p.parentNode.nextSibling.style.display=""
	}
	else
	{	p.innerHTML="<font color='green'>Show Details</font>"
		p.title="Expand"
		p.parentNode.nextSibling.style.display="none"
	}
}
function mouse(b)
{	var s=window.event.srcElement
	while(s!=null && s.tagName!="TR") s=s.parentNode;
	if(s==null) return;
	var c=s.className.substr(0,4)
	if(c=="tRow" || c=="eRow")
		s.style.backgroundColor=b
}
function close_window()
{	var x=_e("txtReloadReport").value
	if (x=="yes")
		opener.reload_report()
	self.close()
}
function limit_length(ctrl_txt, limit_value)
{	return (ctrl_txt.value.length <= limit_value)
}
function edit_row(ctrl_btn, entry_id, entry_type)
{	var ctrl_row = ctrl_btn.parentNode.parentNode
	var t2 = ctrl_row.cells[2].innerText
	var t3 = ctrl_row.cells[3].innerText
	var t4 = ctrl_row.cells[4].innerText
	var x = ctrl_row.cells.length
	for(var i=0; i<x; i++)
	{	ctrl_row.deleteCell(0)
	}
	var cell = ctrl_row.insertCell(0)
	cell.innerHTML = "<input type='image' src='../images/back.gif' alt='Cancel changes' onclick='return cancel_edit(this);' width='12px' height='12px' />"
	cell = ctrl_row.insertCell(1)
	cell.innerHTML = "<input type='image' src='../images/save.gif' alt='Save changes' onclick='return save_row(this,"+entry_id+","+entry_type+");' width='12px' height='12px' />"
	cell = ctrl_row.insertCell(2)
	cell.innerHTML = "<input type='text' value='"+t2+"' style='width:110px;' onkeypress='return limit_length(this,20);'/>"
	cell = ctrl_row.insertCell(3)
	cell.innerHTML = "<input type='text' value='"+t3+"' style='width:90px;' onkeypress='return limit_length(this,20);'/>"
	cell = ctrl_row.insertCell(4)
	cell.innerHTML = "<input type='text' value='"+t4+"' style='width:410px;' onkeypress='return limit_length(this,200);'/>"
	return false
}
function cancel_edit(ctrl_btn)
{	var ctrl_row = ctrl_btn.parentNode.parentNode
	var cell_index = ctrl_btn.parentNode.cellIndex
	ctrl_row.cells[cell_index + 2].innerText = ""
	ctrl_row.cells[cell_index + 3].innerText = ""
	ctrl_row.cells[cell_index + 4].innerText = ""
	return true
}
function delete_row(ctrl_btn, entry_id, entry_type)
{//update data:
	var ctrl_row = ctrl_btn.parentNode.parentNode
	var cell_index = ctrl_btn.parentNode.cellIndex
	var t2 = ctrl_row.cells[cell_index + 1].innerText
	var t3 = ctrl_row.cells[cell_index + 2].innerText
	//check with RegExp money format
	t3 = t3.replace("$","")
	t3 = t3.replace(",","")
	var org = _e("lblOrganization").innerText
	var fiscal_year = _e("lblFiscalYear").innerText
	var month = _e("txtBookMonth").value
	var group_code = _e("lblReportGroupCode").innerText
	//build the XML
	var obj_temp_xml = get_xml_doc_object()
	var obj_xml_parent = append_xml_element(obj_temp_xml, "delete_entry")
	//add params:
	var obj_xml_node = add_xml_element(obj_temp_xml,obj_xml_parent, "params")
	add_xml_attribute (obj_temp_xml,obj_xml_node,"type",entry_type)
	add_xml_attribute (obj_temp_xml,obj_xml_node,"id",entry_id)
	add_xml_attribute (obj_temp_xml,obj_xml_node,"org",org)
	add_xml_attribute (obj_temp_xml,obj_xml_node,"fy",fiscal_year)
	add_xml_attribute (obj_temp_xml,obj_xml_node,"bm",month)
	add_xml_attribute (obj_temp_xml,obj_xml_node,"gcd",group_code)
	add_xml_attribute (obj_temp_xml,obj_xml_node,"doc",t2)
	add_xml_attribute (obj_temp_xml,obj_xml_node,"amount",t3)
	//alert("before send xml: " + obj_temp_xml.xml)
	if (send_server_request(obj_temp_xml))
	{	_e("txtReloadReport").value = "yes"
		return true
	}
	return false
}
function save_row(ctrl_btn, entry_id, entry_type)
{	//save the data:
	var ctrl_row = ctrl_btn.parentNode.parentNode
	var cell_index = ctrl_btn.parentNode.cellIndex
	var t2 = ctrl_row.cells[cell_index + 1].firstChild.value
	var t3 = ctrl_row.cells[cell_index + 2].firstChild.value
	var t4 = ctrl_row.cells[cell_index + 3].firstChild.value
	//check with RegExp money format
	t3 = t3.replace("$","")
	t3 = t3.replace(",","")
	if (t3 == "")
		t3 = "0"
	var objRegExp = /(^[-]?\d+[.]?\d*$)/;
	if (objRegExp.test(t3) != true)
	{	alert("Please enter the money amount in the correct format.")
		return false;
	}
	if (t2.length > 20)
	{	alert("The DocNumber value cannot exceed 20 letters.")
		return false
	}
	if (t4.length > 200)
	{	alert("The Explanation text cannot exceed 200 letters.")
		return false
	}
	var org = _e("lblOrganization").innerText
	var fiscal_year = _e("lblFiscalYear").innerText
	var month = _e("txtBookMonth").value
	var group_code = _e("lblReportGroupCode").innerText
	//build the XML
	var obj_temp_xml = get_xml_doc_object()
	var obj_xml_parent = append_xml_element(obj_temp_xml, "update_data")
	//add params:
	var obj_xml_node = add_xml_element(obj_temp_xml,obj_xml_parent, "params")
	add_xml_attribute (obj_temp_xml,obj_xml_node,"type",entry_type)
	add_xml_attribute (obj_temp_xml,obj_xml_node,"id",entry_id)
	add_xml_attribute (obj_temp_xml,obj_xml_node,"org",org)
	add_xml_attribute (obj_temp_xml,obj_xml_node,"fy",fiscal_year)
	add_xml_attribute (obj_temp_xml,obj_xml_node,"bm",month)
	add_xml_attribute (obj_temp_xml,obj_xml_node,"gcd",group_code)
	add_xml_attribute (obj_temp_xml,obj_xml_node,"doc",t2)
	add_xml_attribute (obj_temp_xml,obj_xml_node,"amount",t3)
	add_xml_attribute (obj_temp_xml,obj_xml_node,"exp",t4)
	//alert("before send xml: " + obj_temp_xml.xml)
	if (send_server_request(obj_temp_xml))
	{	_e("txtReloadReport").value = "yes"
		return true
	}
	return false
}
function send_server_request(obj_params_xml)
{	var obj_return_xml = load_asp_xml("HTTPFunds.aspx", obj_params_xml)
	var str_err_msg = ""
	if (obj_return_xml)
	{	if (obj_return_xml.documentElement.nodeName == 'ok')
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
