
<script type="text/javascript" >
<!--
    
    var selected_row;
    var reassign_btn;
        
    function reassign_item(ctrlBtn, item_id, doc_number, org_code, reviewer)
    {
        try
        {
            selected_row = ctrlBtn.parentElement.parentElement;
            reassign_btn = ctrlBtn;
        
            var target_page = document.getElementById("txtReassignTargetPage").value;
            var h;
            if (target_page == "Reroute")
                h = 460;
            else
                h = 600;
            var popup = window.open(target_page+".aspx?item="+item_id+"&doc="+doc_number+"&org="+org_code+"&user="+reviewer, "OpenItems", 
                "width=600,height="+h+",menubar=no,status=no,resizable=yes,scrollbars=yes,toolbar=no,top=200,left=200");    

            popup.focus();
        } catch(e) {}
    }
    
    function on_reassign_cancel()
    {
        //do nothing
    }
    
    function on_reassign(doc_number, organization, reviewer, action)
    {
        if (selected_row)
        {
            if (selected_row.cells[0].innerText == doc_number)
            {
                if (action == "assign")
                {
                    //reload the page:
                    location.href = "ReviewOpenItems.aspx?back=y";
                }
                else if (action == "reroute")
                {
                    //the item was rerouted by BD Admin, the organization could change for this row,
                    //so the link to the OpenItem screen will not work.
                    //reload the page:
                    location.href = "ReviewOpenItems.aspx?back=y";
                }
                else
                {
                    //reassigmment has been done to another organization
                    //var dt = new Date();
                    //var str_date = (dt.getMonth()+1)+"/"+dt.getDate()+"/"+dt.getFullYear();
                    //selected_row.cells[9].innerText = "Reassignment Request " + str_date;
                    selected_row.cells[9].innerText = "Reassignment Request";
                    if (reassign_btn)
                        reassign_btn.disabled = true;
                        
                }
            }
        }    
    }
    
    function row_on_click(ctrl, redirect_page, item_id, org_code, reviewer_id, load_id)
    {
    //debugger
        location.href=redirect_page+"?id="+item_id+"&org="+org_code+"&user="+reviewer_id+"&load_id="+load_id;
    }
    
    function on_verify(chkboxCtrl, item_id, org_code, reviewer_id)
    {
        if (chkboxCtrl.checked)
        {
        
            //build the XML
            var obj_temp_xml = get_xml_doc_object();
            var obj_xml_parent = append_xml_element(obj_temp_xml, "verify_assignment");
            //add params:
            var obj_xml_node = add_xml_element(obj_temp_xml,obj_xml_parent, "params");
            add_xml_attribute (obj_temp_xml,obj_xml_node,"item_id",item_id);
            add_xml_attribute (obj_temp_xml,obj_xml_node,"org_code",org_code);	
            add_xml_attribute (obj_temp_xml,obj_xml_node,"reviewer_id",reviewer_id);
        
            //alert("before send xml: " + obj_temp_xml.xml);            
            if (send_server_request(obj_temp_xml))
            {            
                chkboxCtrl.disabled = true;
                chkboxCtrl.parentElement.parentElement.cells[9].innerText = 'Assigned';
            }
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
	            alert("Error in saving data." + "\n" + str_err_msg);    			
	            return false;			
            }
        }
    }        

//  -->       
</script>

