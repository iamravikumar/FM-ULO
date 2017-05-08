$(document).ready(function() {
    $(".document-modal").on("hidden.bs.modal", function () {
        clearDocument();
        hideErrorMsg();
    });

    addDocumentDeleteClick();

    $(".save-document").on("click", function () {
        var documentId = $(this).data("target");
        var workflowId = getParameterByName("workflowId");
        var documentTypeId = $("#" + documentId + "ModalDocumentType").val();
        if (documentTypeId === "") {
            showErrMsg("You must select a Document Type before saving", $(this));
        } else {
            hideErrorMsg();
            saveDocument(documentId, workflowId, documentTypeId);
        }
        
    });
});

function addDocumentDeleteClick() {
    $(".delete-document").on("click", function () {
        var documentId = $(this).data("target");
        deleteDocument(documentId);
    });
}

function deleteTempAttachments() {
    $("tr.temp-attachment").remove();
}

function clearDocument() {
    $.ajax({
        type: "POST",
        url: "/Documents/Clear",
        success: function (result) {
            deleteTempAttachments();
            $("#0ModalDocumentType").val("");
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
    $(".document-modal").modal("hide");
}

function addDocumentRow(document) {
    $("#" + document.Id + "Modal .document-heading-row").addClass("show").removeClass("hide");
    $(".documents-list > tbody:last-child").append("<tr id='document" + document.Id + "'><td>" + document.UserName + "</td><td>" + document.DocumentTypeName + "</td><td><a data-target='' data-toggle='modal' href='#" + document.Id + "Modal'>View</a> | <a data-toggle='modal' data-target='#" + document.Id + "ModalDelete' href='#" + document.Id + "ModalDelete'>Delete</a></td></tr>");
}

function deleteDocumentRow(documentId) {
    $("#document" + documentId).remove();
}

function saveDocument(documentId, workflowId, documentTypeId) {
    $.ajax({
        type: "POST",
        url: "/Documents/Save?documentId=" + documentId + "&workflowId=" + workflowId + "&documentTypeId=" + documentTypeId,
        success: function (result) {
            closeModal();
            if (documentId == 0) {
                console.log(result);
                addDocumentRow(result);         
            }
            loadDocumentModal(result.Id, addDocumentDeleteClick);
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
    if (!results[2]) return "";
    return decodeURIComponent(results[2].replace(/\+/g, " "));
}

function showErrMsg(msg, location) {
    $(location).siblings(".document-error-msg").html(msg);
    $(location).siblings(".document-error-msg").show();
}

function hideErrorMsg() {
    $(".document-error-msg").hide();
}

function deleteDocument(documentId) {
    $.ajax({
        type: "POST",
        url: "/Documents/Delete?documentId=" + documentId,
        success: function (result) {
            deleteDocumentRow(result.Id);
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