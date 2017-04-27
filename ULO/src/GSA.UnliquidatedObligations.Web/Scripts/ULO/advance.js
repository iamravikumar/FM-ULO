//$(document).ready(function () {
//    $("#Answer").change(function () {
//        if ($(this).find(":selected").text() === "Other") {
//            $(".other-comments").show();
//        } else {
//            $(".other-comments").hide();
//        }
//    });
//});

function ChoiceChange(value, model) {
    var select = document.getElementById("justifications");
    var justifications = model.QuestionChoices.filter(function(qc) {
        return qc.Value === value;
    })[0].Justifications;

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