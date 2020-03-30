/*! reviews.js */
$(document).ready(function () {
    showHideRegion();
    $("#ReviewScopeId").change(function () {
        showHideRegion();
    });
});

function showHideRegion() {
    if ($("#ReviewScopeId").val() == "Region")
        $("#reviewRegionContainer").show();
    else
        $("#reviewRegionContainer").hide();
}
