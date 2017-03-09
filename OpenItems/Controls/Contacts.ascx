<%@ Control Language="C#" AutoEventWireup="true" Inherits="GSA.OpenItems.Web.Controls_Contacts" Codebehind="Contacts.ascx.cs" %>
<div id="divContacts" runat="server" >
    <table width="450px" id="tblContacts" runat="server" border="0" >
        
        <tr class="lrgBldText" style="background-color:#d3d3d3;height:42px;border-bottom:solid 1px tan;">
            <td colspan="4" style="width:100%;border-bottom:solid 1px tan;" >
                <table width="100%">
                    <tr>
                        <td>Contact Section</td>
                    </tr>
                </table>
            </td>
        </tr>
        <tr style="height:30px">
            <td colspan="4" style="border-bottom:solid 1px tan">
                <span  style="vertical-align:baseline" class="regBldGreyText">
                    Contact List: 
                </span>
                &nbsp;&nbsp;&nbsp;&nbsp;
                <span  style="vertical-align:top">
                    <input type="image" src="~/images/btn_add_contact.gif" id="btnAddContact" runat="server"  
                    onclick="javascript:return add_contact();" alt="Add New Contracting Officer / Project Manager" />
                </span>
            </td>
        </tr>
        <tr>
            <td colspan="4" >
                <asp:Label ID="lblContactsErr" runat="server" CssClass="regBldRedText" Visible="false" />
             </td>
        </tr>
        
        
        
        
        <tr style="display:none" >
            <td style="width:22px;"></td>
            <td class="regBldBlueText" style="width:170px;">Contracting Officer by CO </td>
            <td><asp:Label ID="lblCOfficer" runat="server" CssClass="regText" /></td>
            <td></td>
        </tr>        
    </table>    
</div>

<script type="text/javascript" >

    function add_contact()
    {
    //debugger;
        var org_code = document.getElementById("lblOrgCode").innerText;
        var item_id = document.getElementById("txtItemID").value;
        var doc_num = document.getElementById("lblDocNumber").innerText;
        
        if (item_id == "" || org_code == "" || doc_num == "")
        {
            alert("Error in the page. Missing parameters for this Item.");
        }
            
        var popup = window.open("../SearchPerson.aspx?item="+item_id+"&org="+org_code+"&doc="+doc_num, "OpenItems", 
            "width=600,height=400,menubar=no,status=no,resizable=yes,scrollbars=yes,toolbar=no,top=200,left=200");    
        
        popup.focus();
        
        return false;
    }

    function insert_contact_row(contact_role, contact_name, contact_phone)
    {
        
        tb = document.getElementById('ctrlContacts_tblContacts');
        if (tb!=null)
        {
            var row_id = tb.rows.length;
            tb.insertRow(row_id);
            
            var cell_0 = tb.rows[row_id].insertCell(0);
            cell_0.innerHTML = "<img id='btnDeleteContact' src='../images/btn_contact_delete.gif' onclick='javascript:return remove_contact(this);' class='icDelete' alt='Remove Contact' />";
            var cell_1 = tb.rows[row_id].insertCell(1);
            cell_1.innerHTML = "<label class='regBldBlueText' >"+contact_role+"</label>";
            var cell_2 = tb.rows[row_id].insertCell(2);
            cell_2.innerHTML = "<label class='regText' >"+contact_name+"</label>";
            var cell_3 = tb.rows[row_id].insertCell(3);
            cell_3.innerHTML = "<label class='regText' >"+contact_phone+"</label>";
        }
    }
    
    
    function remove_contact(ctrl)
    {   
        var doc_num = document.getElementById("lblDocNumber").innerText;
        var org_code = document.getElementById("lblOrgCode").innerText;
        var item_id = document.getElementById("txtItemID").value;
        var contact_name = ctrl.parentElement.parentElement.cells[2].innerText;
        var contact_role = ctrl.parentElement.parentElement.cells[1].innerText;
        
        if (confirm ("Are you sure you want to delete " + contact_role + " " + contact_name + " as a contact person for this OpenItem?"))
        {        
            //alert ("doc_num: " + doc_num + ", org_code" + org_code + ", contact_name: " + contact_name + ", contact_role: " + contact_role);
             //build the XML
            var obj_temp_xml = get_xml_doc_object();
            var obj_xml_parent = append_xml_element(obj_temp_xml, "delete_contact");
            //add params:
            var obj_xml_node = add_xml_element(obj_temp_xml,obj_xml_parent, "params");
            add_xml_attribute (obj_temp_xml,obj_xml_node,"doc",doc_num);
            add_xml_attribute (obj_temp_xml,obj_xml_node,"org",org_code);	
            add_xml_attribute (obj_temp_xml,obj_xml_node,"item",item_id);	
            add_xml_attribute (obj_temp_xml,obj_xml_node,"name",contact_name);	
            add_xml_attribute (obj_temp_xml,obj_xml_node,"role",contact_role);	
            
            //alert("before send xml: " + obj_temp_xml.xml);            
            if (send_server_request(obj_temp_xml))
            {
                var row_id =  ctrl.parentElement.parentElement.rowIndex;
                ctrl.parentElement.parentElement.parentElement.deleteRow(row_id);
            }                        
        }
        
        return false;
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
    
</script>