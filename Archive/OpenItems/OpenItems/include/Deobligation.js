<script type="text/javascript" >
<!--
    
    function on_certify(chkboxCtrl, item_id, line_num, org_code)
    {
        var result_date = "";
        
        if (chkboxCtrl != null)
        {            
            if (!chkboxCtrl.checked)
                if (!confirm("Are you sure you want to cancel verification for this item?"))
                    return false;
                    
            //build the XML
            var obj_temp_xml = get_xml_doc_object();
            var obj_xml_parent = append_xml_element(obj_temp_xml, "certify_deobl");
            //add params:
            var obj_xml_node = add_xml_element(obj_temp_xml,obj_xml_parent, "params");
            add_xml_attribute (obj_temp_xml,obj_xml_node,"item_id",item_id);
            add_xml_attribute (obj_temp_xml,obj_xml_node,"line_num",line_num);
            add_xml_attribute (obj_temp_xml,obj_xml_node,"org",org_code);
            
            result_date = send_server_request(obj_temp_xml);
            try
            {
                if (result_date != "")
                {
                    chkboxCtrl.checked = true;
                    chkboxCtrl.parentElement.parentElement.cells[6].innerText = result_date;
                }
                else
                {
                    chkboxCtrl.checked = false;
                    chkboxCtrl.parentElement.parentElement.cells[6].innerText = "";
                }
            } catch(e){}
        }
    }            
    
    function send_server_request(obj_params_xml)
    {
        var obj_return_xml = load_asp_xml("../HTTPServer.aspx", obj_params_xml);
        var str_err_msg = "";
        if (obj_return_xml)
        {
   
            if (obj_return_xml.documentElement.nodeName  == 'result_date')
	            return obj_return_xml.text;
            else
            {
	            try
	            {
		            var obj_xml_node = obj_return_xml.documentElement.firstChild;
		            str_err_msg = obj_xml_node.attributes.getNamedItem("msg").value;			
	            }
	            catch(e){}
	            alert("Error in saving data." + "\n" + str_err_msg);    			
	            return false;			
            }
        }
    }        


//  -->
</script>

