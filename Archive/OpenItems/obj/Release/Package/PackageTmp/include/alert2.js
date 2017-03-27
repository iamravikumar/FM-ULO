function alert2(s,z)
{var d=document.createElement('DIV');d.className="AlertWindow";z=z==null?"Note":z
 s="<div class='AlertHeader'>"+z+"</div><div class='AlertBody'>"+s
 s+="<br/><input class='AlertButton' id='_b' onclick='alert2_(this)' onfocusout='alert2_(this)' type='button' value='Close'"
 s+=" onmouseover='this.className="+'"'+"AlertButtonHover"+'"'+"' onmouseout='this.className="+'"'+"AlertButton"+'"'+"'/></div>"
 d.innerHTML=s
 var b=document.body
 b.appendChild(d);
 if(document.compatMode!="BackCompat")
    b=b.parentNode
 d.style.top=(b.clientHeight-d.offsetHeight)/2+b.scrollTop
 d.style.left=(b.clientWidth-d.offsetWidth)/2+b.scrollLeft
 document.getElementById('_b').focus()
 document.forms[0].disabled=true
}
function alert2_(t)
{document.body.removeChild(t.parentNode.parentNode)
 document.forms[0].disabled=false
}
var open_items_app=true
var bln_close=false
function _ST(t)
{if(opener&&opener.open_items_app)
	bln_close=true
 setTimeout(bln_close?"window.close();":"window.location='/OpenItems/Default.aspx?_ST=1';",t)
}
