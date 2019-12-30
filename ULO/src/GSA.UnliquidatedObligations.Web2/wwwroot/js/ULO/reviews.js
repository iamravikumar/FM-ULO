/*! reviews.js */
$(document).ready(function () {
    showHideRegion();
    $("#ReviewScopeId").change(function () {
        showHideRegion();
    });
});

function showHideRegion() {
    if ($("#ReviewScopeId").val() == "1")
        $("#reviewRegionContainer").show();
    else
        $("#reviewRegionContainer").hide();
}
