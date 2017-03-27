<%@ Control Language="C#" AutoEventWireup="true" Inherits="GSA.OpenItems.Web.Controls_Menu" Codebehind="Menu.ascx.cs" %>


<div id="navigation">
<!-- NAVIGATION -->
    <div id="usual1" class="usual">
        <ul>
          <li>
            <asp:HyperLink  Visible="false" ID="hlTabApp1" runat="server"  Text="ULO - Open Items" />
          </li>
          <li>
            <asp:HyperLink Visible="false" ID="hlTabApp2" runat="server"  Text="BA61 Fund Status" />
          </li>
        </ul>
        <div id="div_app1" runat="server">
            <asp:HyperLink runat="server" NavigateUrl="~/OpenItems/ReviewOpenItems.aspx" Text="OI Review" ToolTip="Review Open Items List" />
            <span class="dividers"> | </span> 
    
            <asp:HyperLink runat="server" NavigateUrl="~/OpenItems/Search.aspx" Text="Search" ToolTip="Search Open Items" />
            <span class="dividers"> | </span> 
    
            <asp:HyperLink ID="hlUploadData" runat="server" Visible="false" NavigateUrl="~/UploadData.aspx" Text="Upload" ToolTip="Upload New Data from Central Office or Other Region" />
            <span class="dividers" ID="sUploadData" runat="server" Visible="false"> | </span>

            <div style="display: none">
            <asp:HyperLink ID="hlAdmin" runat="server" Visible="false" NavigateUrl="~/AdminPage.aspx" Text="Admin" ToolTip="Admin functions" />
            <span class="dividers" ID="sAdmin" runat="server" Visible="false"> | </span> 
            </div>

            <asp:HyperLink ID="hlAssignment" runat="server" visible="false" NavigateUrl="~/OpenItems/Assignments.aspx" Text="Disputed Assignments" ToolTip="Confirm Reassignments Requests" />            
            <span class="dividers" ID="sAssignment" runat="server" Visible="false"> | </span> 
    
            <asp:HyperLink ID="hlDeobligation" runat="server" Visible="false" NavigateUrl="~/OpenItems/Deobligation.aspx" Text="Confirm Deobligation" ToolTip="Confirm Deobligation Invalid Open Items" />
            <span class="dividers" ID="sDeobligation" runat="server" Visible="false"> | </span>

            <asp:HyperLink runat="server" NavigateUrl="~/OpenItems/DocAdministration.aspx" Text="Documents" ToolTip="Attached Documents Administration" />
            <span class="dividers"> | </span>

            <asp:HyperLink ID="hlReports" runat="server" Visible="false" NavigateUrl="~/OpenItems/Reports.aspx" Text="Reports" ToolTip="Reports" />
            <span class="dividers" ID="sReports" runat="server" Visible="false"> | </span>
            
            <asp:HyperLink ID="nlPassword" runat="server" Visible="false" NavigateUrl="~/Users.aspx?mode=pswd" Text="Change Password" ToolTip="Change your password" /><span
                style="color: #ff3366"> </span>
            <span class="dividers" ID="sPassword" runat="server" Visible="false"> | </span>

            <label title="User Manual" class="label_link" onclick="javascript:window.open('http://p1130s-dotnet01/OpenItems/docs/UserManual.pdf');" >Help</label>       
            <span class="dividers"> | </span>

            <asp:HyperLink runat="server" NavigateUrl="~/Logoff.aspx" Text="Exit" ToolTip="Log Out / Exit" /> 
            &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; 
             <span style="text-align:right">                
                <asp:Label ID="lblUserFullName" CssClass="Indentity" runat="server" Text="User"></asp:Label>    
            </span>              
        </div>
        <div id="div_app2" runat="server"  >
            <asp:HyperLink runat="server" NavigateUrl="~/FundStatus/FundsReview.aspx" Text="Funds Review" ToolTip="Obligations/Income Review" />
            <span class="dividers"> | </span>
            <asp:HyperLink ID="hlFundsSearch" runat="server" Visible="true" NavigateUrl="~/FundStatus/FundSearch.aspx" Text="Funds Search" ToolTip ="Document Fund Status Search" />
            <span class="dividers" ID="sFundsSearch" runat="server" Visible="true"> | </span>
            <asp:HyperLink ID="hlFundsStatus" runat="server" Visible="false" NavigateUrl ="~/FundStatus/FundsReport.aspx" Text ="Fund Status" ToolTip ="Fund Status Report" />
            <span class="dividers" ID="sFundsStatus" runat="server" Visible="false"> | </span>
            <asp:HyperLink ID="hlSummaryReport" runat="server" Visible="false" NavigateUrl="~/FundStatus/SummaryReport.aspx" Text="Summary Report" ToolTip="Summary Projection Report" />
            <span class="dividers" ID="sSummaryReport" runat="server" Visible="false"> | </span>
            <asp:HyperLink ID="hlAllowance" runat="server" Visible="false" NavigateUrl="~/FundStatus/Allowance.aspx" Text="NCR Allowance" ToolTip="Allowance Over NCR" />
            <span class="dividers" ID="sAllowance" runat="server" Visible="false"> | </span>
            <asp:HyperLink ID="hlAllowanceOrg" runat="server" Visible="false" NavigateUrl="~/FundStatus/OrgAllowance.aspx" Text="Allowance Distribution" ToolTip="Allowance Distribution Over Organizations/Functions" />
            <span class="dividers" ID="sAllowanceOrg" runat="server" Visible="false"> | </span> 
            <asp:HyperLink runat="server" NavigateUrl="~/FundStatus/FSReferDocs.aspx" Text="References" ToolTip="Fund Status Reference documents" />
            <span class="dividers"> | </span>                     
            <label title="User Manual" class="label_link" onclick="javascript:window.open('../docs/UserManual.pdf');" >Help</label>       
            <span class="dividers"> | </span>
            <asp:HyperLink runat="server" NavigateUrl="~/Logoff.aspx" Text="Exit" ToolTip="Log Out / Exit" />  
            <span class="dividers"> |</span> 
                 
        </div>                       
    </div>
<!-- END of NAVIGATION -->         
</div>

<script type="text/javascript" >


</script>


