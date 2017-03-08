<script language="javascript">
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
{var p=o.value
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
 while(Math.abs(p)>Math.abs(Max+1))
	p=parseInt(p/10);
 if(o.value!=p)
	o.value=p
}
</script>
