<%@ Control Language="C#" AutoEventWireup="true" Inherits="GSA.OpenItems.Web.Controls_CriteriaFields" Codebehind="CriteriaFields.ascx.cs" %>

<table width="70%"  cellspacing="0" cellpadding="0" border="0" style="border-color:Red">
      <tr>
        <td style="width:25%"   class="lrgBldText">Select Review: </td>     
        <td style="width:75%">
	        <asp:DropDownList ID="ddlLoad" runat="server"  Width="95%" AutoPostBack="true" />
	    </td> 
      </tr>
      <tr style="height:40px">
          <td colspan="2">
            <table style="width:100%;border-color:Yellow"  border="0">
                <tr>
                    <td style="width:10%" class="regBldText" >Due Date:  </td>
                    <td  class="regText" >
                        <asp:Label ID="lblDueDate" runat="server" />
                    </td>
                    <td  style="width:10%" class="regBldText" >Source:  </td>
                    <td  class="regText" >
                        <asp:Label ID="lblSource" runat="server" />
                    </td>
                    <td   style="width:14%" class="regBldText" >Review Round:  </td>
                    <td  class="regText" >
                        <asp:Label ID="lblRound" runat="server" />
                    </td>
                </tr>
            </table>
          </td>
      </tr>
      <tr>
        <td />
        <td >
            <asp:CheckBox ID="chkUnArchive" runat="server" Text="UnArchive Review"  Visible="false" CssClass="regBldGreyText" AutoPostBack="true" />
            <asp:CheckBox ID="chkArchive" runat="server" Text="Archive Review"  Visible="false" CssClass="regBldGreyText" AutoPostBack="true" />
        </td>
        
        <td colspan="2" >
            <asp:TextBox ID="txtLoadTypeID" Visible="false" runat="server" Width="30px" /></td>
      </tr>
      </table>
            <br />
            <hr class="thickHR" />
      <table width="100%" border="0" cellspacing="0" cellpadding="0" id="tblSearch" runat="server" >
      <tr>
        <td width="14%" class="lrgBldText">Search Criteria: </td>
        <td><label class="regBldGreyText">DocNumber: </label><asp:TextBox ID="txtDocNumber" runat="server" CssClass="regText" MaxLength="20"/></td>
        <td><label class="regBldGreyText">Budget Activity: </label><asp:TextBox ID="txtBA" runat="server" CssClass="regText" MaxLength="20"/></td>
        <td><label class="regBldGreyText">Project Number: </label><asp:TextBox ID="txtProjNum" runat="server" CssClass="regText" MaxLength="20"/></td>
        <td><label class="regBldGreyText">Award Number: </label><asp:TextBox ID="txtAwardNum" runat="server" CssClass="regText" MaxLength="20"/></td>
        <td><label class="regBldGreyText">Latest Review</label><asp:CheckBox ID="chkLatestReview" runat="server" /></td>
      </tr>
      <tr><td colspan="5"><br /></td></tr>
      </table>  
      <table width="100%" border="0" cellspacing="0" cellpadding="0" id="tblSubmit" runat="server" >
      <tr>
        <td width="14%" class="lrgBldText">Select View: </td>
        <td width="16%" class="regBldText" >
            <asp:DropDownList ID="ddlViewFilter" runat="server" AutoPostBack="false" />
        </td>
        <td >
            <asp:Button ID="btnSubmit" CssClass="button" runat="server" Text="Submit" />&nbsp;
            <asp:Button ID="btnValHistory" CssClass="button" runat="server" Visible ="false"  Text="Get Review History" OnClick="btnValHistory_Click" /></td>
      </tr>
      <tr>
        <td colspan="3" >
            <br />
            <asp:Label ID="lblCriteriaMsg" runat ="server" />
            </td>
      </tr>
    </table>
    
    <script type="text/javascript" >
    
    function DispayReportButtons()
    {
    //debugger
    
        //var load_type_id = document.getElementById("lblLoadTypeID").innerHTML; //lblDueDate
        //var ddl = document.getElementById('ctrlCriteria_ddlLoad');
        var doc = document.getElementById("ctrlCriteria_txtLoadTypeID");
        //var load_type_id = doc.value;
        //alert('!');
        
        //load_type_id='6';
        
//        if(load_type_id=='6')
//        {
//            document.getElementById("btnTotalDaily").value = "BA53 Total Daily!";
//        }
//        else
//        {
//            document.getElementById("btnTotalDaily").value = "Total Daily!";
//        }  
    }
    
    </script>