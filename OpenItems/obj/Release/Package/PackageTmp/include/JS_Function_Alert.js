
function checkCapsLock(e, i, mes) 
    {
   //debugger
        var myKeyCode=0;
        var myShiftKey=false;
        var myMsg = "";
        if(i=="1")
        {
            myMsg='To prevent entering your password incorrectly,<br/>you should press <span style="color: red;">Caps Lock</span> to turn it off.';
            ALERT_TITLE = "CAPS LOCK IS ON!";
           
            // Internet Explorer 4+
            if ( document.all ) 
            {
                myKeyCode=e.keyCode;
                myShiftKey=e.shiftKey;

            // Netscape 4
            } 
            else if ( document.layers ) 
            {
                myKeyCode=e.which;
                myShiftKey=( myKeyCode == 16 ) ? true : false;

            // Netscape 6
            } 
            else if ( document.getElementById ) 
            {
                myKeyCode=e.which;
                myShiftKey=( myKeyCode == 16 ) ? true : false;
            }

            // Upper case letters are seen without depressing the Shift key, therefore Caps Lock is on
            if ( ( myKeyCode >= 65 && myKeyCode <= 90 ) && !myShiftKey ) 
            {
                alert( myMsg );

            // Lower case letters are seen while depressing the Shift key, therefore Caps Lock is on
            } 
            else if ( ( myKeyCode >= 97 && myKeyCode <= 122 ) && myShiftKey ) 
            {
                alert( myMsg );
            }
            }
        else if(i=="2") //text-align:justify; //<span style="color: red;text-align:center">
        {
            myMsg="To log in to ULO application you need to use your<BR><span style='color: red;text-align:center'>ENT User Name and Password</span>.<br>Do NOT use any other names or passwords (i.e.,  your full GSA email address or the part of your GSA email address preceding the '@' sign) as they will prevent you from logging in to the application.";
            ALERT_TITLE = "Login Tips";
            alert( myMsg );
        }
        else if(i=="3")
        {
            myMsg=mes;
            ALERT_TITLE = "ULO Message Box";
            alert( myMsg );
        }
//        else if(i=="4")
//        {
//            myMsg=mes;            
//            ALERT_TITLE = "Please be patient!";
//            ALERT_BUTTON_TEXT = "Continue"
//            alert( myMsg );
//        }
        
    }
    
// constants to define the title of the alert and button text.
var ALERT_TITLE = "";
var ALERT_BUTTON_TEXT = "Close";

// over-ride the alert method only if this a newer browser.
// Older browser will see standard alerts
if(document.getElementById) 
{
	window.alert = function(txt) 
	{
		createCustomAlert(txt);
	}
}

function createCustomAlert(txt) 
{
	// shortcut reference to the document object
	d = document;

	// if the modalContainer object already exists in the DOM, bail out.
	if(d.getElementById("modalContainer")) return;

	// create the modalContainer div as a child of the BODY element
	mObj = d.getElementsByTagName("body")[0].appendChild(d.createElement("div"));
	mObj.id = "modalContainer";
	 // make sure its as tall as it needs to be to overlay all the content on the page
	mObj.style.height = document.documentElement.scrollHeight + "px";

	// create the DIV that will be the alert 
	alertObj = mObj.appendChild(d.createElement("div"));
	alertObj.id = "alertBox";
	// MSIE doesnt treat position:fixed correctly, so this compensates for positioning the alert
	if(d.all && !window.opera) alertObj.style.top = document.documentElement.scrollTop + "px";
	// center the alert box
	alertObj.style.left = (d.documentElement.scrollWidth - alertObj.offsetWidth)/2 + "px";
	alertObj.style.top = (d.documentElement.scrollHeight - alertObj.offsetHeight)/3 + "px";

	// create an H1 element as the title bar
	h1 = alertObj.appendChild(d.createElement("h1"));
	h1.appendChild(d.createTextNode(ALERT_TITLE));

	// create a paragraph element to contain the txt argument
	msg = alertObj.appendChild(d.createElement("p"));
	msg.innerHTML = txt;
	
	// create an anchor element to use as the confirmation button.
	btn = alertObj.appendChild(d.createElement("a"));
	btn.id = "closeBtn";
	btn.appendChild(d.createTextNode(ALERT_BUTTON_TEXT));
	btn.href = "#";
	// set up the onclick event to remove the alert when the anchor is clicked
	btn.onclick = function() { removeCustomAlert();return false; }
}

// removes the custom alert from the DOM
function removeCustomAlert() {
	document.getElementsByTagName("body")[0].removeChild(document.getElementById("modalContainer"));
}


