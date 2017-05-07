$(document).ready(function() {
    $('.document-modal').on('hidden.bs.modal', function () {
        clearDocument();
    });

    $('.save-document').on('click', function () {
        var documentId = $(this).data("target");
        var workflowId = getParameterByName('workflowId');
        var documentTypeId = $("#" + documentId + "ModalDocumentType").val();
        saveDocument(documentId, workflowId, documentTypeId);
    });
});

function deleteTempAttachments() {
    $("tr.temp-attachment").remove();
}

function clearDocument() {
    $.ajax({
        type: "POST",
        url: "/Documents/Clear",
        success: function (result) {
            deleteTempAttachments();
        },
        error: function (xhr, status, p3, p4) {
            var err = "Error " + " " + status + " " + p3 + " " + p4;
            if (xhr.responseText && xhr.responseText[0] == "{")
                err = JSON.parse(xhr.responseText).Message;
            console.log(err);
        }
    });
    return false;
}

function closeModal() {
    $('.document-modal').modal('hide');
}

function addDocumentRow(document) {
    $(".documents-list > tbody:last-child").append("<tr><td>" + document.AspNetUser.UserName + "</td><td>" + document.DocumentType.Name + "</td><td><a data-target='' data-toggle="modal" href="#14Modal">View</a> |<a href="/Documents/Delete?documentId=14">Delete</a></td></tr>");
}

function saveDocument(documentId, workflowId, documentTypeId) {
    $.ajax({
        type: "POST",
        url: "/Documents/Save?documentId=" + documentId + "&workflowId=" + workflowId + "&documentTypeId=" + documentTypeId,
        success: function (result) {
            closeModal();
            if (documentId == 0) {
                console.log(result)
            }
        },
        error: function (xhr, status, p3, p4) {
            var err = "Error " + " " + status + " " + p3 + " " + p4;
            if (xhr.responseText && xhr.responseText[0] == "{")
                err = JSON.parse(xhr.responseText).Message;
            console.log(err);
        }
    });
    return false;
}

function getParameterByName(name, url) {
    if (!url) url = window.location.href;
    name = name.replace(/[\[\]]/g, "\\$&");
    var regex = new RegExp("[?&]" + name + "(=([^&#]*)|&|#|$)"),
        results = regex.exec(url);
    if (!results) return null;
    if (!results[2]) return '';
    return decodeURIComponent(results[2].replace(/\+/g, " "));
}