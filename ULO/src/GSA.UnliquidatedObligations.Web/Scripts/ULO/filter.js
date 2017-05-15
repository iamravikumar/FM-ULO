$(document).ready(function () {
    $("#filterRegions").hide();
    $("#filterWorkflows").click(function () {
        var pdn = $("#pdn").val();
        var org = $("#organization").val();
        filter(pdn, org);
    });

    $("#showFilters").click(function() {
        $("#filterRegions").toggle();
        if ($("#filterRegions").is(":visible")) {
            $(this).html("Hide Filters");
        } else {
            $(this).html("Show Filters");
        }
    });
});



function filter(pdn, org) {
    var url = "/Ulo/Search?";
    url += "pegasysDocumentNumber=" + pdn;
    url += "&organization=" + org;
    $("#masterRecordsListing").load(url);
}