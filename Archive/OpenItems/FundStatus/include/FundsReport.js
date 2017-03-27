var popup_alert; 

function onmouse(action,e,css)
{               
    var s=window.event.srcElement
    while(s!=null && s.tagName!="TR") s=s.parentNode;
    if(s!=null && s.className=="reportRow")
        if(action==1)
            s.style.backgroundColor = '#cccccc';
        else
            s.style.backgroundColor = '';
            
    var t=e.target!=null?e.target:e.srcElement
    if(t.tagName.toUpperCase()=="TD"&&t.className.indexOf('reportAmountLink')>=0)
        t.className=css;
}
             
function close_child()
{ try { popup_alert.close(); } catch(e){}} 

function recalc_chart()
{
    var org="";
    var fy="";    
    var bl="";   
    var list = document.getElementById("ctrlCriteria_ddlOrganization");
    if(list)
        org = list.options[list.selectedIndex].value;
    list = document.getElementById("ctrlCriteria_ddlBusinessLine");
    if(list)
        bl = list.options[list.selectedIndex].value;            
    list = document.getElementById("ctrlCriteria_ddlFiscalYear");
    if(list)
        fy = list.options[list.selectedIndex].value;
        
     //build the XML
    var obj_temp_xml = get_xml_doc_object();
    var obj_xml_parent = append_xml_element(obj_temp_xml, "recalc_report");
    //add params:
    var obj_xml_node = add_xml_element(obj_temp_xml,obj_xml_parent, "params");
    add_xml_attribute (obj_temp_xml,obj_xml_node,"bl",bl);
    add_xml_attribute (obj_temp_xml,obj_xml_node,"org",org);
    add_xml_attribute (obj_temp_xml,obj_xml_node,"fy",fy);
    
    send_server_request(obj_temp_xml);
    
    alert2('Report is recalculating.<br/>See it after a few minutes.','Fund Status message');
}

function send_server_request(obj_params_xml)
{
    var obj_return_xml = load_asp_xml("HTTPFunds.aspx", obj_params_xml);
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
    
function reload_report()
{
    var table = document.getElementById("tblData");
    if (table)
    { if (table.rows.length > 0)
        { var submit_button = document.getElementById("ctrlCriteria_btnSubmit");
            if (submit_button)
            { submit_button.click(); }
        }
    }
}

function reload_rwa()
{
    var org="";
    var fy="";       
    var rwa="";
    var month="";
    var list = document.getElementById("ctrlCriteria_ddlOrganization");
    if(list)
        org = list.options[list.selectedIndex].value;            
    list = document.getElementById("ctrlCriteria_ddlFiscalYear");
    if(list)
        fy = list.options[list.selectedIndex].value;
    list = document.getElementById("ctrlCriteria_ddlBookMonth");
    if(list)
        month = list.options[list.selectedIndex].value;
        
    if (reload_rwa.arguments.length>0)
        rwa = reload_rwa.arguments[0];
        
     //build the XML
    var obj_temp_xml = get_xml_doc_object();
    var obj_xml_parent = append_xml_element(obj_temp_xml, "recalc_rwa");
    //add params:
    var obj_xml_node = add_xml_element(obj_temp_xml,obj_xml_parent, "params");
    add_xml_attribute (obj_temp_xml,obj_xml_node,"org",org);
    add_xml_attribute (obj_temp_xml,obj_xml_node,"fy",fy);
    add_xml_attribute (obj_temp_xml,obj_xml_node,"m",month);
    add_xml_attribute (obj_temp_xml,obj_xml_node,"rwa",rwa);        
    
    if (send_server_request(obj_temp_xml))
        reload_report();
}

function get_link(str_param)
{
    //alert(str_param);
    location.href = unescape(str_param);                               
}

function open_popup(str_params)
{
    var popup = window.open("FundsEntryData.aspx?"+unescape(str_params), "FundStatus", 
            "width=860,height=600,menubar=no,status=no,resizable=yes,scrollbars=yes,toolbar=no,top=200,left=200");            
    popup.focus();
}

function open_rwa_popup(str_params)
{
    var popup = window.open("RWAProjection.aspx?"+unescape(str_params), "FundStatus", 
            "width=620,height=720,menubar=no,status=no,resizable=yes,scrollbars=yes,toolbar=no,top=200,left=200");            
    popup.focus();
}       


