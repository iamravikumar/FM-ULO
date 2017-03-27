<script type="text/javascript" >
<!--

    function on_cancel()
    {
        self.close();
        opener.on_reassign_cancel();
        return false;
    }
    
    function on_assign_request()
    {
        var listOrg = document.getElementById("ctrlOrgUsers_ddlOrganizations");
        var listUsers = document.getElementById("ctrlOrgUsers_ddlUsers");
        
        var org_value = listOrg.options[listOrg.selectedIndex].text;
        var reviewer = listUsers.options[listUsers.selectedIndex].value;
        var doc_num = document.getElementById("lblDocNumber").innerText;
        var comments = document.getElementById("txtComment").innerText;
                        
        if ((reviewer == "" || org_value == "") && comments != "")
        {
            if (!confirm("You did not select suggested organization and/or reviewer. Would you like to reroute this item without your suggestions?"))
            return false;
        }
        
        if ((reviewer == "" || org_value == "") && comments == "")
        {
            alert("You did not select suggested organization and/or reviewer. \nIf you wish to reroute this item without your suggestions, please fill the COMMENT field.");
            return false;
        }
                
        //update DB
        //debugger
        if (update_reviewer(reviewer, org_value, comments))
        {
            try
            {                
                opener.on_reassign();
            }
            catch(e){}
            
            self.close();
            return false;
        }
    }    
    
    function update_reviewer(user, org_value, comments)
    {
        //action_code values - see tblHistoryActions in DB
        // 5 - 'Reassignment Request' - request send to Org Admin
        
        //get additional values:
        var item_id = document.getElementById("txtItemID").value;
        var lines = document.getElementById("lblLines").innerText;
        var org_code = document.getElementById("lblOrgCode").innerText;
        //build the XML
        var obj_temp_xml = get_xml_doc_object();
        var obj_xml_parent = append_xml_element(obj_temp_xml, "reassign_request");
        //add params:
        var obj_xml_node = add_xml_element(obj_temp_xml,obj_xml_parent, "params");
        add_xml_attribute (obj_temp_xml,obj_xml_node,"item",item_id);
        add_xml_attribute (obj_temp_xml,obj_xml_node,"lines",lines);
        add_xml_attribute (obj_temp_xml,obj_xml_node,"org_code",org_code);
        add_xml_attribute (obj_temp_xml,obj_xml_node,"org_new_value",org_value);        
        add_xml_attribute (obj_temp_xml,obj_xml_node,"user_id",user);
        add_xml_attribute (obj_temp_xml,obj_xml_node,"com", comments);
    
        //alert("before send xml: " + obj_temp_xml.xml);            
        return send_server_request(obj_temp_xml);
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

