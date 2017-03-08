
function _e(o){return document.getElementById(o)}
function _son1(o){o=o.firstChild;return o.nodeType==1?o:o.nextSibling}
function _up(o,u){while(u--) o=o.parentNode;return o}
function mouse(b)
{	var s=window.event.srcElement
	while(s!=null&&s.tagName!="TR") s=s.parentNode;
	if(s==null) return
	if(s.className=="tRow"||s.className=="tRowAlt")
		s.style.backgroundColor=b
}
function formatMoney(m)
{	var s=String(Math.round(m))    
	var oRegExp = new RegExp('(-?[0-9]+)([0-9]{3})')
	while(oRegExp.test(s))
		s=s.replace(oRegExp,'$1,$2');	
	return (s.indexOf('-')>-1?s.replace('-','($')+')':'$'+s)	
}
function blockNonNumbers(o,e,aD,aN)
{	var p=o.value
	var key=e.keyCode
	if(isNaN(key)||e.ctrlKey)
		return true
	key=String.fromCharCode(key)
	var reg=/\d/
	return (aN?key=='-'&&p.indexOf('-')==-1:false)||(aD?key=='.'&&p.indexOf('.')==-1:false)||reg.test(key)
}
function extractNumber(o,dp,aN,Max)
{
var p=o.value
 var sR0='[0-9]*'
 if(dp>0)
 	sR0+='\\.?[0-9]{0,'+dp+'}';
 else if(dp<0)
	sR0+='\\.?[0-9]*';
 sR0=aN?'^-?'+sR0:'^'+sR0
 sR0=sR0+'$'
 var R0=new RegExp(sR0)
 if(!R0.test(p))
 {	var sR1='[^0-9'+(dp!=0?'.':'')+(aN?'-':'')+']'
	var R1=new RegExp(sR1,'g')
	p=p.replace(R1,'')
	if(aN)
	{	var hasNegative=p.length>0&&p.charAt(0)=='-'
		var R2=/-/g
		p=p.replace(R2,'')
		if(hasNegative)
			p='-'+p
	}
	if(dp!=0)
	{	var R3=/\./g;
		var R3Array=R3.exec(p);
		if(R3Array!=null)
		{	var R3r=p.substring(R3Array.index+R3Array[0].length)
			R3r=R3r.replace(R3,'')
			R3r=dp>0?R3r.substring(0,dp):R3r
			p=p.substring(0,R3Array.index)+'.'+ R3r
		}
	}
 }
 if(o.value!=p)
	o.value=p
 if(o.value!=o.alt)
 {	o.alt=o.value
	recalculate(o)
 }
}
function recalculate(o)
{	
//debugger
    var r = _up(o,2);
    var inc = r.cells[1].innerText.replace('($','-').replace(')','').replace('$','').replace(',','');
    var inc_v = parseFloat(inc);
    var d = parseInt(_e("lblD").value);
    var dd = parseInt(_e("lblDD").value);
    var p = parseFloat(o.value);
    r.cells[3].innerText = formatMoney((p*d/dd)-inc_v);    
}
function send_rezult()
{	var rr=_e("tblData").rows
	var r=""
	for (var i = 1; i < rr.length; i++)
	{	var j=_son1(rr[i].cells[2]).value
	    var g=rr[i].cells[1].alt;
	    r+=g==""?"0|":g+"|"
		r+=j==""?"0;":parseFloat(j)+";"
	}
	window.opener.reload_rwa(r);
	self.close()
}
function Page_OnLoad()
{	
	var rr=_e('tblData').rows
	var cnt=rr.length;
	for(var i=1; i<cnt; i++)
	{	var t=_son1(rr[i].cells[2])
		if(t.value=="")t.value=0;
		t.alt=t.value
		recalculate(t)
	}
}

