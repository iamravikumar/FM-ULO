var select, submitActor;


$(document).ready(function() {

    $("#validateAnswerMessage").hide();
    $("#validateJustificationMessage").hide();
    $("#validateCommentMessage").hide();

    var $form = $('#uloDetailsForm');
    var $submitActors = $form.find('input[type=submit]');

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

function ChoiceChange(value, pleaseSelect, justificationKey) {
    debugAlert('ChoiceChange("' + value + '", "' + pleaseSelect + '", "' + justificationKey + '")');

    var select = $("#justifications")[0];
    select.options.length = 0;
    var el = document.createElement("option");
    el.textContent = pleaseSelect;
    el.value = "";
    el.disabled = true;
    el.selected = justificationKey==null;
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
}
