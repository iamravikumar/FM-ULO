
<script type="text/javascript" >
<!--
    
    var arr_items = ""; 
    //SM_  
    var ulo_org_code = ""; 
    
    function select_all()
    {
    //debugger
        if (arr_items == document.getElementById("txtAllSelected").value)
        {
            //unselect all checkboxes
            arr_items = "";
            select_checkboxes(false);
        }
        else
        {
            //select all checkboxes
            arr_items = document.getElementById("txtAllSelected").value;
            select_checkboxes(true);
        }
    }
    
    function select_checkboxes(value)
    {
        var x = document.getElementsByTagName("input");
        
        for(i=0; i < x.length; i++)
        {
            if (x[i].type == "checkbox" && !x[i].disabled)
                try { x[i].checked = value; } catch(e) {}            
        }
    }
    
    function reassign_items()
    {
    //debugger
        try
        {
            //alert (arr_items);
            
            if (arr_items == "")
            {
                alert("Please select Open Items for reassignment.");
                return;
            }
        
            var target_page = document.getElementById("txtReassignTargetPage").value; 
            //SM_                       
            var popup = window.open(target_page+".aspx?group="+arr_items + "&org_code="+ ulo_org_code, "OpenItems", 
                "width=600,height=360,menubar=no,status=no,resizable=yes,scrollbars=yes,toolbar=no,top=200,left=200");    

            popup.focus();
        } catch(e) {}
    }
    
    function on_reassign_cancel()
    {
        arr_items = "";
        select_checkboxes(false);
    }
    
    function on_reassign(doc_number, organization, reviewer, action)
    {
    
        //reload the page:
        location.href = "Search.aspx?back=y";
    }
    
    //SM_   parameter ulo_org_code added
    function check_for_assign(ctrl, item_id, line_num,org_code)
    {
    //debugger
        var value = item_id + '_' + line_num +',';
        ulo_org_code = org_code;
        if (ctrl.checked)
        {
            if (arr_items.indexOf(value)<0)
            {
                arr_items+=value;                
            }
        }
        else
        {
            if (arr_items.indexOf(value)>(-1))
                arr_items = arr_items.replace(value,'');
        }        
    }
    
    function row_on_click(item_id, org_code, reviewer_id, load_id, load_type_id)
    {
    //debugger
        if(load_type_id=="6")
        {
            window.location.href="OIBA53Review.aspx?id="+item_id+"&org="+org_code+"&user="+reviewer_id+"&load="+load_id;
        }
        else
        {
            window.location.href="OpenItemDetails.aspx?id="+item_id+"&org="+org_code+"&user="+reviewer_id+"&load="+load_id;
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



