<script type="text/javascript" >
<!--

    
    function on_cancel()
    {
        opener.on_reassign_cancel();
        self.close();      
        return false;
    }           
    
    function on_reroute()
    {
        var listOrg = document.getElementById("ctrlOrgUsers_ddlOrganizations");
        var listUsers = document.getElementById("ctrlOrgUsers_ddlUsers");
        
        var org_value = listOrg.options[listOrg.selectedIndex].text;
        var reviewer = listUsers.options[listUsers.selectedIndex].value;
        var reviewer_name = listUsers.options[listUsers.selectedIndex].text;
        
        if (reviewer == "" || reviewer == "0" || org_value == "")
        {
            alert("Please select Organization and Reviewer. Both fields are required.");
            return false;
        }
        
        var comments = document.getElementById("txtComment").innerText;
        
        var items_arr = document.getElementById("txtGroupAssign").value;
                
        if (items_arr.length > 0)
        {
            //build the XML
            var obj_temp_xml = get_xml_doc_object();
            var obj_xml_parent = append_xml_element(obj_temp_xml, "group_reroute");
            //add params:
            var obj_xml_node = add_xml_element(obj_temp_xml,obj_xml_parent, "params");
            add_xml_attribute (obj_temp_xml,obj_xml_node,"items",items_arr);     
            add_xml_attribute (obj_temp_xml,obj_xml_node,"org_new_value",org_value);
            add_xml_attribute (obj_temp_xml,obj_xml_node,"user",reviewer);
            add_xml_attribute (obj_temp_xml,obj_xml_node,"com", comments);
            
        }
        else 
        {
            var item_id = document.getElementById("txtItemID").value;
            var lines = document.getElementById("lblLines").innerText;
            if (lines == '' || lines == 'all') { lines = '0'; }
            var doc_num = document.getElementById("lblDocNumber").innerText;
            var request_id = document.getElementById("txtRequestID").value;
            var prev_org = document.getElementById("txtPrevOrganization").value;
            var prev_user = document.getElementById("txtPrevReviewer").value;
            
            //build the XML
            var obj_temp_xml = get_xml_doc_object();
            var obj_xml_parent = append_xml_element(obj_temp_xml, "reroute");
            //add params:
            var obj_xml_node = add_xml_element(obj_temp_xml,obj_xml_parent, "params");
            add_xml_attribute (obj_temp_xml,obj_xml_node,"request_id",request_id);
            add_xml_attribute (obj_temp_xml,obj_xml_node,"item_id",item_id);
            add_xml_attribute (obj_temp_xml,obj_xml_node,"doc_num",doc_num);
            add_xml_attribute (obj_temp_xml,obj_xml_node,"lines",lines);
            add_xml_attribute (obj_temp_xml,obj_xml_node,"prev_org",prev_org);
            add_xml_attribute (obj_temp_xml,obj_xml_node,"prev_user",prev_user);
            add_xml_attribute (obj_temp_xml,obj_xml_node,"org_new_value",org_value);
            add_xml_attribute (obj_temp_xml,obj_xml_node,"user",reviewer);
            add_xml_attribute (obj_temp_xml,obj_xml_node,"com", comments);
        }
                                                                       
        //update DB
        if (send_server_request(obj_temp_xml))
        {
            var doc_num = "";
            var org = "";                                
            try
            {
                doc_num = document.getElementById("lblDocNumber").innerText;
                org = org_value.substring(0, org_value.indexOf(':')-1);                
            } 
            catch(e) {}            
            opener.on_reassign(doc_num, org, reviewer_name, "reroute");
            self.close();
            return false;
        }
    }            
    
    function send_server_request(obj_params_xml)
    {
        var obj_return_xml = load_asp_xml("../HTTPServer.aspx", obj_params_xml);
        var str_err_msg = "";
        if (obj_return_xml)
        {
        
            if (obj_return_xml.documentElement.nodeName  == 'ok')
	            return true;
            else
            {
	            try
	            {
		            var obj_xml_node = obj_return_xml.documentElement.firstChild;
		            str_err_msg = obj_xml_node.attributes.getNamedItem("msg").value;			
	            }
	            catch(e){}
	            //alert("Error in saving data." + "\n" + str_err_msg);    	
	            alert(str_err_msg);		
	            return false;			
            }
        }
    }        



//  -->
</script>

