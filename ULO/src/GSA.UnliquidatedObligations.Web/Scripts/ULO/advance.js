var select, submitActor;


$(document).ready(function() {

    select = document.getElementById("justifications");
    if (select && select.length === 1) { select.selectedIndex = 0; }

    $("#validateAnswerMessage").hide();
    $("#validateJustificationMessage").hide();

    var $form = $('#uloDetailsForm');
    var $submitActors = $form.find('input[type=submit]');


    $submitActors.click(function () {
        submitActor = this;
    });

    $form.submit(function () {
        if (submitActor.name === "Advance") {
            if ($("#Answer").val() === "") {
                $("#validateAnswerMessage").show();
                return false;
            }
            if ($("#justifications").val() === "") {
                $("#validateJustificationMessage").show();
                return false;
            }
           
        }

        $("#validateAnswerMessage").hide();
        $("#validateJustificationMessage").hide();
        return true;
    })
});

function ChoiceChange(value, model) {
    var justifications = [];
    if (value != "") {
        justifications = model.QuestionChoices.filter(function (qc) {
            return qc.Value === value;
        })[0].Justifications;
    }

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