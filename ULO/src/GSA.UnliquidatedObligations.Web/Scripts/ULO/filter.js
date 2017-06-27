$(document).ready(function () {
    $("#filterRegions").hide();

    $("#clearFilters").click(function() {
        $("#pegasysDocumentNumber").val("");
        $("#organization").val("");
        $("#region").val("");
        $("#zone").val("");
        $("#fund").val("");
        $("#baCode").val("");
        $("#docType").val("");
        $("#ptn").val("");
        $("#pvn").val("");
        $("#contractingOfficersName").val("");
        $("#awardNumber").val("");
        $("#reasonIncludedInReview").val("");
        $("#valid").val("");
        $("#status").val(""); 
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
    url += "&fund=" + (fund ? fund : "");
    url += "&baCode=" + (baCode ? baCode : "");
    url += "&docType=" + (docType ? docType : "");
    url += "&pegasysTitleNumber=" + (ptn ? ptn : "");
    url += "&pegasysVendorName=" + (pvn ? pvn : "");
    url += "&contractingOfficersName=" + (contractingOfficersName ? contractingOfficersName : "");
    url += "&awardNumber=" + (awardNumber ? awardNumber : "");
    url += "&reasonIncludedInReview=" + (reasonIncludedInReview ? reasonIncludedInReview : "");
    url += "&valid=" + (valid ? valid : "");
    url += "&status=" + (status ? status : "");
    $("#masterRecordsListing").load(url);
}