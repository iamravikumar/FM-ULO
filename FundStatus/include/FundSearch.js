function data_to_excel()
{	var popup=window.open("FundReportExcel.aspx?type=2","FundsDataToExcel",
			"width=600,height=600,menubar=no,status=no,resizable=yes,scrollbars=yes,toolbar=yes,top=200,left=200")
	popup.focus()
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

function email_request()
{
    //build the XML
	var obj_temp_xml = get_xml_doc_object()
	var obj_xml_parent = append_xml_element(obj_temp_xml, "search_request")
	//add params:
	var obj_xml_node = add_xml_element(obj_temp_xml,obj_xml_parent, "params")	
	//alert("before send xml: " + obj_temp_xml.xml)
	if (send_server_request(obj_temp_xml))
	{	
		alert2("You will get email with requested report shortly. Thank you.","Fund Status message");
	}
	return false;
}

