function _FindCB()
{var pc=document.getElementsByTagName("input")
 for(var l=0;l<pc.length;l++)
 {	if(pc[l].id.indexOf("CB_Art")>=0)
		return pc[l]
 }
}
var MESS
var f_Flash=0
function SwitchCSS()
{if(MESS.innerHTML!="")
 {	MESS.className=MESS.className.indexOf("Alt")<0?MESS.className+"Alt":MESS.className.replace("Alt","")
	setTimeout("SwitchCSS()",500)
 }
 else
	f_Flash=0
}
function SetErrMess(x)
{MESS=_FindCB().nextSibling;MESS.innerHTML=x
 if(f_Flash==0&&x!=""&&MESS.className!="")
	{setTimeout("SwitchCSS()",500);f_Flash=1}
}
function GetErrMess(){return _FindCB().nextSibling.innerHTML}
function GetSessionVar(v){return GetHttp(_FindCB().value,v)}
function art_session()
{var q=_FindCB();var x=q.nextSibling.innerHTML
 if(!q.checked)
	q.checked=true
 else	//"IE back" clicked or history.back()
 {	x=GetHttp(q.value,"Error")
	if(x=="")//no error
	{	var o=window.location.href
		if(o.indexOf("GetParams")<0&&o.indexOf("Default")<0) //these pages never change
		{	window.location.href=o //re-read, data might been changed
			return true
		}
	}
 }
 SetErrMess(x)
 return false
}
window.onunload=function(){}
