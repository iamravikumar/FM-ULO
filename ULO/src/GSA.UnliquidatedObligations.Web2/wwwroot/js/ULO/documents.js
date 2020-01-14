﻿/*! documents.js */
var preventDismiss = false;
$(document).ready(function () {
    addDocumentDeleteClick();
    addDocumentSaveClick();
});

function setButtonActions(actionSwitch) {
    debugLambda("setButtonActions", function () {
        if (!actionSwitch) {
            $(".save-document").prop('disabled', true);
            $(".save-document").text("Saving...");
            $(".close-document-modal").prop('disabled', true);
            $(".attachments-add-btn").prop('disabled', true);
        }
        else {
            $(".save-document").prop('disabled', false);
            $(".save-document").text("Save Document");
            $(".close-document-modal").prop('disabled', false);
            $(".attachments-add-btn").prop('disabled', false);
        }
    }, actionSwitch);
}

function attachmentsPresent(parentDialog) {
    return $(parentDialog).find(".temp-attachment,.attachment-row").length > 0;
}

function addDocumentSaveClick() {
    debugLambda("addDocumentSaveClick", function () {
        $(".save-document").unbind("click");
        $(".save-document").on("click", function () {
            hideErrorMsg();
            setButtonActions(false);

            var documentId = $(this).data("target");
            var workflowId = currentWorkflowId;
            var documentTypeId = $("#" + documentId + "ModalDocumentType").val();
            var documentName = $("#" + documentId + "ModalDocumentName").val();
            if (documentName == "") {
                showDocumentErrMsg("You must enter a Document Name before saving", this);
                setButtonActions(true);
            }
            else if (documentTypeId == "") {
                showDocumentErrMsg("You must select a Document Type before saving", this);
                setButtonActions(true);
            }
            else if (!attachmentsPresent($(this).closest(".modal-dialog")[0])) {
                showDocumentErrMsg("You must add attachments before saving", this);
                setButtonActions(true);
            }
            else {
                saveDocument(documentId, documentName, workflowId, documentTypeId);
            }
        });
    });
}

function addDocumentDeleteClick() {
    $(".delete-document").unbind("click");
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
    $(".modal-backdrop").remove();
    clearDocument();
    hideErrorMsg();
}

function updateDocumentList(documentId, document) {
    debugLambda("updateDocumentList", function () {
        $(".documents-heading-row").addClass("show").removeClass("hide");
        var docTypeNames = "";
        for (var x = 0; x < document.DocumentTypeNames.length; ++x) {
            docTypeNames += "<li>" + document.DocumentTypeNames[x] + "</li>";
        }
        var tableRowString =
            "<tr id='document" + document.Id + "'><td>" +
            document.Name + "</td><td>" +
            '<ul class="document-type-list">' + docTypeNames + "</ul></td><td>" +
            document.AttachmentCount + "</td><td>" +
            document.UserName + "</td><td>" +
            document.UploadedDate + "</td><td>" +
            (documentId > 0 ?
                ("<a data-target='' data-toggle='modal' href='#" + document.Id + "Modal'>View</a> | <a data-toggle='modal' data-target='#" + document.Id + "ModalDelete' href='#" + document.Id + "ModalDelete'>Delete</a>") :
                "page reload required") +
            "</td></tr>";
        if (documentId === 0) {
            $(".documents-list > tbody:last-child").append(tableRowString);
        } else {
            $("#document" + document.Id).replaceWith(tableRowString);
        }
        $("#noDocumentMessage").css("display", "none");
        $("#documentsTable").css("display", "");
    }, documentId, document);
}

function deleteDocumentRow(documentId) {
    $("#document" + documentId).remove();
}

var newRemovedAttachmentIds = [];

function saveDocument(documentId, documentName, workflowId, documentTypeId) {
    return debugLambda("saveDocument", function () {
        var url = "/Documents/Save?";
        url += "documentId=" + documentId;
        url += "&documentName=" + documentName;
        url += "&workflowId=" + workflowId;
        url += "&documentTypeId=" + documentTypeId;
        url += "&newRemovedAttachmentIds=" + newRemovedAttachmentIds;
        debugAlert(url);
        $.ajax({
            type: "POST",
            url: url,
            success: function (result) {
                closeModal();
                if (result.ErrorMessage != null) {
                    alert(result.ErrorMessage);
                    return;
                }
                updateDocumentList(documentId, result);
                //            loadDocumentModal(result.Id, addDocumentDeleteClick, addDocumentSaveClick, window.addAddAttachmentClick, window.addDeleteAttachmentClick);
                setButtonActions(true);
                $("#noDocumentMessage").hide();
                $("#documentsTable").show();
            },
            error: function (xhr, status, p3, p4) {
                var err = "Error " + " " + status + " " + p3 + " " + p4;
                if (xhr.responseText && xhr.responseText[0] == "{")
                    err = JSON.parse(xhr.responseText).Message;
                console.log(err);
            },
            data: appendStalenessData({})
        });
    }, documentId, documentName, workflowId, documentTypeId);
}

function showDocumentErrMsg(msg, location) {
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
            debugLambda("deleteDocument.success", function () {
                if (result.ErrorMessage != null) {
                    alert(result.ErrorMessage);
                    return;
                }
                deleteDocumentRow(result.Id);
            });
        },
        error: function (xhr, status, p3, p4) {
            debugLambda("deleteDocument.error", function () {
                var err = "Error " + " " + status + " " + p3 + " " + p4;
                if (xhr.responseText && xhr.responseText[0] == "{")
                    err = JSON.parse(xhr.responseText).Message;
                console.log(err);
            });
        },
        data: appendStalenessData({})
    });
    return false;
}