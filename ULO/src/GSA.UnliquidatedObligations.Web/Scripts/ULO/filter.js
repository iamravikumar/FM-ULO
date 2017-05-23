$(document).ready(function () {
    $("#filterRegions").hide();
    $("#filterWorkflows").click(function () {
        var pdn = $("#pdn").val();
        var org = $("#organization").val();
        var region = $("#region").val();
        var zone = $("#zone").val();
        var fund = $("#fund").val();
        var baCode = $("#baCode").val();
        var docType = $("#docType").val();
        var ptn = $("#ptn").val();
        var pvn = $("#pvn").val();
        var contractingOfficersName = $("#contractingOfficersName").val();
        var awardNumber = $("#awardNumber").val();
        var reasonIncludedInReview = $("#reasonIncludedInReview").val();
        var valid = $("#valid").val();
        var reviewedBy = $("#reviewedBy").val();
        var status = encodeURI($("#status").val());
        //var pegasysTitleNumber = $("#pegasysTitleNumber").val();
        //var pegasysVendorName = $("#pegasysVendorName").val();
        filter(pdn, org, region, zone, fund, baCode, docType, ptn, pvn, contractingOfficersName, awardNumber, reasonIncludedInReview, valid, reviewedBy, status);
    });

    $("#clearFilters").click(function() {
        $("#pdn").val("");
        $("#organization").val("");
        $("#region").val("");
        $("#zone").val("");
        $("#baCode").val("");
        $("#docType").val("");
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



function filter(pdn, org, region, zone, fund, baCode, docType, ptn, pvn, contractingOfficersName, awardNumber, reasonIncludedInReview, valid, reviewedBy, status) {
    var url = "/Ulo/Search?";
    url += "pegasysDocumentNumber=" + (pdn ? pdn : "");
    url += "&organization=" + (org ? org : "");
    url += "&region=" + (region ? region : "");
    url += "&zone=" + (zone ? zone : "");
    url += "&fund=" + fund;
    url += "&baCode=" + (baCode ? baCode : "");
    url += "&docType=" + (docType ? docType : "");
    url += "&pegasysTitleNumber=" + (ptn ? ptn : "");
    url += "&pegasysVendorName=" + (pvn ? pvn : "");
    url += "&contractingOfficersName" + (contractingOfficersName ? contractingOfficersName : "");
    url += "&awardNumber" + (awardNumber ? awardNumber : "");
    url += "&reasonIncludedInReview" + (reasonIncludedInReview ? reasonIncludedInReview : "");
    url += "&valid=" + (valid ? valid : "");
    url += "&reviewedBy=" + (reviewedBy ? reviewedBy : "");
    url += "&status=" + (status ? status : "");
    $("#masterRecordsListing").load(url);
}