$(document).ready(function () {
    $("#filterRegions").hide();
    $("#filterWorkflows").click(function () {
        var pdn = $("#pdn").val();
        var org = $("#organization").val();
        var region = $("#region").val();
        var zone = $("#zone").val();
        //var fund = $("#fund").val();
        var baCode = $("#baCode").val();
        //var pegasysTitleNumber = $("#pegasysTitleNumber").val();
        //var pegasysVendorName = $("#pegasysVendorName").val();
        filter(pdn, org, region, zone, baCode);
    });

    $("#clearFilters").click(function() {
        $("#pdn").val("");
        $("#organization").val("");
        $("#region").val("");
        $("#zone").val("");
        $("#baCode").val("");
        filter();
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



function filter(pdn, org, region, zone, baCode) {
    var url = "/Ulo/Search?";
    url += "pegasysDocumentNumber=" + (pdn ? pdn : "");
    url += "&organization=" + (org ? org : "");
    url += "&region=" + (region ? region : "");
    url += "&zone=" + (zone ? zone : "");
    //url += "&fund=" + fund;
    url += "&baCode=" + (baCode ? baCode :"");
    //url += "&pegasysTitleNumber=" + pegasysTitleNumber;
    //url += "&pegasysVendorName=" + pegasysVendorName;
    $("#masterRecordsListing").load(url);
}