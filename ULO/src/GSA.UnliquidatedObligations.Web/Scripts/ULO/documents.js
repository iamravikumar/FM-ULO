$(document).ready(function() {
    $(".document-modal").on("hidden.bs.modal", function () {
        clearDocument();
        hideErrorMsg();
    });

    addDocumentDeleteClick();
    addDocumentSaveClick();
});

function addDocumentSaveClick() {
    $(".save-document").on("click", function () {
        hideErrorMsg();
        $(this).prop('disabled', true);
        var documentId = $(this).data("target");
        var workflowId = getParameterByName("workflowId");
        var documentTypeId = $("#" + documentId + "ModalDocumentType").val();
        var documentName = $("#" + documentId + "ModalDocumentName").val();
        if (documentTypeId === "") {
            showErrMsg("You must select a Document Type before saving", $(this));
            $(this).prop('disabled', false);
        } else if (documentName === "") {
            showErrMsg("You must enter a Document Name before saving", $(this));
            $(this).prop('disabled', false);
        } else {
            saveDocument(documentId, documentName, workflowId, documentTypeId);
            $(this).prop('disabled', false);
        }

    });
}

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
            $("#0ModalDocumentName").val("");
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

function updateDocumentList(documentId, document) {
    $(".documents-heading-row").addClass("show").removeClass("hide");
    var tableRowString = "<tr id='document" + document.Id + "'><td>" + document.Name + "</td><td>" + document.DocumentTypeName + "</td><td>" + document.UserName + "</td><td>" + document.UploadedDate + "</td><td><a data-target='' data-toggle='modal' href='#" + document.Id + "Modal'>View</a> | <a data-toggle='modal' data-target='#" + document.Id + "ModalDelete' href='#" + document.Id + "ModalDelete'>Delete</a></td></tr>"
    if (documentId === 0) {
        $(".documents-list > tbody:last-child").append(tableRowString);
    } else {
        $("#document" + document.Id).replaceWith(tableRowString);
    }
}

function deleteDocumentRow(documentId) {
    $("#document" + documentId).remove();
}

function saveDocument(documentId, documentName, workflowId, documentTypeId) {
    var url = "/Documents/Save?";
    url += "documentId=" + documentId;
    url += "&documentName=" + documentName;
    url += "&workflowId=" + workflowId;
    url += "&documentTypeId=" + documentTypeId;
    $.ajax({
        type: "POST",
        url: url,
        success: function (result) {
            closeModal();
            updateDocumentList(documentId, result);
            loadDocumentModal(result.Id, addDocumentDeleteClick, addDocumentSaveClick);

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