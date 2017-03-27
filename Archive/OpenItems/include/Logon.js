<script language="javascript">
<!--

    function forgot_password()
    {
        document.getElementById("lblPswd").style.display = "inline";
        document.getElementById("btnEmail").style.display = "inline";
    }
    
    function send_email()
    {
        var username = document.getElementById("txtUsername").value;
        if (username.length > 0)
        {
            var obj_temp_xml = get_xml_doc_object();
            var obj_xml_parent = append_xml_element(obj_temp_xml, "send_pswd");
            //add params:
            var obj_xml_node = add_xml_element(obj_temp_xml,obj_xml_parent, "params");
            add_xml_attribute (obj_temp_xml,obj_xml_node,"user",username);
            
            if (send_server_request(obj_temp_xml))
                alert("Your password has been sent to you. Thank you.");
        }
        else
            alert("Please enter your username.");
    }            
    
    function send_server_request(obj_params_xml)
    {
        var obj_return_xml = load_asp_xml("HTTPLogon.aspx", obj_params_xml);
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
	            alert(str_err_msg);    			
	            return false;			
            }
        }
    }  
    
// -->
</script>