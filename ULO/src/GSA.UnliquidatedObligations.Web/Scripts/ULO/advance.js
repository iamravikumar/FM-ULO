var select, submitActor;


$(document).ready(function() {

    if (justificationNeeded()) {
        select = document.getElementById("justifications");
        if (select && select.length === 1) { select.selectedIndex = 0; }
    }

    $("#validateAnswerMessage").hide();
    $("#validateJustificationMessage").hide();
    $("#validateExpectedDateMessage").hide();
    $("#validateCommentMessage").hide();

    if ($("#ExpectedDateForCompletionEditable").val() == "False") {
        $("#ExpectedDateForCompletion").attr('type', 'text');
        $("#ExpectedDateForCompletion").attr('readonly', true);
    }

    var $form = $('#uloDetailsForm');
    var $submitActors = $form.find('input[type=submit]');

    var showExpectedDateBool = $("#ExpectedDateAlwaysShow").val() == "True" || $("#Answer").val() === "Valid" || $("#Answer").val() === "Approve" || $("#ExpectedDateForCompletion").val() !== "";

    showExpectedDate(showExpectedDateBool);


    $submitActors.click(function () {
        submitActor = this;
    });

    $form.submit(function () {
        if (submitActor.name === "Advance") {
            if ($("#Answer").val() === "") {
                $("#validateAnswerMessage").show();
                return false;
            }
            if (justificationNeeded() && $("#justifications").val() === "") {
                $("#validateJustificationMessage").show();
                return false;
            }
            if ($("#ExpectedDateForCompletionNeeded").val() === "True" && ($("#Answer").val() === "Valid" || $("#Answer").val() === "Approve") && $("#ExpectedDateForCompletion").val() === "") {
                $("#validateExpectedDateMessage").show();
                return false;
            }

            if ($("#justifications option:selected").text() === "Other" && $("#Comments").val() == "") {
                $("#validateCommentMessage").show();
                return false;
            }

           
        }

        $("#validateAnswerMessage").hide();
        $("#validateJustificationMessage").hide();
        return true;
    })
});

function justificationNeeded() {
    return $("#justifications").length > 0;
}

function showExpectedDate(showBool) {
    if (showBool) {
        $("#expectedDateForCompletionContainer").show();     
    }
    else {
        $("#expectedDateForCompletionContainer").hide();
        $("#expectedDate").val("");
    }
}

function ChoiceChange(value, model) {
    if (justificationNeeded()) {
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

    if (value == "Valid" || value == "Approve" || $("#ExpectedDateAlwaysShow").val() == "True") {
        showExpectedDate(true);
    }
    else {
        showExpectedDate(false);
    }

}