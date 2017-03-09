function xmlhttpCreate()
{ var oX=false
 /*@cc_on @*/
 /*@if(@_jscript_version>=5)
 try{oX=new ActiveXObject("Msxml2.XMLHTTP")}
 catch(e)
 {	try{oX=new ActiveXObject("Microsoft.XMLHTTP")}
	catch(E){oX=false}
 }
 @end @*/
 if(!oX&&typeof XMLHttpRequest!='undefined')
 {	try{oX=new XMLHttpRequest()}
	catch (e){oX=false}
 }
 if(!oX&&window.createRequest)
 {	try{oX=window.createRequest()}
	catch (e){oX=false}
 }
 return oX
}
function GetHttp(pg,prm)
{if(pg.length==0||prm.length==0)
	return null
 oX=xmlhttpCreate()
 oX.open("POST",pg,false)
 oX.setRequestHeader("Accept",prm)
 oX.send(null)
 if(oX.readyState==4)
	return oX.responseText
}

