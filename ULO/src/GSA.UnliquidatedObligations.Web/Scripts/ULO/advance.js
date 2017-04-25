$(document).ready(function () {
    if ($("#Justification").find(":selected").text() === "Other") {
        $(".other-comments").show();
    } else {
        $(".other-comments").hide();
    }
    $("#Justification").change(function () {
        if ($(this).find(":selected").text() === "Other") {
            $(".other-comments").show();
        } else {
            $(".other-comments").hide();
        }
    });
});