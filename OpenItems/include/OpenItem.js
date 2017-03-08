
<script type="text/javascript" >
<!--


    var blnCompDateIsChanged = false;
    var blnCommentIsChanged = false;
    var redirectURL = location.href;
    var arr_lines = "";
        
    function line_selected(ctrl, line_num)
    {
    //debugger
        var value = line_num;
        if (ctrl.checked)
        {
            if (arr_lines.indexOf(value)<0)
            {
                if(arr_lines.length<1)
                {
                    arr_lines+=value;
                } 
                else
                {
                    arr_lines+="," + value;
                }
                ctrl.name = "test_chk";
            }
        }
        else
        {
            if (arr_lines.indexOf(value)>(-1))
                arr_lines = arr_lines.replace(value,'');
        }
        arr_lines = arr_lines.replace(",,",',');
        
         if(arr_lines.charAt(0) == ",") 
         {
            arr_lines = arr_lines.slice(1, arr_lines.length);
         }
    }
    
    function onchange_date()
    {
        blnCompDateIsChanged = true;                        
    }
    
    function onchange_comment()
    {
        blnCommentIsChanged = true;
    }
    
    function reassign_lines()
    {
    
        var item_id = document.getElementById("txtItemID").value;
        var doc_number = document.getElementById("lblDocNumber").innerText;
        var org_code = document.getElementById("lblOrgCode").innerText;
        var current_reviewer = document.getElementById("txtReviewerUserID").value;
       // debugger
        if (arr_lines.length>0)
            arr_lines = arr_lines.substring(0, arr_lines.length);
        else
        {
            var status_code = document.getElementById("txtItemStatusCode").value;
            if (status_code == 4)
            {
                //for item in status "Reassign Request" we can not accept reroute request for all lines
                //because some of the lines are already waiting for reroute.
                //user must select lines available for reroute.
                alert("Please select specific lines you wish to reroute.");
                return false;;
            }
        }
        //alert ("arr_lines: " + arr_lines);
    
        var reassign_target = document.getElementById("txtReassignTargetPage").value;
        var h;
            if (reassign_target == "Reassign")
                h = 600;
            else
                h = 460;
                
 
        if(arr_lines.charAt( arr_lines.length-1 ) == ",") 
        {
             arr_lines = arr_lines.slice(0, -1);
        }
              
        var popup = window.open(reassign_target+".aspx?item="+item_id+"&doc="+doc_number+"&org="+org_code+"&user="+current_reviewer+"&lines="+arr_lines, "OpenItems", 
            "width=600,height="+h+",menubar=no,status=no,resizable=yes,scrollbars=yes,toolbar=no,top=200,left=200");    

        popup.focus();
        return false;
    }
    
    function on_reassign()
    {
        on_cancel();
    }
    
    function on_reassign_cancel()
    {
        try{
        var x = document.getElementsByTagName("input");
        for (i=0; i++; i<x.length)
            x.checked = false;
        } catch(e) {}
    }

    function get_line_details(url)
    {
        redirectURL = url;
        save_changes();
        //check_changes()            
    }        

    function check_changes()
    {                
        /*
        var blnChanged = true;
                
        if (document.getElementById("txtUDO").value == init_udo &&
            document.getElementById("txtDO").value == init_do &&
            !blnCommentIsChanged && !blnCompDateIsChanged)
            blnChanged = false;
        
        if (blnChanged)
            save_changes();
        else
            location.href = redirectURL;
       */
    }       
    
    function open_history()
    {
        var doc = document.getElementById("lblDocNumber").innerText;
        var popup = window.open("OpenItemHistory.aspx?doc="+doc, "OpenItemsHistory", 
            "width=980,height=600,menubar=no,status=no,resizable=yes,scrollbars=yes,toolbar=no,top=100,left=100");    

        popup.focus();
    }
    
    function alert_organization(org_code, item_id, line_num)
    {
        save_changes();
        alert("This Line belongs " + org_code + " organization.");        
        location.href = 'LineNumDetails.aspx?id='+item_id+'&num='+line_num+'&r=y';
    }
    
    function save_feedback(ctrl_btn, load_id, do_udo_required)
    {
        var valid_to_save;
        valid_to_save = true;
        
        var btn_id = ctrl_btn.id;        
        var ctrl_ddl = document.getElementById(btn_id.replace("btnSaveFdb","ddlFdbValid"));
        var valid_value = ctrl_ddl.options[ctrl_ddl.selectedIndex].value;        
        var response = document.getElementById(btn_id.replace("btnSaveFdb","txtFdbResponse")).innerText;            

        //debugger
        
        if (valid_value == 0)
        {
            var ctrl_rf_valid = document.getElementById(btn_id.replace("btnSaveFdb","rfvValid"));
            ctrl_rf_valid.style.display = "inline";
            valid_to_save = false;
        }
        if (response.length == 0)
        {
            var ctrl_rf_response = document.getElementById(btn_id.replace("btnSaveFdb","rfvResponse"));
            ctrl_rf_response.style.display = "inline";
            valid_to_save = false;
        }
        
        var txt_udo = "";
        var txt_do = "";
        var ctrl_udo = document.getElementById(btn_id.replace("btnSaveFdb","txtFdbUDO"));
        var ctrl_do = document.getElementById(btn_id.replace("btnSaveFdb","txtFdbDO"));
        if (ctrl_udo && ctrl_do)
        {
            txt_udo = ctrl_udo.value;
            txt_do = ctrl_do.value;
        }
               
        var ctrl_re_udo = document.getElementById(btn_id.replace("btnSaveFdb","revFdbUDO"));
        var ctrl_re_do = document.getElementById(btn_id.replace("btnSaveFdb","revFdbDO"));
        
        if (do_udo_required.toLowerCase() == "true" && (txt_udo == "$" || txt_udo == ""))
            ctrl_re_udo.style.display = "inline";
        if (do_udo_required.toLowerCase() == "true" && (txt_do == "$" || txt_do == ""))
            ctrl_re_do.style.display = "inline";
              
        if (valid_to_save == false || ctrl_re_udo.style.display != "none" || ctrl_re_do.style.display != "none")
            return;
                  
        var item_id = document.getElementById("txtItemID").value;
        var reviewer = document.getElementById("txtReviewerUserID").value;    
        var doc_number = document.getElementById("lblDocNumber").innerText;    
        var org_code = document.getElementById("lblOrgCode").innerText;
        //build the XML
        var obj_temp_xml = get_xml_doc_object();
        var obj_xml_parent = append_xml_element(obj_temp_xml, "save_feedback");
        //add params:
        var obj_xml_node = add_xml_element(obj_temp_xml,obj_xml_parent, "params");
        add_xml_attribute (obj_temp_xml,obj_xml_node,"item_id",item_id);
        add_xml_attribute (obj_temp_xml,obj_xml_node,"reviewer_id",reviewer);	
        add_xml_attribute (obj_temp_xml,obj_xml_node,"doc_num",doc_number);
        add_xml_attribute (obj_temp_xml,obj_xml_node,"load_id",load_id);
        add_xml_attribute (obj_temp_xml,obj_xml_node,"org_code",org_code);
        add_xml_attribute (obj_temp_xml,obj_xml_node,"valid",valid_value);
        add_xml_attribute (obj_temp_xml,obj_xml_node,"response",response);
        add_xml_attribute (obj_temp_xml,obj_xml_node,"do",txt_do);
        add_xml_attribute (obj_temp_xml,obj_xml_node,"udo",txt_udo);
    
        //alert("before send xml: " + obj_temp_xml.xml);            
        send_server_request(obj_temp_xml);
        
        //back to the source page:
        on_cancel();
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
