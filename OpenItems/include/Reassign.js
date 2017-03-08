
<script type="text/javascript" >
<!--
    
    function on_cancel()
    {
        self.close();
        opener.on_reassign_cancel();
        return false;
    }
    
    function on_assign()
    {  
     //include\Reassign.js
    //debugger      
        var list = document.getElementById("ddlAssign");
        var reviewer = list.options[list.selectedIndex].value;
        var reviewer_name = list.options[list.selectedIndex].text;
        
        var comments = document.getElementById("txtCommentAssign").innerText;        
        
        if (reviewer == "")
        {
            alert ("Please select reviewer to assign item.");
            return false;
        }
        
        var items_arr = document.getElementById("txtGroupAssign").value;
        
                //ctrlOrgUsers.Visible = false;
         //debugger
        var OrgCode = document.getElementById("txtOrgCode").value;
        
        if (items_arr.length > 0)
        {
            //build the XML
            var obj_temp_xml = get_xml_doc_object();
            var obj_xml_parent = append_xml_element(obj_temp_xml, "group_reroute");
            //add params:
            var obj_xml_node = add_xml_element(obj_temp_xml,obj_xml_parent, "params");
            add_xml_attribute (obj_temp_xml,obj_xml_node,"items",items_arr); 
            // \OpenItems\OpenItems\include\Reassign.js
            add_xml_attribute (obj_temp_xml,obj_xml_node,"org_new_value",OrgCode);
            add_xml_attribute (obj_temp_xml,obj_xml_node,"user",reviewer);
            add_xml_attribute (obj_temp_xml,obj_xml_node,"com", comments);
            
            //update DB
            if (send_server_request(obj_temp_xml))
            {                
                opener.on_reassign();
                self.close();
                return false;
            }
        }
        else
        {
            var doc_num = document.getElementById("lblDocNumber").innerText;
            
            //update DB -
            //in the case of assignment we should not update organization on the main page - 
            //the organization stays the same
            if (update_reviewer(reviewer, "", 3, comments))
            {            
                //doc_number, organization, reviewer, action
                opener.on_reassign(doc_num, "", reviewer_name, "assign");
                
                self.close();
                return false;
            }
        }        
    }
    
    function on_reroute()
    {
        var listOrg = document.getElementById("ctrlOrgUsers_ddlOrganizations");
        var listUsers = document.getElementById("ctrlOrgUsers_ddlUsers");
        
        var org_value = listOrg.options[listOrg.selectedIndex].text;
        var reviewer = listUsers.options[listUsers.selectedIndex].value;
        var reviewer_name = listUsers.options[listUsers.selectedIndex].text;
        var doc_num = document.getElementById("lblDocNumber").innerText;
        var comments = document.getElementById("txtCommentReroute").innerText;
                        
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
        if (update_reviewer(reviewer, org_value, 5, comments))
        {
            //doc_number, organization, reviewer, action
            opener.on_reassign(doc_num, org_value, reviewer_name, "reroute");

            self.close();
            return false;
        }
    }    
    
    function update_reviewer(user, org_value, action_code, comments)
    {
        //action_code possible values: (see tblHistoryActions in DB)
        // 3 - 'Reviewer Assignment' - responsibility of Org Admin - direct assisgnment within the same organization
        // 5 - 'Reassignment Request' - send to BD Admin
        
        //get additional values:
        var item_id = document.getElementById("txtItemID").value;
        var request_id = document.getElementById("txtRequestID").value;
        var lines = document.getElementById("lblLines").innerText;
        if (lines == "all") {lines = "0";}
        var org_code = document.getElementById("lblOrgCode").innerText;
        var prev_user = document.getElementById("txtCurrentReviewer").value;
        //build the XML
        var obj_temp_xml = get_xml_doc_object();
        var obj_xml_parent = append_xml_element(obj_temp_xml, "reassign_reroute");
        //add params:
        var obj_xml_node = add_xml_element(obj_temp_xml,obj_xml_parent, "params");
        add_xml_attribute (obj_temp_xml,obj_xml_node,"request",request_id);
        add_xml_attribute (obj_temp_xml,obj_xml_node,"item",item_id);
        add_xml_attribute (obj_temp_xml,obj_xml_node,"lines",lines);
        add_xml_attribute (obj_temp_xml,obj_xml_node,"org_code",org_code);
        add_xml_attribute (obj_temp_xml,obj_xml_node,"org_new_value",org_value);
        add_xml_attribute (obj_temp_xml,obj_xml_node,"action",action_code);
        add_xml_attribute (obj_temp_xml,obj_xml_node,"prev_user_id",prev_user);
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


