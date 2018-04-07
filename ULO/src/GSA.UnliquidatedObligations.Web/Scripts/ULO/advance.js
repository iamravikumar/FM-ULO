var select, submitActor;

function clearValidationErrors() {
    $("#validateAnswerMessage,#validateJustificationMessage,#validateExpectedDateMessage,#validateCommentMessage,#validateCommentMessageMax").hide();
}

function clearJustificationValidationErrors() {
    $("#validateJustificationMessage").hide();
}

function clearDateValidationErrors() {
    $("#validateExpectedDateMessage").hide();
}

$(document).ready(function () {

    $("#validateAnswerMessage").hide();
    $("#validateJustificationMessage").hide();
    $("#validateExpectedDateMessage").hide();
    $("#validateCommentMessage").hide();


    var $form = $('#uloDetailsForm');
    var $submitActors = $form.find('input[type=submit]');

    $submitActors.click(function () {
        submitActor = this;
    });

    $form.submit(function () {
        var needsAnswer = $("#q").val() == "" || $("#q").val() == null;
        var justificationVal = $("#justifications").val();
        var justificationNeeded = $("#justifications").children().length > 1 && $("#justifications").is(":visible");
        var expectedDateForCompletionNeeded = $("#ExpectedDateForCompletion").is(":visible");
        debugAlert(
                "form.submit checker: \n" +
                "submitActor.name=" + submitActor.name + "\n" +
                "submitActor.value=" + submitActor.value + "\n" +
                "needed=" + justificationNeeded + "\n" +
                "J_len=" + $("#justifications").children().length + "\n" +
                "j_val=[" + justificationVal + "]" + "\n" +
                "justificationNeeded=" + justificationNeeded + "\n" +
                "expectedDateForCompletionNeeded=" + expectedDateForCompletionNeeded + "\n" +
                "needsAnswer=" + needsAnswer + "\n" +
                "answer=[" + $("#q").val()+"]"
                );
        var allowSubmit = true;
        var comment = $("#Comments").val();
        clearValidationErrors();
        if (submitActor.value == "Submit") {
            if (needsAnswer) {
                $("#validateAnswerMessage").show();
                allowSubmit = false;
            }
            if (justificationNeeded) {
                if (justificationVal == null || justificationVal == "") {
                    $("#validateJustificationMessage").show();
                    allowSubmit = false;
                }
            }
            if (expectedDateForCompletionNeeded && $("#ExpectedDateForCompletion").val() === "") {
                $("#validateExpectedDateMessage").show();
                allowSubmit = false;
            }
            if ($("#justifications option:selected").text() === "Other" && comment == "") {
                $("#validateCommentMessage").show();
                allowSubmit = false;
            }
        }
        comment = comment || "";
        if (comment.length > 4000) {
            $("#validateCommentMessageMax").show().text("The maximum comment size is 4000 and you entered "+comment.length+" characters");
            allowSubmit = false;
        }
        debugAlert(
            "form.submit checker: \n" +
            "result=" + allowSubmit
        );
        return allowSubmit;
    })
});

function ChoiceChange() {
    clearValidationErrors();
    var value = $("#q").val();
    var mrjk = $("#justifications").attr("mrjk");
    debugAlert(
        "ChoiceChange\n"+
        "value= [" + value + "]\n" +
        "mrjk= [" + mrjk + "]\n" +
        "please= [" + pleaseSelectOneMessage + "]"
        );

    var select = $("#justifications")[0];
    select.options.length = 0;
    var el = document.createElement("option");
    el.textContent = pleaseSelectOneMessage;
    el.value = "";
    el.selected = true;
    el.disabled = true;
    select.appendChild(el);

    var jc = 0;
    var q = questionChoiceByQuestionChoiceValue[value];

    if (q != null) {
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
                el.selected = key == mrjk;
                select.appendChild(el);
                ++jc;
            }
        }
    }

    if (jc == 0) {
        $(select.parentElement).hide();
    }
    else {
        $(select.parentElement).show();
    }

    if (q!=null && q.expectedDateAlwaysShow) {
        $("#expectedDateForCompletionContainer").show();
    }
    else {
        $("#expectedDateForCompletionContainer").hide();
    }
}
