<!--
function Get_Item(i)
    {
    //debugger
        var td_1 = document.getElementById("td_item1")
        var td_2 = document.getElementById("td_item2")
        
        if(i =='1')
        {
           td_1.className = "lrgBldWhiteText";
           td_2.className = "lrgBldBlackTextGrBckgrd";
        }
        else
        { 
           td_1.className = "lrgBldBlackTextGrBckgrd";
           td_2.className = "lrgBldWhiteText";
        }
    }
    
//debugger
function Substract(contr1,contr2,contr_res)
 {
    var n1 = document.getElementById(contr1).value
    n1 =  n1.replace('$','');
    n2 = document.getElementById(contr2).value
    n2 =  n2.replace('$','');
    var diff = n1-n2;
    diff = diff.toFixed(2);
    //alert(diff);
    //debugger
    document.getElementById(contr_res).value = diff;
    //document.getElementById(contr_res).readOnly = true;
 }
 
 //debugger
 function Divide(contr1,contr2,contr_res, factor, symbol)
 {
    var n1 = document.getElementById(contr1).value
    n1 =  n1.replace('$','');
    var n2 = document.getElementById(contr2).value
    n2 =  n2.replace('$','');
    if(n2!='' && n2!='0')
    {
        var res = n1*factor/n2;
        res = res.toFixed(2);
        res = res + symbol;
        document.getElementById(contr_res).value = res;
        //document.getElementById(contr_res).readOnly = true;
    }
 }
    
    -->