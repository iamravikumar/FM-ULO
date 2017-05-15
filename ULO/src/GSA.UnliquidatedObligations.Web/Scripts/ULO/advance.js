var select;

$(document).ready(function() {

    select = document.getElementById("justifications");
    if (select) {select.selectedIndex = 0;}
});

function ChoiceChange(value, model) {
    
    var justifications = model.QuestionChoices.filter(function(qc) {
        return qc.Value === value;
    })[0].Justifications;

    if (justifications.length > 0) {
        select.options.length = 0;
        var el = document.createElement("option");
        el.textContent = "Select...";
        el.value = "";
        select.appendChild(el);


        for (var i = 0; i < justifications.length; i++) {
            var opt = justifications[i];
            var el = document.createElement("option");
            el.textContent = opt.JustificationText;
            el.value = opt.JustificationId;
            select.appendChild(el);
        }
    }
}