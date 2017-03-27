<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/ULO/ULONoMenu.master" AutoEventWireup="true" Inherits="GSA.OpenItems.Web.Logon" Codebehind="Logon.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="title" runat="Server">
    Open Items - Logon
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="head" runat="Server">
    <!--#include virtual="~/include/Logon.js" -->
    <!--TODO: don't use jscript-->
    <script language="javascript" type="text/jscript">

        //function CheckBrowser() {
        //    try {
        //        //document.getElementById("Text1").style.display = "none";

        //        var ua = window.navigator.userAgent;
        //        var msie = ua.indexOf('MSIE');
        //        var chr = ua.indexOf('Chrome');
        //        var ff = ua.indexOf('Firefox');

        //        if (msie > 0)      // If Internet Explorer
        //        {
        //            //debugger
        //            var d = document.getElementById("Text1");
        //            if (d != null) {
        //                d.style.display = "none";
        //            }
        //            else {
        //                alert("Object is null");
        //            }
        //            //document.getElementById("smm").style.display = "";
        //            //alert("IE");

        //        }
        //        else if (chr > 0) {
        //            //d.innerHTML = "Invalid browser error";
        //            //alert("INVALID BROWSER!\n\nThe 'ULO' application is compatible only with the Internet Explorer only.\nDo not use Chrome or another non-IE based browsers.\nPlease copy and paste the link for the homepage into IE and open the page in IE");
        //            //myAlert("Invalid Browser Alert", "The 'ULO' application is compatible only with the Internet Explorer  only.\nDo not use Chrome or another non-IE based browsers.\nPlease copy and paste the link for the homepage into IE and open the page in IE");
        //        }
        //        else if (ff > 0) {
        //            //d.innerHTML = "Invalid browser error";
        //            //alert("INVALID BROWSER!\n\nThe 'ULO' application is compatible only with the Internet Explorer  only.\nDo not use Firefox or another non-IE based browsers.\nPlease copy and paste the link for the homepage into IE and open the page in IE");
        //            //myAlert("Invalid Browser Alert", "The 'ULO' application is compatible only with the Internet Explorer  only.<br>Do not use Firefox or another non-IE based browsers.<br>Please copy and paste the link for the homepage into IE and open the page in IE");
        //        }
        //        else // If another browser, return 0
        //        {
        //            //d.innerHTML = "Invalid browser error";
        //            //alert("INVALID BROWSER!\n\nThe 'ULO' application is compatible only with the Internet Explorer  only.\nDo not any non-IE based browsers.\nPlease copy and paste the link for the homepage into IE and open the page in IE");
        //            //myAlert("Invalid Browser Alert", "The 'ULO' application is compatible only with the Internet Explorer  only.\nDo not use any non-IE based browsers.\nPlease copy and paste the link for the homepage into IE and open the page in IE");
        //        }

        //    }
        //    catch (err) {
        //        alert(err.description);
        //    }
        //}

        function myAlert(mytitle, mytext) {
            nw = false;
            html = "<html><head><title>" + mytitle + "</title></head>"
            + "<body bgcolor=thistle>" + mytext + "</body></html>";
            args = "width=600,height=100,menubar=no,status=no,resizable=no,scrollbars=no,top=300,left=500,location=0,fullscreen=no"
            nw = window.open('', "myAlert", args)
            nw.document.write(html)
            //alert("This is standard alert box:\n"+ html)
        }
    </script>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="contentTitle" runat="Server">
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="content" runat="Server">
    <div class="section" style="text-align:center">

    <table   width="100%" border="0" cellspacing="8" cellpadding="0" style="text-align:center">
        <tr id="tr_disabled" runat="server" style="display:none;">
            <td colspan="3" align="center">
                <asp:Label ID="lblDisabledMessage"  runat="server" Width="100%" Font-Bold="false" ForeColor="Red"></asp:Label>
            </td>       
        </tr>
        <tr>
            <td colspan="4" align="center" >
                <span style="color:Red;">
                <b>
                    <asp:Label ID="lblEnvironment" runat="server" Visible="false" >        
                    YOU ARE CONNECTED TO THE TEST DATABASE!
                    <br />
                    THIS IS NOT PRODUCTION ENVIRONMENT
                    </asp:Label>
                </b>
                </span>
            </td>
        </tr>

        <tr>
             <td  valign="top" align="left" colspan="4" id="td_warning" runat="server">
                       <font face="Verdana" font size="1"><font color="red">***WARNING***</font><br> <font color="blue">
	                This is an official U.S. Government System for authorized use only.
	                To protect this system from unauthorized use,activities on this system are monitored and                 recorded and subject to audit.
	                Use of this system is expressed consent to such monitoring and recording.
	                Any unauthorized access or use of this system is prohibited and could be subject
	                to criminal and civil penalties and/or administrative action.
	                </font><br/><br/>
	                </font>
              </td>
         
        </tr>
        <span id = "tr_login" runat="server">
        <tr style="height:30px">
            <td  colspan="4" >
                 <font face="Verdana" size="2" color="#1b5fb2">
                    To login use your ENT user name and password 
                    <span style="VERTICAL-ALIGN:middle" onclick="checkCapsLock(event, '2')">
                        <a class="infoMessage" style="CURSOR:hand"><img alt="Info" height="15" width="15" src="Images/info_small_20.jpg"/></a>
					</span>
				</font>
            </td>
        </tr>

        <tr>
            <td style="width: 45%" align="right" >
                <label class="regBldBlueText">ENT User Name:</label>
            </td>
            <td style="width: 20%">
                <asp:TextBox ID="txtUsername" runat="server" CssClass="textfield" Width="100%" MaxLength="50"/>
            </td>
            <td />
        </tr>
        <tr id="tr_pswd" runat="server">
            <td  align="right" >
                <label class="regBldBlueText">ENT Password:</label>
            </td>
            <td >
                <asp:TextBox ID="txtPassword" onKeyPress="checkCapsLock(event,'1')" runat="server" CssClass="textfield" TextMode="Password" Width="100%" MaxLength="50"/>
            </td>
            <td>
                <asp:Label ID="lblBrowserTypeAlert"  Visible="true" runat="server" ></asp:Label>
            </td>
        </tr>        
        <tr>
            <td colspan="3" style="height:10px;">
            </td>
        </tr>
       
       
        <tr>
            <td id="td_forgot_pswd" runat="server" align="right" colspan="2"  >                    
                <asp:Button ID="btnSubmit" runat="server" CssClass="button" Text="Login" />
                <input type="button" id="btnEmail" class="button" value="Send Email" onclick="send_email()" style="display:none;" />
            </td>
            <td>
            </td>
            
        </tr>
        <tr>
            <td colspan="2" style="text-align:right" title="click to reset your ENT password">
             <a href="https://reset.gsa.gov/"   target="_blank" id="forgot-pwd" class="regBldBlueText" style="color:Navy;"> Forgot your ENT password?</a>
               
            </td>
            
            <td colspan="2" >
            </td>
        </tr>
        </span>
         <tr>
            <td colspan="4" align="left" >
                <!-- #include virtual="include/ErrorLabel.inc" -->                
            </td>            
        </tr>
        
        <tr>
            <td colspan="3" align="center" style="display:none;">
                <label id="lblPswd" class="regBldBlueText" style="display:none;" >Please enter your username and click button 'Send email'. <br />Your password will be sent to your GSA email account.</label>
            </td>
        </tr>
    </table>

</div> 
     <input id="txtDBServerName"   type="text" runat="server" style="color: white; width:100%; border:none 1px white; text-align:center" value="server name" />
</asp:Content>

