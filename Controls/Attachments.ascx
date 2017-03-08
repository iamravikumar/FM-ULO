<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Attachments.ascx.cs" Inherits="GSA.OpenItems.Web.Controls_Attachments" %>
<div id="divAttachments" runat="server">
    <table width="100%" id="tblAttachments" border="0" runat="server">

        <tr class="lrgBldText" style="background-color: #d3d3d3; height: 42px">
            <td style="border-bottom: solid 1px tan;">
                <table>
                    <tr>
                        <td style="width: 50%">Attachment Section</td>
                        <td>
                            <a href="../docs/Acceptable Documentation for High Risk ULO Review.xlsx">
                                <img src="../images/btn_acceptable_documentation_3.gif" style="border-style: none" /></a>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
        <tr style="height: 30px">
            <td style="border-bottom: solid 1px tan">
                <span class="regBldGreyText" style="vertical-align: super;">Documents 
                </span>
                <span style="vertical-align: super">
                    <asp:Label ID="lblAttCount" runat="server" CssClass="regBldGreyText" />
                </span>
                &nbsp;
                <span style="vertical-align: top">
                    <input type="image" src="~/images/btn_add_manage_attachments.gif" id="btnAttachments" runat="server" title="You can see up to 3 attachments here. To see more, please click on 'ADD AND MANAGE ATTACHMENTS' icon" alt="View All / Add New Attachment" />
                </span>
            </td>
        </tr>

        <tr>
            <td>
                <table id="tblAttLinks" class="regBldGreyText" style="vertical-align: middle" runat="server" border="0"></table>
            </td>

        </tr>
        <tr>
            <td>
                <asp:Label ID="lblAttachErr" runat="server" CssClass="regBldRedText" /></td>
        </tr>




        <tr>
            <td>
                <div id="div_email">
                    <input type="image" src="~/images/btn_email_user_1.gif" id="btnEmail" style="width: 194px; height: 28px;" onclick="javascript:return ExecuteConfirm();" title="Click to send a reminder to the reviewer to attach supporting documentation" causesvalidation="false" runat="server" alt="send e-mail to reviewer" onserverclick="btnEmail_ServerClick" /></div>
                <input type="hidden" name="hdnSendEmail" id="hdnSendEmail" runat="server" />
                <input type="hidden" name="hdnUserIsAdmin" id="hdnUserIsAdmin" runat="server" />
                <input type="hidden" name="hdnReviewerEmail" id="hdnReviewerEmail" runat="server" />
                <input type="hidden" name="hdnDocNum" id="hdnDocNum" runat="server" />
                <asp:HiddenField runat="server" ID="hdnSendEmailAfterConfirm" />
            </td>

        </tr>
    </table>
</div>

<script type="text/javascript">
    
function ExecuteConfirm()         
{   
//debugger
 
    var reviewer_email  = document.getElementById('ctrlAttachments_hdnReviewerEmail').value; 
	var returnValue= confirm('Are you sure you want to send email to ' + reviewer_email + '?'); 
	
	           
	if(returnValue)            
	{             
	   document.getElementById('ctrlAttachments_hdnSendEmailAfterConfirm').value="1"; 
	   return true;                                    
	} 
	else
	{
	    document.getElementById('ctrlAttachments_lblAttachErr').innerText=""; 
	    return false;
	}                                    
} 

function ClearErrMsg()
{
    document.getElementById('ctrlAttachments_lblAttachErr').innerText=""; 
}

function ShowAlert(s)
{
    alert(s);
}

   
    function view_att()
    {   
    //debugger     
        var popup = window.open("Attachments.aspx", "OpenItems", 
            "width=1100,height=750,menubar=no,status=yes,resizable=yes,scrollbars=yes,toolbar=no,top=100,left=100");    
        
        popup.focus();
        
        return false;
    }
    
    function rebuild_att_table(str_array)
    {
        
    //debugger
        var arr = new Array();
        arr = str_array.split("&&&");
        
        var count = Number(arr[0]);
       
                    
        if (count > 0)
        {
            document.getElementById("ctrlAttachments_lblAttCount").innerText = " ("+count+") ";
            
            if( document.getElementById("ctrlAttachments_btnEmail")!=null)
            {
                document.getElementById("ctrlAttachments_btnEmail").style.visibility = "hidden";
            }
            
            var tb = document.getElementById("ctrlAttachments_tblAttLinks");
            if (tb!=null)
            {
                var tb_rows = tb.rows.length;
                for (var i = 0 ; i < tb_rows; i++)
                {
                    tb.deleteRow(0);
                }
                
                count = arr.length - 2;
                for (var i = 0; i < count && i < 3; i++)
                {
                    var row_arr = new Array();
                    row_arr = arr[i+1].split("|");
                    
                    tb.insertRow(i);
                    var cell_t = tb.rows[i].insertCell(0);
                    cell_t.innerHTML = "<a  href='javascript:view_doc("+row_arr[0]+");' ><img src='../images/btn_view_file.gif' alt='' title='view file'  style='border:0;'/><span style='vertical-align:top' class='regBldBlueText'>"+row_arr[1]+"</span></a>";
                }                               
            }
            
        }
        else
        {
        //debugger
            document.getElementById("ctrlAttachments_lblAttCount").innerText = "(0)";
            
            if( document.getElementById("ctrlAttachments_hdnUserIsAdmin").value=="1")
            {
                if( document.getElementById("ctrlAttachments_btnEmail")!=null)
                {
                    document.getElementById("ctrlAttachments_btnEmail").style.visibility = "visible";
                }
            }
            
            var tb = document.getElementById("ctrlAttachments_tblAttLinks");
            var tb_rows = tb.rows.length;
            
            if (tb_rows=="1")
            {
                tb.deleteRow(0);
            }
            else
               {
                    for (var i = 0 ; i < tb_rows; i++)
                    {
                        tb.deleteRow(0);
                    }
                }
        }
        
        document.getElementById('ctrlAttachments_lblAttachErr').innerText=""; 
        
    }
    
    function view_doc(doc_id)
    {
        window.open("../Viewer.aspx?id="+doc_id,"OIViewer", 
            "width=900,height=600,menubar=yes,status=yes,resizable=yes,scrollbars=yes,toolbar=no,top=100,left=100");
    }
    
</script>
