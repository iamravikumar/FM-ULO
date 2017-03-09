//--public methods
function SetDateMode(dt,f,ve,s)
{var q=_e(dt+"_lDate")
 if(s!=null)
	q.className=s
 q.readOnly=ve||f;q.style.cursor=!f&&ve?"pointer":"";q=q.parentNode
 _SetPointer(q,!f&&ve);_son1(_up(q,2)).style.display="none";_SetPointer(_e(dt+"_tdBttn"),!f)
}
function GetSelectedDate(dt){return CreateDate(_e(dt+"_lDate").value)}
function SetSelectedDate(dt,d)
{var q=_e(dt+"_lDate");var p7=_son1(_prv(_up(q,2)))
 if(d==null)
 {	var p=p7.rows[7].cells[5]
	if(p.innerHTML!="")
		{q.value="";_nxt(p7).value=""}
 }
 else
 {	var y=d.getFullYear();var m=d.getMonth()
	q.value=_sDateF(y,m,d.getDate())
	SelectRow(dt+"_ddgMonth",m)
	SelectGridItem(dt+"_ddgYear",y)
 }
 showCal(p7.parentNode)
}
function SetMaxDate(dt,d){SetMinMax(dt,d,null)}
function SetMinDate(dt,d){SetMinMax(dt,null,d)}
function isSelectedDateEmpty(dt)
{var q=_e(dt+"_lDate")
 return q==null?null:(q.value=="")
}
function GetSelectedIndex(ddg)
{var q=_e(ddg+"_dgIndex")
 if(q==null)
	return -1
 var j=parseInt(q.value)
 return j!=j?-1:j
}
function GetSelectedRow(ddg)
{var dg=_e(ddg+"_ArtDG");
 if(dg==null)
	return null
 var i=dg.parentNode.id==""?_dwn(_nxt(dg.parentNode),2).ch:parseInt(dg.summary)
 return i>=0?dg.rows[i].cells:null
}
function GetGridRow(ddg,j)
{var dg=_e(ddg+"_ArtDG")
 if(dg==null)
	return null
 if(j!=j||j<0||j>=dg.rows.length-1)
	return null
 var k=0;j=-j-1;while(dg.rows[++k].tabIndex!=j);
 return dg.rows[k].cells
}
function SelectGridItem(ddg,sID)
{var dg=_e(ddg+"_ArtDG")
 if(dg==null)
	return -1
 if(dg.rows.length<2)
	return -1
 var i=1;var ti=dg.rows.length-1
 while(dg.rows[i].cells[0].innerHTML!=sID&&i++<ti);
 if(i==dg.rows.length)
	return -1;
 ti=-dg.rows[i].tabIndex
 if(dg.parentNode.id=="")
	{dg.parentNode.style.display="";dClick(dg,i)}
 else
	{dgmClick(dg.rows[i]);ShowItem(dg.rows[i])}
 return ti-1
}
function SelectRow(ddg,ti)
{var dg=_e(ddg+"_ArtDG")
 if(dg==null)
	return;
 if(dg.rows.length<2||dg.rows.length<=++ti)
	return;
 var i=0;ti=-ti;while(dg.rows[++i].tabIndex!=ti);
 if(dg.parentNode.id=="")
	{dg.parentNode.style.display="";dClick(dg,i)}
 else
	{dgmClick(dg.rows[i]);ShowItem(dg.rows[i])}
}
function dgFilter2(q,c)
{var dg=_e(q+"_ArtDG")
 if(dg==null||c==null)
	return
 _prv(dg).onfocus=c;dg.lastChild.chOff=666;_Filter_(dg)
}
function dgFilter(ddg,sample,c,f)
{var dg=_e(ddg+"_ArtDG")
 if(dg==null||dg.lastChild.summary==sample)
	return;
 dg.lastChild.summary=sample;dg.lastChild.chOff=f==0?c:-c-1;_Filter_(dg)
}
function SetIC(dg,l,s)
{var s2=s+"Alt";var u=0
 for(var i=1;i<l;i++)
	if(dg.rows[i].style.display=="")
		dg.rows[i].className=u++%2?s2:s
}
function SetDdgMode(ddg,s,sT,sH,f)
{var ix=_e(ddg+"_dgIndex")
 if(ix==null||(ix.tabIndex&64)>0)
	return;
 ix.className=s;ix.tabIndex&=0x0F7
 if(sH==null)
	ix.tabIndex|=8;
 dg=_nxt(ix);dg.className=sT;var r=dg.rows;r[0].className=sH!=null?sH:"GridHeader";var l=r.length
 if(dg.parentNode.id=="")
 {	ix.tabIndex&=0x0FD
	if(f) ix.tabIndex|=2
	SetIC(dg,l,s);
	var q=_nxt(_up(dg,2));var p=_up(dg,5);p.className=dg.parentNode.className;p.style.height="";q.className=r[0].className
	q.style.display=(ix.tabIndex&2)==0||l<3||ix.className.substr(0,3)!="Art"?"none":""
	q=_son1(_nxt(dg.parentNode));q.rows[0].className=r[0].className;q.className=sT
	q.rows[0].style.display=r[0].style.display=(ix.tabIndex&8)>0?"none":""
	q.title=l<3||ix.className.substr(0,3)!="Art"?"":"Click to expand"
	if(ix.value=="") return
	q.rows[1].className=r[_son1(q).ch].className=s+"Selected"
	if(IE){toClick(dg);toClick(dg)}
 }
 else
 {	if((ix.tabIndex&4)!=0&&l>1)
	{	if(r[--l].className=="")
			r[l].className=r[0].className;
	}
	SetIC(dg,l,s);r[0].style.display=(ix.tabIndex&8)>0?"none":""
	if(ix.value!="")
	{	var ss=dg.summary.split(";");var i=(ix.tabIndex&2)!=0?ss.length-2:0;
		do{r[parseInt(ss[i])].className=s+"Selected"
		}while(i--)
	}
	RefreshDG()
 }
}
var _mg=null
function RefreshDG()
{if(_mg)
 {	var q=_mg
	if(QM)
		{while(q.offsetParent.tagName.toUpperCase()!="BODY"){q=q.offsetParent}}
 	else
		{while(q.parentNode.tagName.toUpperCase()!="HTML"){q=q.parentNode}}
	var h=0;var p=_mg.parentNode
	if(IE) h=QM?document.body.clientHeight:document.documentElement.clientHeight;
	else{_mg.lastChild.style.height="";h=window.innerHeight}
	h+=p.clientHeight-q.offsetHeight-q.offsetTop-_marginBottom;
	q=_mg.offsetHeight
	if(IE&&QM){var d=p.offsetHeight-p.clientHeight;h+=d;q+=d}
	q=q<h?q:h>80?h:q<80?q:80
	if(q>=0)
	{	_nxt(p).style.height=p.style.height=q+"px";_setHY(_mg)
		q=parseInt(_mg.summary)
		if(q>0) ShowItem(_mg.rows[q])
	}
 }
 var p=document.getElementsByTagName("TABLE")
 for(var i=0;i<p.length;i++)
 {	var dg=p[i]
	if(dg.id.substr(dg.id.length-5,5)=="ArtDG")
	{	var q=dg.parentNode
		if(q.id=="")
		{   var z=_dwn(_nxt(q),3).style;z.position="";z.position="relative"
		    if(q.style.display=="") CalcH(dg)
		}
		else
		{	_setHY(dg)
			if(IE&&!QM) q.style.width=_nxt(q).clientWidth+"px"
		}
	}
 }
}
var _marginBottom=5
//-- private
function _e(o){return document.getElementById(o)}
function _s(e){return e.target!=null?e.target:e.srcElement}
function _up(o,u){while(u--) o=o.parentNode;return o}
function _dwn(o,u){while(u--) o=_son1(o);return o}
function _nxt(o){o=o.nextSibling;return o==null||o.nodeType==1?o:o.nextSibling}
function _prv(o){o=o.previousSibling;return o==null||o.nodeType==1?o:o.previousSibling}
function _son1(o){o=o.firstChild;return o.nodeType==1?o:o.nextSibling}
function _son2(o){return _nxt(_son1(o))}
function _son3(o){return _nxt(_nxt(_son1(o)))}
function _k(e){return e.keyCode?e.keyCode:e.charCode}
function _td(e)
{var t=e.target!=null?e.target:e.srcElement
 while(t!=null&&t.tagName!=null&&t.tagName.toUpperCase()!="TD"){t=t.parentNode}
 return t!=null&&t.tagName!=null&&t.tagName.toUpperCase()=="TD"&&_up(t,3).id.indexOf("ArtDG")>0?t:null
}
function _tr(e)
{var t=e.target!=null?e.target:e.srcElement
 while(t!=null&&t.tagName!=null&&t.tagName.toUpperCase()!="TR"){t=t.parentNode}
 return t!=null&&t.tagName!=null&&t.tagName.toUpperCase()=="TR"&&_up(t,2).id.indexOf("ArtDG")>0?t:null
}
function _SetPointer(q,f){q.style.cursor=f?"pointer":"";q.title=f?"Click to change date":""}
function _setHY(dg)
{var p=dg.parentNode;var h=_prv(dg).style.height;var b=dg.lastChild;var s=b.scrollTop;b.style.height=""
 if(h!="")
	p.style.height=h
 h=dg.offsetHeight
 if(p.clientHeight>h)
 {	p.style.height=h+"px"
	if(p.clientHeight!=h&&h+p.offsetHeight-p.clientHeight>0)
		p.style.height=h+p.offsetHeight-p.clientHeight+"px"
 }
 if(p.id!=""&&IE&&!QM)
	_nxt(p).style.height=p.style.height
 if(!IE)
 {	h=p.clientHeight-dg.rows[0].offsetHeight;
	if(b.clientHeight>h)
		{b.style.height=h+"px";b.scrollTop=s}
 }
}
function CalcH(dg)
{var q=dg.parentNode;var p=q.style;p.height="";dg.lastChild.style.height=""
 var y=_prv(dg).style.height;var d=q.offsetHeight-q.clientHeight
 if(y=="")
 {	var o=q;y=0;var s=QM?"BODY":"HTML";
	while(q.tagName.toUpperCase()!=s)
	{	if(o==q){y+=o.offsetTop;o=q.offsetParent}
		q=q.parentNode
	}
	q=q.clientHeight+q.scrollTop-y;q=IE&&QM?q:q-d
	y=dg.parentNode.offsetHeight;y=IE&&QM?y:y-d
	y=y<q?y:q>80?q:y<80?y:80
 }
 q=dg.offsetHeight;q=IE&&QM?q+d:q;y=parseInt(y);y=y>q?q+"px":y+"px";
 q=_up(dg,5).offsetWidth;q=IE&&QM?q:q-d;
 p.width=q+"px";p.height=y;_setHY(dg)
}
function _UnFoc(q){q.className=q.className.replace("Focus","")}
function _dgFoc(q){q=q.parentNode.id!=""?q.parentNode:_up(q,5);_altCSS(q,"Focus")}
function _findIdx(p)
{if(p.id.substr(p.id.length-5,5)!="ArtDG"||p.rows.length<2||p.rows[1].className.substr(0,3)!="Art")
	return -1
 var i=p.lastChild.ch
 if(i>0)
	return i
 i=parseInt(_prv(p).value)
 return i!=i||i<0?(p.rows[1].style.display!=""?StepDwn(1,p):1):(p.parentNode.id==""?_dwn(_nxt(p.parentNode),2).ch:parseInt(p.summary))
}
function rowOut(p)
{var s=p.className
 if(s!=null&&s.substr(s.length-4,4)=="Over"){p.className=s.replace("Over","");_up(p,2).lastChild.ch=0}
}
function _clearCh(p)
{var i=p.lastChild.ch
 if(i>0) rowOut(p.rows[i])
}
function rowIn(p)
{var s=p.className;
 if(s==null||s.substr(0,3)!="Art") return
 var q=_up(p,2);_clearCh(q);q.lastChild.ch=p.rowIndex
 if(s.substr(s.length-8,8)!="Selected"&&s.substr(s.length-4,4)!="Over")
	p.className=s+"Over"
}
function row_Out(e)
{var q=_tr(e)
 if(q!=null)
	rowOut(q)
}
function row_In(e)
{var q=_tr(e)
 if(q!=null)
	rowIn(q)
}
function __tr(e)
{var q=_tr(e)
 if(q==null) return false
 return q.className.substr(0,3)=='Art'
}
function _g_D(q)
{if(q.tagName.toUpperCase()=="TABLE") q=_dwn(q,4)
 return _son2(q)
}
function C508_(e,q)
{if(_k(e)!=9) return
 q=_g_D(q);q.lastChild.ch=0;q.style.zIndex=1;_dgFoc(q)
 var i=_findIdx(q)
 if(i<=0) return
 q.lastChild.ch=i
 if(q.parentNode.id!=""){rowIn(q.rows[i]);ShowItem(q.rows[i])}
}
function StepUp(j,q)
{while(--j>0&&q.rows[j].style.display!="");
 return j<1?-1:j
}
function StepDwn(j,q)
{while(++j<q.rows.length&&q.rows[j].style.display!="");
 return j>=q.rows.length?-1:j
}
function C508(e,q)
{var k=_k(e);q=_g_D(q)
 if(!IE&&k==9)
 {	if(q.parentNode.id=="") _ddg_out(q)
	else _mdg_out(q)
 }
 if(k<27||k>40) return true
 if(q.parentNode.style.display!="")
 {	if(k==32){toClick(q);return false}
	return true
 }
 var i=_findIdx(q)
 if(i<0) return true
 var j=i;var l
 var p=parseInt((q.parentNode.clientHeight-q.parentNode.clientTop-q.rows[1].offsetTop)/(q.rows[i].offsetHeight+parseInt(q.cellSpacing)))
 if(p>2) p--
 switch(k)
 {case 27://esc
	if(q.parentNode.id==""){toClick(q);_dgFoc(q)}
	break
  case 38://up
	j=StepUp(j,q)
	break
  case 40://dwn:
	j=StepDwn(j,q)
	break
  case 34://PgDwn
	while(p-->0&&j>0){l=j;j=StepDwn(l,q)}
	if(j<0) j=l
	break
  case 33://PgUp
	while(p-->0&&j>0){l=j;j=StepUp(l,q)}
	if(j<0) j=l
	break
  case 36://Home
	j=1
	if(q.rows[1].style.display!="") j=StepDwn(1,q)
	break
  case 35://End
	j=q.rows.length-1
	if(q.rows[j].style.display!="") j=StepUp(j,q)
	break
  case 32://Spc
	if(q.parentNode.id==""){q.parentNode.style.display="";dClick(q,i);_dgFoc(q)}
	else{if(q.rows[i].className.substr(0,3)=="Art"){dgmClick(q.rows[i],e);ShowItem(q.rows[i])}}
 }
 if(j>0&&j!=i)
 {rowOut(q.rows[i]);q.lastChild.ch=j;rowIn(q.rows[j]);ShowItem(q.rows[j])}
 if(IE) e.keyCode=0
 else e.stopPropagation()
 return false
}
function SetBody(q,y)
{var o=q
 while(q.tagName.toUpperCase()!="BODY")
 {	if(o==q){y+=q.offsetTop;o=q.offsetParent}
	q=q.parentNode
 }
 y-=q.clientHeight
 if(q.scrollTop<y)
	q.scrollTop=y
}
function ShowItem(q)
{var d=IE?_up(q,3):_up(q,1);
 var y=q.offsetHeight*3/2+q.offsetTop-d.clientHeight+d.clientTop
 if(!IE) y-=_up(q,2).rows[0].offsetHeight
 if(d.scrollTop<y) d.scrollTop=y
 y=q.offsetTop-_up(q,2).rows[0].offsetHeight-q.offsetHeight/2
 if(d.scrollTop>y) d.scrollTop=y
 SetBody(d,q.offsetHeight+q.offsetTop-d.scrollTop)
}
function _DateToS(d){return d==null?"":d.getFullYear()+"/"+(d.getMonth()+1)+"/"+d.getDate()}
function SetMinMax(dt,d1,d0)
{var q=_e(dt+"_day")
 var y=_son2(_dwn(_son1(_prv(q)).rows[0].cells[1],5))
 if(d1==null){s1=q.title.split(";")[1];s0=_DateToS(d0)}
 else{s0=q.title.split(";")[0];s1=_DateToS(d1)}
 if(s0=="")	s0=_son1(y.rows[y.rows.length-1]).innerHTML+"/1/1"
 if(s1==""||s1==null) s1=_son1(y.rows[1]).innerHTML+"/12/31"
 q.title=s0+";"+s1
 showCal(q.parentNode)
}
function _SetClass(b,p,s,u)
{if(p.className.substr(p.className.length-8,8)=="Selected")
 {	var q=_up(p,2)
	if(q.parentNode.id!="") q.summary+=p.rowIndex+"="+(u%2)+";"
	else _dwn(_nxt(q.parentNode),2).chOff=u%2?"Alt":""
	return ++u
 }
 if(b)
 {	p.style.display="";p.className=u%2?s+"Alt":s
	return ++u
 }
 p.style.display="none";return u
}
function HideFiltered(dg)
{var l=_prv(dg);var sB=l.className;var c=dg.lastChild.chOff;var rr=dg.rows;var u=0;l=rr.length
 if(dg.parentNode.id!="") dg.summary=""
 if((_prv(dg).tabIndex&4)!=0&&dg.parentNode.id!="") l--
 if(c!=666)
 {	var s=dg.lastChild.summary;var j=s.length
	if(c>=0)
		for(var i=1;i<l;i++)
			u=_SetClass(j==0||s==rr[i].cells[c].innerHTML.substr(0,j),rr[i],sB,u)
	else
	{	c=-c-1
		for(var i=1;i<l;i++)
			u=_SetClass(j==0||rr[i].cells[c].innerHTML.indexOf(s)>=0,rr[i],sB,u)
	}
 }
 else
 {	var s=_prv(dg).onfocus
	for(var i=1;i<l;i++)
		u=_SetClass(s(rr[i].cells),rr[i],sB,u)
 }
 _setHY(dg)
}
function _Filter_(dg)
{if(dg.parentNode.id=="")
 {	if(dg.rows.length<3) return
	HideFiltered(dg);var i=_dwn(_nxt(dg.parentNode),2).ch
	if(dg.parentNode.style.display!="") dClick(dg,i)
	else{RefreshDG();ShowItem(dg.rows[i])}
 }
 else
 {	HideFiltered(dg);RefreshDG();var i=parseInt(dg.summary)
	if(i>=0) ShowItem(dg.rows[i])
 }
}
function dClick(dg,k)
{if(_prv(dg).className.substr(0,3)!="Art"||dg.rows.length<3) return
 var r=dg.rows[k];var p=_up(dg,5)
 if(dg.parentNode.style.display=="")
 { 	p.className=dg.parentNode.className;p.style.height="" 
	var q=_dwn(_nxt(dg.parentNode),2);dg.rows[q.ch].className=_prv(dg).className+q.chOff
	q.ch=k;q.chOff=r.className.indexOf("Alt")>0?"Alt":""
	r.className=_prv(dg).className+"Selected";
	dg.parentNode.style.display="none"
	p=(-r.tabIndex-1)+";"+r.cells[0].innerHTML
	if(_prv(dg).value!=p)
	{	_prv(dg).value=p;p=dg.rows[0].cells.length-1
		do{q.rows[1].cells[p].innerHTML=r.cells[p].innerHTML}while(p--)
		q=_son3(dg.parentNode)
		if(q.onclick!=null)
			q.click()
	}
    if(IE&QM)
	{	p=document.getElementsByTagName("TABLE")
		for(var i=0;i<p.length;i++)
		{	q=p[i]
			if(q.id.substr(q.id.length-5,5)=="ArtDG")
			{	q=q.parentNode
				if(q.id==""){q=_dwn(_nxt(q),3).style;q.position="";q.position="relative"}
			}
		}
	}
 }
 else
 {	p.style.height=p.offsetHeight+"px";p.className=dg.parentNode.style.display="";
	CalcH(dg)
	if(IE&&!QM)
		CalcH(dg)
	ShowItem(r)
 }
}
var _S
function _a_dg()
{var q=_up(_S,3)
 return q.id.substr(q.id.length-5,5)=="ArtDG"?q:_son2(_prv(q.parentNode))
}
function Sorting()
{var dg=_a_dg();var tl=" .?.?;";var q=dg.rows;var k=_S.cellIndex-1;_clearCh(dg)
 while(q[0].cells[++k].innerHTML!=_S.innerHTML);
 var l=q.length-1; var ft=(_prv(dg).tabIndex&4)!=0&&dg.parentNode.id!=""
 if(ft)
	l--
 var a=new Array(l);var i=l
 if(_S.innerHTML.toUpperCase().indexOf("MONTH")>=0)
 {	var m="OctoberNovemberDecemberJanuaryFebruaryMarchAprilMayJuneJulyAugustSeptember"
	do{a[i-1]=m.indexOf(q[i].cells[k].innerHTML)+500000000000+tl+(i-1)}while(--i)
 }
 else if(_S.innerHTML.indexOf("$")>=0)
 {	do{a[i-1]=parseFloat(q[i].cells[k].innerHTML.replace(",","").replace(",","").replace(",",""))+500000000000+tl+(i-1)}while(--i)
 }
 else
 switch(_S.tabIndex)
 {case -3://number
	do{a[i-1]=parseFloat(q[i].cells[k].innerHTML)+500000000000+tl+(i-1)}while(--i)
	break
  case -2://date
	do{ var d=CreateDate(q[i].cells[k].innerHTML)
		a[i-1]=d?d.getFullYear()+"/"+(d.getMonth()+10)+"/"+(d.getDate()+10)+tl+(i-1):tl+(i-1)
	}while(--i)
	break
  case -1:
  default:
	do{a[i-1]=q[i].cells[k].innerHTML+tl+(i-1)}while(--i)
 }
 a.sort()
 var tp=_S.title=="Ascending sort";var t=new Array(l+1);i=l
 do{t[i]=dg.lastChild.removeChild(dg.rows[i])}while(--i)
 var ss=new Array();var s=_prv(dg).className;var sm=s+"Selected";i=l-1
 do{var n=tp?l-1-i:i;var j=parseInt(a[n].substr(a[n].indexOf(tl)+6))+1
	if(t[j].className==sm) ss.push(l-i)
	else t[j].className=i%2?s:s+"Alt";
    dg.lastChild.appendChild(t[j])
 }while(i--)
 if(ft)
 {t=dg.lastChild.removeChild(dg.rows[1])
  dg.lastChild.appendChild(t)
 }
_S.title=(tp?"De":"A")+"scending sort";t=_prv(dg).style.cursor
 _S.style.cursor=tp?t.replace("asc.cur","desc.cur"):t;k=-1
 if(dg.parentNode.id=="")
 {	var i=_S.cellIndex;s=_son1(_nxt(dg.parentNode));t=s.rows[0].cells[i];a=q[0].cells[i]
	a.title=t.title=_S.title;a.style.cursor=t.style.cursor=_S.style.cursor;
	if(ss.length>0) k=ss[0]
	s.lastChild.chOff=k>0&&k%2?"":"Alt";s.lastChild.ch=k
	if(dg.parentNode.style.display!="") _dgFoc(dg)
 }
 else
 {	if(ss.length>0)
	{	dg.summary="";var i=_prv(dg).tabIndex&2?ss.length-1:0;
		do{ var j=ss[i];dg.summary+=j+"="+((j+1)%2)+";";if(k<j) k=j}while(i--)
	}
 }
 if(_prv(dg).onfocus!=null||dg.lastChild.summary!=null) HideFiltered(dg)
 if(k<0) k=_findIdx(dg)
 if(k>=0){rowIn(q[k]);ShowItem(q[k])}
}
function DoSorting()
{if(_S.parentNode.rowIndex!=0)
	return;
 var s=_a_dg();var q=_prv(s)
 if(s.rows.length<3||(q.tabIndex&128)>0||(s.parentNode.id==""&&q.className.substr(0,3)!="Art") )
	return
 _S.style.cursor="wait"
 setTimeout("Sorting()",1)
}
function ddg_click(e)
{_S=_td(e)
 if(_S==null) return
 var q=_a_dg()
 if(_S.parentNode.rowIndex>0)
 {	dClick(q,_up(_S,2).ch)
	if(q.parentNode.style.display!="") _dgFoc(q)
 }
 else
	DoSorting()
}
function toClick(dg)
{if(_prv(dg).value==""||dg.rows.length==0)
	return
 dClick(dg,_dwn(_nxt(dg.parentNode),2).ch)
}
function ddgExp(e)
{_S=_s(e);_dgFoc(_son2(_son1(_prv(_S.parentNode))));toClick(_son2(_son1(_prv(_S.parentNode))))
}
function dgmClick(p,e)
{var dg=_up(p,2);var ix=_prv(dg);var k=-p.tabIndex-1;var s=ix.className
 if((ix.tabIndex&2)!=0)
 {	var x=new String(k+";")
	if(p.className.substr(p.className.length-8,8)!="Selected")
		{ix.value+=x;dg.summary+=p.rowIndex+"="+(p.className.indexOf("Alt")>0?"1":"0")+";";p.className=s+"Selected"}
	else
	{	var ss=dg.summary.split(";")
		for(var j=0;j<ss.length;j++)
		{	var c=ss[j].split("=")
			if(c[0]==p.rowIndex)
				{s=c[1]==0?s:s+"Alt";dg.summary=dg.summary.replace(ss[j]+";","");break}
		}
		p.className=e!=null&&((e.type=="click"&&_s(e).parentNode==p)||e.type=="keydown")?s+"Over":s;ix.value=ix.value.replace(x,"")
	}
 }
 else
 {	if(ix.value!="")
	{	var c=dg.summary.split("=");dg.rows[c[0]].className=c[1]==0?s:s+"Alt"
		if(p.rowIndex==c[0]){dg.summary=ix.value="";return}
	}
	dg.summary=p.rowIndex+"="+(p.className.indexOf("Alt")>0?"1":"0");p.className=s+"Selected";ix.value=k+";"+p.cells[0].firstChild.data
 }
 p.parentNode.ch=p.rowIndex;p=_son3(ix.parentNode)
 if(p.onclick!=null)
	p.click()
}
function dgm_click(e)
{_S=_td(e)
 if(_S==null) return
 var p=_S.parentNode;var d=_up(p,2)
 if(d.style.zIndex==0){var y=d.parentNode.scrollTop;d.parentNode.focus();_dgFoc(d);d.style.zIndex=1;d.parentNode.scrollTop=y}
 if(p.className.substr(0,3)=="Art"){dgmClick(p,e);_clearCh(d)}
 else DoSorting()
}
function cal_over(e)
{var p=_s(e)
 if(p.className=="days")
	p.className="dayMouseOver"
}
function cal_out(e)
{var p=_s(e)
 if(p.className=="dayMouseOver"&&p.tabIndex!=0)
	p.className="days"
}
function KDate(e,o)
{if(e.type=="keyup"&&_k(e)==9){_prv(_up(o,2)).style.display="none";return}
 var ss=o.value;var i=ss.length-1
 if(i>=0) do{
	var s=ss.substr(i,1)
	if((s<"0"||s>"9")&&s!="/") ss=ss.replace(s,"")
 }while(i--)
 if(o.value!=ss)
	o.value=ss
}
function _my(dg,i)
{var s=_dwn(_son1(dg).rows[0].cells[i],6).value
 return i==0?s.substr(0,s.indexOf(";")):s.substr(s.indexOf(";")+1)
}
function _unF(q,i){_UnFoc(_son1(_son1(q).rows[0].cells[i]))}
function _MiMa(p,i)
{var ss=p.split("/");var j=parseInt(ss[0])*12+parseInt(ss[1],10)-1
 return i>j?0:i==j?parseInt(ss[2],10):32
}
function _sDateF(y,m,d){return _DateF==12?(++m)+"/"+d+"/"+y:_DateF==31?d+"/"+(++m)+"/"+y:y+"/"+(++m)+"/"+d}
function CreateDate(s)
{if(_DateF!=12){var ss=s.split("/");s=_DateF==31?ss[1]+"/"+ss[0]+"/"+ss[2]:ss[1]+"/"+ss[2]+"/"+ss[0]}
 var d=new Date(s)
 return isNaN(d)?null:d
}
function showCal(q,f)
{if(q.style.display!="") return;
 SetBody(q,q.offsetHeight);var p=CreateDate(_dwn(_nxt(q),2).value)
 var m=_my(q,0);var y=_my(q,1);var t=_son1(q);var day=0
 if(p!=null&&p.getMonth()==m&&p.getFullYear()==y) day=p.getDate()
 var k=1-(new Date(y,m,1)).getDay();var min=0;var max=32;var i
 var dm=(m==1&&((y%4==0&&y%100!=0)||y%400==0))?29:(new Array(31,28,31,30,31,30,31,31,30,31,30,31))[m];
 var fday=_son2(q).value
 if(fday==""||fday>dm){_son2(q).value=fday=15}
 var sR=_son2(q).title.split(";")
 if(sR.length==2)
	{i=y*12+parseInt(m);min=_MiMa(sR[0],i);max=_MiMa(sR[1],i)}
 for(i=2;i<8;i++)
 {	for(var j=0;j<7;j++)
	{	p=t.rows[i].cells[j];p.tabIndex=-1
		if(k>0&&k<=dm)
		{	p.firstChild.data=k;p.className=k>=min&&k<=max?k!=day?"days":"daySelected":"dayVoid";
			if(k==fday)
			{	p.tabIndex=0
				if(f==1){_unF(q,0);_unF(q,1);p.focus()}
				if(k!=day&&k>=min&&k<=max) p.className="dayMouseOver";
			}
		}
		else
		{	if(i==7&&j>=2) break;
			p.firstChild.data="0";p.className="dayEmpty"
		}
		k++
	}
 }
}
function Cal508(e)
{var k=_k(e)
 if(k==9) return true
 var _S=_s(e)
 if(k==32)
 {	if(_S.type=="button") _S.click()
	else cal_click(e)
	return false
 }
 while(_S.id.substr(_S.id.length-5,5)!="clndr"){_S=_S.parentNode}
 if(k>36&&k<41)
 {	var d=new Date(_my(_S,1),parseInt(_my(_S,0)),_son2(_S).value).getTime()+28800000;var day=86400000
	d=new Date(k==37?d-day:k==38?d-7*day:k==39?d+day:d+7*day);
	_son2(_S).value=d.getDate()
	SelectRow(_S.id.replace("_clndr","_ddgMonth"),d.getMonth())
	SelectGridItem(_S.id.replace("_clndr","_ddgYear"),d.getFullYear())
	showCal(_S,1)
	return false
 }
 if(k==27){_S.style.display="none";_dwn(_nxt(_S),2).focus()}
 return false
}
function cal_c(t,s)
{t.style.display="none";var p=_nxt(t);var q=_dwn(p,2)
 if(q.value!=s)
 {	q.value=s;q=_son3(p)
	if(q.onclick!=null)
		q.click()
 }
 _dwn(_nxt(t),2).focus()
}
function cal_click(e)
{var p=_s(e)
 if(p.className=="dayMouseOver"||p.className=="daySelected")
 {	var t=_up(p,4);_son2(t).value=p.firstChild.data
	cal_c(t,_sDateF(_my(t,1),_my(t,0),p.firstChild.data))
 }
}
function cal_close(q){q=_up(q,5);q.style.display='none';_dwn(_nxt(q),2).focus()}
function expCal(e)
{var q=_s(e)
 if(q.style.cursor==""&&_k(e)!=32) return;
 q=q.parentNode
 if(q.tagName.toUpperCase()!="DIV")
	q=q.parentNode;
 q=_prv(q);q.style.display="";var p1=_dwn(_son1(q).rows[0].cells[0],1);var p=_son2(_dwn(p1,4));var s=_prv(p).style
 if(s.height=="")
 {	var d=p.parentNode;d.style.overflow=p.lastChild.style.overflow="";toClick(p)
	var h=IE?p.offsetHeight-d.clientHeight+d.offsetHeight:p.lastChild.scrollHeight
	h=IE&&!QM?h-d.offsetHeight+d.clientHeight:h;
	_son1(_dwn(_son1(q).rows[0].cells[1],5)).style.height=s.height=h+"px"
	toClick(p)
 }
 showCal(q)
 if(!IE){ff_clck(e);e.stopPropagation()}
 p1.focus();_dgFoc(p)
}
function CalC508(e)
{var k=_k(e)
 if(k==32&&_son2(_up(_s(e),2)).style.cursor!="") expCal(e)
 return k>46&&k<58
}
function FreezeH(dg)
{var div=dg.parentNode
 if(_son1(dg).tagName.toUpperCase()=="THEAD") return
 div.style.overflow=IE?"auto":"hidden";div.style.overflowX="hidden"
 var b=dg.lastChild;b.tabIndex=-1;var r=b.removeChild(dg.rows[0])
 if(r==null) return
 var h=document.createElement("THEAD");h.appendChild(r);dg.insertBefore(h,_son1(dg))
 if(!IE){b.style.overflow="auto";b.style.overflowX="hidden"}
}
function RenderMG(dg)
{var pr=dg.parentNode;var ix=_prv(dg);pr.style.height=ix.style.height;pr.style.width=ix.style.width;FreezeH(dg)
 if(IE)
	pr.onfocusout=function(){mdg_out(event,this)}
 var p=dg.rows;var l=p.length
 if((ix.tabIndex&4)!=0&&l>1)
 {	p[--l].title=""
	if(p[l].className=="") p[l].className=p[0].className;
 }
 p[0].style.display=(ix.tabIndex&8)>0?"none":"";dg.summary=""
 if(ix.style.height=="") _mg=dg
 if(ix.value!="")
 {	var s=ix.className;var ss=ix.value.split(";");var i=(ix.tabIndex&2)!=0?ss.length-2:0;
	do{var j=parseInt(ss[i])+1;dg.summary+=p[j].rowIndex+"="+(p[j].className.indexOf("Alt")>0?"1":"0")+";";p[j].className=s+"Selected"
	}while(i--)
 }
 if((ix.tabIndex&64)>0) return
 dg.style.display=""
 if(IE&&!QM){p=_nxt(pr);p.className=pr.className;p=p.style;pr=pr.style;p.height=pr.height;p.width=pr.width;p.display="";pr.position="absolute"}
}
function IsBlur(e,p)
{var q=document.activeElement
 if((e!=null&&e.y<0)||q==null) return true;
 while(q.tagName.toUpperCase()!="BODY"&&q!=p)
 {q=q.parentNode;if(q.tagName==null) return false}
 return q!=p
}
function _altCSS(q,c)
{if(q.className.substr(q.className.length-c.length,c.length)!=c) q.className+=c
}
function _mdg_out(p){_clearCh(p);p.style.zIndex=0;_UnFoc(p.parentNode)}
function mdg_out(e,p)
{if(IsBlur(e,p)) _mdg_out(_son2(p))
}
function _ddg_out(dg)
{_UnFoc(_up(dg,5));
 if(dg.parentNode.style.display==""&&(_prv(dg).tabIndex&4)==0) toClick(dg)
}
function ddg_out(e,dg)
{if(IsBlur(e,dg)) _ddg_out(_son2(_dwn(dg,4)))
}
function ff_clck(e)
{var q=e.target;var g=q
 while(q!=document.body)
 {	var s=q.tagName.toUpperCase();var d=q.id.substr(q.id.length-5,5)
	if(s=="DIV"&&d=="ArtCC"){q=_dwn(q,2);break}
	if(s=="TABLE"&&d=="ArtDD") g=_son2(_dwn(q,4))
	else if(s=="DIV"&&d=="ArtMM") g=_son2(q)
	q=q.parentNode
 }
 var p=document.getElementsByTagName("TABLE");var i=p.length-1
 if(i>=0) do{
	var dg=p[i]
	if(dg==q||dg==g) continue
	var s=dg.id.substr(dg.id.length-5,5)
 	if(s=="ArtDT")
	{	if((_nxt(dg).tabIndex&4)==0) dg.parentNode.style.display="none"
		continue
	}
	if(s=="ArtDG")
	{	if(dg.parentNode.id=="") _ddg_out(dg)
		else _mdg_out(dg)
	}
 }while(i--)
}
function RenderDDG(dg)
{FreezeH(dg);var q=_nxt(_up(dg,2));var ix=_prv(dg);q.className=dg.rows[0].className;q.style.height="auto"
 q.style.display=(ix.tabIndex&2)==0||dg.rows.length<3||ix.className.substr(0,3)!="Art"?"none":""
 q=_up(dg,5);dg.parentNode.className=q.className
 if(IE)	q.onfocusout=function(){ddg_out(event,this)}
 if(ix.style.width!="")	q.style.width=ix.style.width
 if((ix.tabIndex&64)>0) return;
 q.style.display=""
 q=_son1(_nxt(dg.parentNode));var p=_son1(q)
 q.rows[0].className=dg.rows[0].className
 q.rows[0].style.display=dg.rows[0].style.display=(ix.tabIndex&8)>0?"none":""
 q.title=dg.rows.length<3||ix.className.substr(0,3)!="Art"?"":"Click to expand"
 if(ix.value=="") return
 var k=parseInt(ix.value)+1;var i=dg.rows[0].cells.length-1
 do{q.rows[1].cells[i].innerHTML=dg.rows[k].cells[i].innerHTML}while(i--)
 p.ch=k;p.chOff=dg.rows[k].className.indexOf("Alt")>0?"Alt":""
 q.rows[1].className=dg.rows[k].className=ix.className+"Selected"
 if((ix.tabIndex&32)!=0&&dg.rows.length>1)
	toClick(dg)
}
var _DateF=12
//??var NN=navigator.appName=="Netscape"
var IE=navigator.appName=="Microsoft Internet Explorer"
var QM=document.compatMode=="BackCompat"
function art_onload()
{if(!IE&&_marginBottom<12) _marginBottom=12
 var q=document.body;q.parentNode.style.overflow=q.style.overflow=""
 if(!IE) q.onclick=ff_clck
 q=q.style;q.paddingBottom=0;q.marginBottom=_marginBottom
 var p=document.getElementsByTagName("TABLE");var i=p.length-1
 if(i>=0) do{
	var dg=p[i];var s=dg.id.substr(dg.id.length-5,5)
	if(s=="ArtDT")
	{	if(IE&&(_nxt(dg).tabIndex&4)==0)
			_up(dg,2).onfocusout=function(){if(IsBlur(event,this)) _son1(this).style.display="none"}
		q=_dwn(_nxt(dg.parentNode),2);q.onblur=function(){this.className=this.className.replace("Focus","")}
		_DateF=_nxt(dg).alt
		if((_nxt(dg).tabIndex&1)!=1)
			q.readOnly=true
		continue
	}
	if(s!="ArtDG")
		continue;
	if(dg.parentNode.id=="") RenderDDG(dg)
	else RenderMG(dg)
 }while(i--)
 p=document.getElementsByTagName("INPUT");i=p.length-1
 if(i>=0) do{
	q=p[i]
	if(q.className=="btn"||q.className=="bttn")
	{	q.onmouseover=function(){_altCSS(this,"Hover")}
		q.onmouseout=function(){this.className=this.className.replace("Hover","")}
	}
 }while(i--)
 p=window.onresize;window.onresize=p==null?function(){RefreshDG()}:function(){p();RefreshDG()}//??Netscape
 p=window.onscroll;window.onscroll=p==null?function(){RefreshDG()}:function(){p();RefreshDG()}//??Netscape
 RefreshDG()
}
