
<script type="text/javascript" >
<!--

function delete_att()
{
    if (confirm('Are you sure you want to delete this file?'))
        return true;
    else
        return false;            
}

function edit_doc(doc_id)
{
    location.href = "Attachments.aspx?edit="+doc_id;
    return false;
}

function doc_approved(ctrl, doc_id, doc_type, doc_type_name, file_name, user, org)
{    
    //build the XML
    var obj_temp_xml = get_xml_doc_object();
    var obj_xml_parent = append_xml_element(obj_temp_xml, "select_doc_approved");
    //add params:
    var obj_xml_node = add_xml_element(obj_temp_xml,obj_xml_parent, "params");
    add_xml_attribute (obj_temp_xml,obj_xml_node,"doc",doc_id);
    add_xml_attribute (obj_temp_xml,obj_xml_node,"doc_type",doc_type);
    add_xml_attribute (obj_temp_xml,obj_xml_node,"doc_type_name",doc_type_name);
    add_xml_attribute (obj_temp_xml,obj_xml_node,"file_name",file_name);
    add_xml_attribute (obj_temp_xml,obj_xml_node,"user",user);
    add_xml_attribute (obj_temp_xml,obj_xml_node,"org",org);
    if (ctrl.checked)
        add_xml_attribute (obj_temp_xml,obj_xml_node,"selected","true");
    else
        add_xml_attribute (obj_temp_xml,obj_xml_node,"selected","false");
    
    //alert("before send xml: " + obj_temp_xml.xml);            
    if (!send_server_request(obj_temp_xml))    
        ctrl.checked = !ctrl.checked;
}

function include_rev_email(ctrl, doc_id, doc_title)
{
    //build the XML
    var obj_temp_xml = get_xml_doc_object();
    var obj_xml_parent = append_xml_element(obj_temp_xml, "select_doc_revision_email");
    //add params:
    var obj_xml_node = add_xml_element(obj_temp_xml,obj_xml_parent, "params");
    add_xml_attribute (obj_temp_xml,obj_xml_node,"doc",doc_id);
    if (ctrl.checked)
        add_xml_attribute (obj_temp_xml,obj_xml_node,"selected","true");
    else
        add_xml_attribute (obj_temp_xml,obj_xml_node,"selected","false");
    
    //alert("before send xml: " + obj_temp_xml.xml);            
    if (!send_server_request(obj_temp_xml))    
        ctrl.checked = !ctrl.checked;    
        
    var x = document.getElementsByTagName("input");
    for(var i = 0; i<x.length; i++ )
    {
        if (x[i].type == "checkbox" && x[i].title == doc_title)
            x[i].checked = ctrl.checked;    
    }
}

function include_in_email(ctrl, doc_id, doc_title)
{   
    //build the XML
    var obj_temp_xml = get_xml_doc_object();
    var obj_xml_parent = append_xml_element(obj_temp_xml, "select_doc_to_email");
    //add params:
    var obj_xml_node = add_xml_element(obj_temp_xml,obj_xml_parent, "params");
    add_xml_attribute (obj_temp_xml,obj_xml_node,"doc",doc_id);
    if (ctrl.checked)
        add_xml_attribute (obj_temp_xml,obj_xml_node,"selected","true");
    else
        add_xml_attribute (obj_temp_xml,obj_xml_node,"selected","false");
    
    //alert("before send xml: " + obj_temp_xml.xml);            
    if (!send_server_request(obj_temp_xml))    
        ctrl.checked = !ctrl.checked;    
        
    var x = document.getElementsByTagName("input");
    for(var i = 0; i<x.length; i++ )
    {
        if (x[i].type == "checkbox" && x[i].title == doc_title)
            x[i].checked = ctrl.checked;    
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

function send_email(mode)
{
    //display Send Email screen:
    var popup = window.open("../EmailForm.aspx?mode="+mode, "OpenItemsEmail", 
                "width=600,height=600,menubar=no,status=no,resizable=yes,scrollbars=yes,toolbar=no,top=200,left=200");    

    popup.focus();
    
    return false;    
}

function email_reviewer(reviewer_user_id)
{
    //display Send Email screen:
    var popup = window.open("../EmailForm.aspx?mode=r&uid="+reviewer_user_id, "OpenItemsEmail", 
                "width=600,height=600,menubar=no,status=no,resizable=yes,scrollbars=yes,toolbar=no,top=200,left=200");    

    popup.focus();
    
    return false;
}

function view_doc(doc_id)
{
    window.open("../Viewer.aspx?id="+doc_id,"OIViewer", 
            "width=900,height=600,menubar=yes,status=yes,resizable=yes,scrollbars=yes,toolbar=no,top=100,left=100");                
}

function window_close()
{
//debugger;

    var str_result = "";
    var file = "";
    
    if (prop_array == 'undefined' || prop_array == null)
    {
        prop_array = new Array();            
        prop_array = document.getElementById("txtPropertyArray").value.split('&&&');            
    }
        
    var count = prop_array.length - 1;
    
    str_result = count + "&&&";
    if (count > 0)
    {
        for (var i = 0; i<count; i++)
        {
            var arr = new Array();
            arr = prop_array[i].split('|');
            
            file = arr[7] + "|" + arr[1] + " - " + arr[0];
            
            if (str_result.indexOf(file) == -1)
            {            
                str_result = str_result + file + "&&&";
            }
        }
    }
    else
    {
    }
        
    //alert(str_result );
    try
    {
    //debugger
        opener.rebuild_att_table(str_result, document.getElementById("txtReloadOpener").value);
        //window.opener.location.href="OIBA53Review.aspx";
        


    }
    catch(e) {}
    //opener.Reload();   
    self.close();
}

function display_properties(row_id)
{
    try
    {
        if (document.getElementById("txtDisplayFlag").value == "0")
            return;
            
        if (prop_array == 'undefined' || prop_array == null)
        {
            prop_array = new Array();            
            prop_array = document.getElementById("txtPropertyArray").value.split('&&&');            
        }
        if (row_id < prop_array.length)
        {
            var arr = new Array();
            arr = prop_array[row_id].split('|');
            
            document.getElementById("lblFileName").innerText = arr[0];
            document.getElementById("lblUploadDate").innerText = arr[1];
            document.getElementById("lblUploadUser").innerText = arr[2];
            document.getElementById("lblDocType").innerText = arr[3];
            document.getElementById("lblLineNum").innerText = arr[4];
            document.getElementById("lblComments").innerText = arr[5];
            document.getElementById("lblEmailDate").innerText = arr[6];
        }
        else
        {
            //display empty labels:
            document.getElementById("lblFileName").innerText = "";
            document.getElementById("lblUploadDate").innerText = "";
            document.getElementById("lblUploadUser").innerText = "";
            document.getElementById("lblDocType").innerText = "";
            document.getElementById("lblLineNum").innerText = "";
            document.getElementById("lblComments").innerText = "";
            document.getElementById("lblEmailDate").innerText = "";
        }
    }
    catch(e){}
}

function open_history()
{
    var doc = document.getElementById("lblDocNumber").innerText;
    var popup = window.open("OpenItemHistory.aspx?doc="+doc+"&att=yes", "OpenItemsHistory", 
        "width=980,height=600,menubar=no,status=no,resizable=yes,scrollbars=yes,toolbar=no,top=200,left=200");    

    popup.focus();
}

//  -->
</script>