<script language="javascript">
<!--


//***********************************************
// load an asp xml data
//***********************************************
function load_asp_xml ( url , xml_object )
{	
	var obj_xml_http;
	var obj_xml_dom = null; 
	var bln_error = false;
    var str_err = "";
	
	// create xml http object
	try
    {
        obj_xml_http = new ActiveXObject("Msxml2.XMLHTTP");
    }
    catch(e)
    {
        try
        {
            obj_xml_http = new ActiveXObject("Microsoft.XMLHTTP");
        }
        catch(oc)
        {
            bln_error = true;
        }
    }

    if (!bln_error)
    {			    
	    // call the asp page
	    obj_xml_http.open("POST", url, false);

	    obj_xml_http.send(xml_object);
    		
	    // if the server response is ok
	    if(obj_xml_http.status == 200)
	    {
		    // get the xml dom object
		    obj_xml_dom = obj_xml_http.responseXML;
    			
		    // if there is a xml dom object
		    if(obj_xml_dom)
		    {
			    if(obj_xml_dom.documentElement)
			    {
				    // return the xml dom to the caller
				    return obj_xml_dom;
			    }
			    else
			    {
				    bln_error = true;
				    str_err = "Response XML not valid.";
				}
		    }
		    else
		    {
			    bln_error = true;
			    str_err = "Response XML not valid.";
			}
	    }
	    else
	    {
		    bln_error = true;
		    str_err = obj_xml_http.statusText;   
		}
	}
		
	if(bln_error)
	{	    	    		    
		//return xml object with error message:
		var obj_temp_xml = get_xml_doc_object();
		var obj_xml_parent = append_xml_element(obj_temp_xml, "error");
		var obj_xml_node = add_xml_element(obj_temp_xml,obj_xml_parent, "error_msg");
		add_xml_attribute (obj_temp_xml,obj_xml_node,"msg","Error occurred during saving data. " + str_err);
		return obj_temp_xml;
	}		
} 
 
//***********************************************
//general xml functions:
//***********************************************

function get_xml_doc_object()
{
    var obj_temp_xml = new ActiveXObject("MSXML2.DOMDocument");
    if (obj_temp_xml == null)
        obj_temp_xml = new ActiveXObject("MSXML.DOMDocument");
    return obj_temp_xml;
}

function add_xml_element(obj_main_xml,obj_xml_node,element_name)
{
	var new_element = obj_main_xml.createElement(element_name);
	return obj_xml_node.appendChild(new_element);
}

function append_xml_element(obj_main_xml,element_name)
{
	var new_element = obj_main_xml.createElement(element_name);
	return obj_main_xml.appendChild(new_element);
}

function add_xml_attribute(obj_main_xml,obj_xml_node,attribute_name,attribute_value,overwrite) 
{ 
	var obj_attribute = obj_xml_node.attributes.getNamedItem(attribute_name);
	
	if(!obj_attribute) 
	{ 
		var new_attribute = obj_main_xml.createAttribute(attribute_name);
		new_attribute.text = attribute_value;
		obj_xml_node.attributes.setNamedItem(new_attribute);
	} 
	else 
		if(overwrite)
			obj_attribute.text = attribute_value;
} 


// -->
</script>
