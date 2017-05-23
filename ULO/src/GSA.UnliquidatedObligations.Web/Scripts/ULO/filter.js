$(document).ready(function () {
    $("#filterRegions").hide();
    $("#filterWorkflows").click(function () {
        var pdn = encodeURI($("#pdn").val());
        var org = encodeURI($("#organization").val());
        var region = encodeURI($("#region").val());
        var zone = encodeURI($("#zone").val());
        var fund = encodeURI($("#fund").val());
        var baCode = encodeURI($("#baCode").val());
        var docType = encodeURI($("#docType").val());
        var ptn = encodeURI($("#ptn").val());
        var pvn = encodeURI($("#pvn").val());
        var contractingOfficersName = encodeURI($("#contractingOfficersName").val());
        var awardNumber = encodeURI($("#awardNumber").val());
        var reasonIncludedInReview = encodeURI($("#reasonIncludedInReview").val());
        var valid = encodeURI($("#valid").val());
        var status = encodeURI($("#status").val());
        filter(pdn, org, region, zone, fund, baCode, docType, ptn, pvn, contractingOfficersName, awardNumber, reasonIncludedInReview, valid, status);
    });

    $("#clearFilters").click(function() {
        $("#pdn").val("");
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
    url += "&fund=" + (fund ? fund : "");
    url += "&baCode=" + (baCode ? baCode : "");
    url += "&docType=" + (docType ? docType : "");
    url += "&pegasysTitleNumber=" + (ptn ? ptn : "");
    url += "&pegasysVendorName=" + (pvn ? pvn : "");
    url += "&contractingOfficersName" + (contractingOfficersName ? contractingOfficersName : "");
    url += "&awardNumber" + (awardNumber ? awardNumber : "");
    url += "&reasonIncludedInReview" + (reasonIncludedInReview ? reasonIncludedInReview : "");
    url += "&valid=" + (valid ? valid : "");
    url += "&status=" + (status ? status : "");
    $("#masterRecordsListing").load(url);
}