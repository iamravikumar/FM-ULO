<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Popups/Popup.master" AutoEventWireup="true"  CodeFile="SearchPerson.aspx.cs" Inherits="GSA.OpenItems.Web.SearchPerson" %>

<asp:Content ID="Content1" ContentPlaceHolderID="title" runat="Server">
    Open Items - Search Person
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="head" runat="Server">
    <!--#include virtual="include/HTTPService.js" -->
    <script type="text/javascript">

        function ShowHideControls() {
            //debugger
            //var contacts = document.getElementById("gvPersons");
            //var rowscount = contacts.rows.length;
            var note_top = document.getElementById("lblNoteTop");
            var lbl_info = document.getElementById("lblInfo");
            var ddl = document.getElementById("ddlRoles");
            var txt_lname = document.getElementById("txtLastName");
            var txt_fname = document.getElementById("txtFirstName");
            var btn_search = document.getElementById("btnSearch");
            var note = document.getElementById("note");
            var td_ln = document.getElementById("td_ln");
            var td_fn = document.getElementById("td_fn");
            var grid_row = document.getElementById("grid_row");
            var role = ddl.options[ddl.selectedIndex].text;
            var show = document.getElementById('hdnShow'); //grid_row; 
            //To add the contact please double click on row below. Otherwise click 'Cancel'.


            if (role == "-- please select --") {
                txt_lname.style.display = "none";
                txt_fname.style.display = "none";
                btn_search.style.display = "none";
                note.style.display = "none";
                td_fn.style.display = "none";
                td_ln.style.display = "none";
                grid_row.style.display = "none";
                //lbl_info.innerHTML = "";
                note_top.innerHTML = "Please select the 'Contact Role'";
                show.value = "0";
                //Please select the 'Contact Role'
            }
            else {
                //debugger
                txt_lname.style.display = "";
                txt_fname.style.display = "";
                btn_search.style.display = "";
                //note.style.display = "";
                td_ln.style.display = "";
                td_fn.style.display = "";
                note_top.innerHTML = "Please enter last name and/or first name and then click on 'Search' button";
                grid_row.style.display = "";
                //lbl_info.innerHTML = "";
                show.value = "1";
                //var Length= contacts.options.length;

                if (trim(txt_lname.value) == "" && trim(txt_fname.value) == "") {
                    lbl_info.style.display = "none";
                }
                else {
                    lbl_info.style.display = "";
                    lbl_info.className = "brown_text";
                }

            }
        }

        function trim(str) {
            //     if(!str || typeof str != 'string')  
            //     {      
            //        return "";    
            return str.replace(/^[\s]+/, '').replace(/[\s]+$/, '').replace(/[\s]{2,}/, ' ');
        }

        function on_email_select(email) {
            try {
                opener.insert_email_address(email);
            }
            catch (e) { }
        }

        function on_select(id, name, phone) {
            try {
                var list = document.getElementById("ddlRoles");
                var role = list.options[list.selectedIndex].text;

                if (role == "-- please select --") {
                    alert("Please select contact role and then double click on row to select the person.");
                    return;
                }

                //first update the contact in the DB:
                if (update_contact(id, role, name)) {
                    //second - return to the main page and add the contact to the table (due to not to reload the contacts table)
                    opener.insert_contact_row(role, name, phone);
                    self.close();
                }
            }
            catch (e) { }
        }

        function update_contact(id, role, name) {
            //get additional values:
            var item = document.getElementById("txtItem").value;
            var org = document.getElementById("txtOrg").value;
            var doc = document.getElementById("txtDoc").value;
            //build the XML
            var obj_temp_xml = get_xml_doc_object();
            var obj_xml_parent = append_xml_element(obj_temp_xml, "add_contact");
            //add params:
            var obj_xml_node = add_xml_element(obj_temp_xml, obj_xml_parent, "params");
            add_xml_attribute(obj_temp_xml, obj_xml_node, "item", item);
            add_xml_attribute(obj_temp_xml, obj_xml_node, "doc", doc);
            add_xml_attribute(obj_temp_xml, obj_xml_node, "org", org);
            add_xml_attribute(obj_temp_xml, obj_xml_node, "pid", id);
            add_xml_attribute(obj_temp_xml, obj_xml_node, "role", role);
            add_xml_attribute(obj_temp_xml, obj_xml_node, "name", name);

            //alert("before send xml: " + obj_temp_xml.xml);            
            return send_server_request(obj_temp_xml);
        }

        function send_server_request(obj_params_xml) {
            var obj_return_xml = load_asp_xml("HTTPServer.aspx", obj_params_xml);
            var str_err_msg = "";
            if (obj_return_xml) {

                if (obj_return_xml.documentElement.nodeName == 'ok')
                    return true;
                else {
                    try {
                        var obj_xml_node = obj_return_xml.documentElement.firstChild;
                        str_err_msg = obj_xml_node.attributes.getNamedItem("msg").value;
                    }
                    catch (e) { }
                    alert("Error in saving data." + "\n" + str_err_msg);
                    return false;
                }
            }
        }



    </script>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="contentTitle" runat="Server">
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="content" runat="Server">
    <input type="hidden" id="txtItem" runat="server" />
    <input type="hidden" id="txtDoc" runat="server" />
    <input type="hidden" id="txtOrg" runat="server" />
    <table width="100%">
        <tr>
            <td colspan="4">
                <asp:Label ID="lblNoteTop" Width="100%" CssClass="brown_text" Font-Size="12px" runat="server" Text="Please select the 'Contact Role'"></asp:Label>
            </td>
        </tr>
        <tr>
            <td>
                <label id="lblRole" runat="server" class="regBldText">Contact Role: </label>
            </td>
            <td id="td_ln" runat="server">
                <label class="regBldText">Last Name: </label>
            </td>
            <td id="td_fn" runat="server">
                <label class="regBldText">First Name: </label>
            </td>
            <td></td>
        </tr>
        <tr>
            <td>
                <asp:DropDownList ID="ddlRoles" AutoPostBack="false" onChange="ShowHideControls()" runat="server" CssClass="regText" />
            </td>
            <td>
                <asp:TextBox ID="txtLastName" runat="server" CssClass="regText" Width="100px" />
            </td>
            <td>
                <asp:TextBox ID="txtFirstName" runat="server" CssClass="regText" Width="100px" />
            </td>
            <td>
                <asp:Button ID="btnSearch" runat="server" CssClass="button" Text="Search" />
            </td>
        </tr>
        <tr style="height: 20px">
            <td colspan="4"></td>
        </tr>
        <tr id="note" runat="server">
            <td colspan="4"></td>
        </tr>
    </table>
    <span id="grid_row" runat="server" title="double click on row to select person">
        <asp:Label ID="lblInfo" Width="100%" CssClass="brown_text" Font-Size="12px" runat="server" Text="To add the contact please double click on row below. Otherwise click 'Cancel'." />
        <asp:GridView ID="gvPersons" runat="server" CellPadding="0" AllowPaging="false" AutoGenerateColumns="false"
            AllowSorting="false" CssClass="" Width="100%" HorizontalAlign="Center"
            CellSpacing="0" EnableViewState="false" HeaderStyle-CssClass="th"
            UseAccessibleHeader="true" RowStyle-CssClass="TDeven" AlternatingRowStyle-CssClass="TDodd">
            <Columns>
                <asp:BoundField DataField="Organization" HeaderText="Organization" />
                <asp:BoundField DataField="FullName" HeaderText="Name" />
                <asp:BoundField DataField="PersonnelID" Visible="false" />
                <asp:BoundField DataField="Phone" HeaderText="Office phone" />
                <asp:BoundField DataField="Email" HeaderText="Email" />
            </Columns>
        </asp:GridView>
    </span>
    <br />
    <table width="100%">
        <tr>
            <td align="left">
                <input type="button" id="btnClose" onclick="javascript: self.close();" value="Close" title="Close" class="button" />
                <input id="hdnShow" runat="server" name="hdnShow" type="hidden" value="0" />
            </td>
        </tr>
    </table>
</asp:Content>
