
<script type="text/javascript" >
<!--

    var selected_row;
        
    function reassign_item(ctrlBtn, request_id)
    {
        try
        {
            selected_row = ctrlBtn.parentElement.parentElement;
            var target_page = document.getElementById("txtReassignTargetPage").value;
            var h;
            if (target_page == "Reroute")
                h = 460;
            else
                h = 600;
    
            var popup = window.open(target_page+".aspx?request="+request_id, "OpenItems", 
                "width=600,height="+h+",menubar=no,status=no,resizable=yes,scrollbars=yes,toolbar=no,top=200,left=200");    

            popup.focus();
        }
        catch(e) {}
    }
    
    function on_reassign(doc_number)
    {
        if (selected_row)
        {
            if (selected_row.cells[0].innerText == doc_number)
            {
                delete_row(selected_row);                
            }
        } 
    }
    
    function on_reassign_cancel()
    {
        //do nothing
    }
    
    function on_verify(chkboxCtrl, request_id, new_reviewer, prev_organization, prev_reviewer)
    {
        if (chkboxCtrl != null)
        {            
            if (chkboxCtrl.checked)                        
            {
                 //build the XML
                var obj_temp_xml = get_xml_doc_object();
                var obj_xml_parent = append_xml_element(obj_temp_xml, "verify_reroute");
                //add params:
                var obj_xml_node = add_xml_element(obj_temp_xml,obj_xml_parent, "params");
                add_xml_attribute (obj_temp_xml,obj_xml_node,"prev_org",prev_organization);
                add_xml_attribute (obj_temp_xml,obj_xml_node,"prev_user",prev_reviewer);
                add_xml_attribute (obj_temp_xml,obj_xml_node,"request_id",request_id);	
                add_xml_attribute (obj_temp_xml,obj_xml_node,"new_reviewer",new_reviewer);	
	            
                //alert("before send xml: " + obj_temp_xml.xml);            
                if (send_server_request(obj_temp_xml))
                {
                    //window.location.href="Assignments.aspx?back=y";
                    
                    var row = chkboxCtrl.parentElement.parentElement;
                    delete_row(row);                    
                    
                }    
                else
                {
                    chkboxCtrl.checked = false;
                }
            }            
        }
    }
    
    function delete_row(row)
    {
        try
        {
	        var count = row.cells.length-1;
	        for(var i=count; i>=0; i--)
	        {
		        row.deleteCell(i);
	        }
	    }catch(e){}
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
	            alert2(str_err_msg);
	            return false;			
            }
        }
    }        


//  -->
</script>