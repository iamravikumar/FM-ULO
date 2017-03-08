
<script type="text/javascript" >
<!--
    
    var arr_code_list = null;
    var arr_code_desc_list = null;
    var arr_code_valid = null;
    var arr_display_addon = null;
    var arr_addon_desc = null;
    
    //var original_selected_just; => declared on the server side
    //var original_add_on; => declared on the server side

    function on_code_select(ctrlCode)
    {
        init_arrays();
                
        var selected_code = ctrlCode.options[ctrlCode.selectedIndex].value;
        for (var i=0; i<arr_code_list.length; i++)
        {
            if (arr_code_list[i] == selected_code)
            {
                document.getElementById("lblCode").innerText = arr_code_list[i];
                break;
            }
        }
        return false;
    }
    
    function init_arrays()
    {
        if (arr_code_list == null || arr_code_desc_list == null || arr_code_valid == null)
        {
            arr_code_list = document.getElementById("txtCodeList").value.split('|');
            arr_code_desc_list = document.getElementById("txtCodeDescList").value.split('|');
            arr_code_valid = document.getElementById("txtCodeValidation").value.split('|');
        }
    }
    
    function on_valid_select(ctrlValid)
    {
        init_arrays();
        
        var selected_valid = ctrlValid.options[ctrlValid.selectedIndex].value;
        var ctrlCode = document.getElementById("ddlCode");
        
        clear_list(ctrlCode);        
        add_options(ctrlCode, selected_valid);                
    }
    
    function clear_list(ctrl)
    {    
        while (ctrl.length>0)
        {
            ctrl.remove(ctrl.length-1);
        }
        //clear code message that might stay from previous selection:
        document.getElementById("lblCode").innerText = "";
        //add first empty option as default:
        var option = document.createElement("option");
        option.text = "";
        option.value = "";
        ctrl.add(option);
    }
    
    function add_options(ctrlCode, valid_value)
    {        
        for(var x = 0; x < arr_code_valid.length - 1; x++)
        {
            if (arr_code_valid[x] == valid_value)
            {
                var option = document.createElement("option");
                option.text = arr_code_desc_list[x];
                option.value = arr_code_list[x];
                ctrlCode.add(option);
            }
        }
    }
    
    function on_justification_select(ctrlJust)
    {
        if (arr_display_addon == null || arr_addon_desc == null)
        {
            arr_display_addon = document.getElementById("txtDisplayAddOn").value.split('|');
            arr_addon_desc = document.getElementById("txtAddOnDescList").value.split('|');
        }
        
        if (ctrlJust.options[ctrlJust.selectedIndex].value == "6")
        {
            document.getElementById("lblJustAddOnDesc").innerText = "";
            document.getElementById("lblJustAddOnDesc").style.visibility = "hidden";
            document.getElementById("txtJustAddOn").innerText = "";
            document.getElementById("txtJustAddOn").style.visibility = "hidden";
            
            document.getElementById("txtJustOther").style.visibility = "visible";
            document.getElementById("lblJustOther").style.visibility = "visible";
        }
        else
        {
            document.getElementById("txtJustOther").style.visibility = "hidden";
            document.getElementById("lblJustOther").style.visibility = "hidden";
            
            if (arr_display_addon[ctrlJust.selectedIndex] == "True")
            {
                document.getElementById("lblJustAddOnDesc").innerText = arr_addon_desc[ctrlJust.selectedIndex];
                document.getElementById("lblJustAddOnDesc").style.visibility = "visible";
                document.getElementById("txtJustAddOn").style.visibility = "visible";
                if (ctrlJust.options[ctrlJust.selectedIndex].value == original_selected_just)
                    document.getElementById("txtJustAddOn").innerText = original_add_on;
                else
                    document.getElementById("txtJustAddOn").innerText = "";
            }
            else
            {
                document.getElementById("lblJustAddOnDesc").innerText = "";
                document.getElementById("lblJustAddOnDesc").style.visibility = "hidden";
                document.getElementById("txtJustAddOn").innerText = "";
                document.getElementById("txtJustAddOn").style.visibility = "hidden";
            }
        }
    }
    
    function rbl_display_Just_explanation(strIndex)
    {
        //debugger
        if (arr_display_addon == null || arr_addon_desc == null)
        {
            arr_display_addon = document.getElementById("txtDisplayAddOn").value.split('|');
            arr_addon_desc = document.getElementById("txtAddOnDescList").value.split('|');
        }

            document.getElementById("txtJustOther").style.visibility = "hidden";
            document.getElementById("lblJustOther").style.visibility = "hidden";
//            alert(arr_display_addon);
//            alert(ctrlJust);
            if (arr_display_addon[strIndex] == "True")
            {
                document.getElementById("lblJustificationExplanation").innerText = arr_addon_desc[strIndex];
                document.getElementById("lblJustificationExplanation").style.visibility = "visible";
                document.getElementById("lblJustAddInfo").style.visibility="visible";
                document.getElementById("txtAddJustification").style.visibility="visible";
                //document.getElementById("txtJustAddOn").style.visibility = "visible";
//                if (ctrlJust.options[ctrlJust.selectedIndex].value == original_selected_just)
//                    document.getElementById("txtJustAddOn").innerText = original_add_on;
//                else
//                    document.getElementById("txtJustAddOn").innerText = "";
            }
            else
            {
                document.getElementById("lblJustificationExplanation").innerText = "";
                document.getElementById("lblJustificationExplanation").style.visibility = "hidden";
                document.getElementById("lblJustAddInfo").style.visibility="hidden";
                document.getElementById("txtAddJustification").style.visibility="hidden";
//                document.getElementById("txtJustAddOn").innerText = "";
//                document.getElementById("txtJustAddOn").style.visibility = "hidden";
            }
        
    }
    function open_history()
    {
        var doc = document.getElementById("lblDocNumber").innerText;
        var popup = window.open("OpenItemHistory.aspx?doc="+doc, "OpenItemsHistory", 
            "width=980,height=600,menubar=no,status=no,resizable=yes,scrollbars=yes,toolbar=no,top=100,left=100");    

        popup.focus();
    }        
    
    function on_save()
    {
        var str_msg = "";  
        var ctrlValid = document.getElementById("ddlValid");        
        var ctrlCode = document.getElementById("ddlCode");
        var ctrlJustification = document.getElementById("<%=rblJustification.ClientID%>");
    

        if (ctrlValid.options[ctrlValid.selectedIndex].value == "0" || ctrlValid.options[ctrlValid.selectedIndex].value == "")
            str_msg = str_msg + "Please select 'Valid' value.\n";
        if (ctrlCode.options[ctrlCode.selectedIndex].value == "0" || ctrlCode.options[ctrlCode.selectedIndex].value == "")
            str_msg = str_msg + "Please select 'Code' value.\n";
      
        var radio = ctrlJustification.getElementsByTagName("input"); 
     
        var rbChecked = null;  
        for(var i=0; i<radio.length; i++)
        {
            if(radio[i].checked)
            {
                rbChecked = true;
            }
        }
        
      if(!rbChecked)
       {
            str_msg = str_msg + "Please select 'Justification' value.\n";
       }
       else
       {
        if (document.getElementById("txtAddJustification").style.visibility=="visible" && document.getElementById("txtAddJustification").value == "")
            str_msg = str_msg + "Please fill additional Information field for current Justification.\n";
       }
        
//        if (ctrlJustification.options[ctrlJustification.selectedIndex].value == "0" || ctrlJustification.options[ctrlJustification.selectedIndex].value == "")
//            str_msg = str_msg + "Please select 'Justification' value.\n";
//        else
//        {
//            if (ctrlJustification.options[ctrlJustification.selectedIndex].value < 6 && document.getElementById("txtJustAddOn").value == "")
//                str_msg = str_msg + "Please fill additional description field for current Justification.\n";
//        }
//        if (ctrlJustification.options[ctrlJustification.selectedIndex].value == "6" && document.getElementById("txtJustOther").value == "")
//            str_msg = str_msg + "Please enter your text for 'Justification' value.\n";
        
        if (str_msg != "")
        {
            str_msg = str_msg + "This is required field.";
            alert(str_msg);
            return false;
        }
        else
            return true;
         
    }
    
//  -->
</script>


