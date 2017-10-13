var select, submitActor;


$(document).ready(function () {

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
        var justificationVal = $("#justifications").val();
        debugAlert("submitActor.name=" + submitActor.name + "; submitActor.value=" + submitActor.value + "; needed=" + justificationNeeded() + "; J_len=" + $("#justifications").children().length + "; j_val=[" + justificationVal + "]");
        if (submitActor.value === "Submit") {
            if ($("#Answer").val() === "") {
                debugAlert("inputError: case1");
                $("#validateAnswerMessage").show();
                return false;
            }
            if (justificationNeeded()) {
                if (justificationVal == null || justificationVal == "") {
                    debugAlert("inputError: case2");
                    $("#validateJustificationMessage").show();
                    return false;
                }
            }
            if ($("#ExpectedDateForCompletionNeeded").val() === "True" && ($("#Answer").val() === "Valid" || $("#Answer").val() === "Approve") && $("#ExpectedDateForCompletion").val() === "") {
                debugAlert("inputError: case3");
                $("#validateExpectedDateMessage").show();
                return false;
            }
            if ($("#justifications option:selected").text() === "Other" && $("#Comments").val() == "") {
                debugAlert("inputError: case4");
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
    return $("#justifications").children().length > 1;
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

function ChoiceChange(value, pleaseSelect, justificationKey) {
    debugAlert('ChoiceChange("' + value + '", "' + pleaseSelect + '", "' + justificationKey + '")');

    var select = $("#justifications")[0];
    select.options.length = 0;
    var mrjk = $(select).attr("mrjk");
    justificationKey = justificationKey == null ? mrjk : justificationKey;
    var el = document.createElement("option");
    el.textContent = pleaseSelect;
    el.value = "";
    el.disabled = true;
    el.selected = justificationKey == null;
    select.appendChild(el);

    var jc = 0;
    var q = questionChoiceByQuestionChoiceValue[value];
    var keys = q.justificationKeys;
    if (keys != null) {
        debugAlert(keys.length + " keys of [" + keys + "]");

        for (x = 0; x < keys.length; ++x) {
            var key = keys[x];
            var j = justificationByKey[key];
            if (j == null) {
                alert("missing key value for [" + key + "]");
                continue;
            }
            var desc = j.Description;
            el = document.createElement("option");
            el.textContent = desc;
            el.value = key;
            el.selected = key == justificationKey;
            select.appendChild(el);
            ++jc;
        }
    }

    while (el.tagName != "DIV" && el != null) {
        el = el.parentElement;
    }

    debugAlert("jc=" + jc + "; el.tagName=" + el.tagName + "; el.id=" + el.id);

    if (el != null) {
        if (jc == 0) {
            $(el.parentElement).hide();
        }
        else {
            $(el.parentElement).show();
        }
    }

    showExpectedDate(q.expectedDateAlwaysShow);
}
