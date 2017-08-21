var preventDismiss = false;
$(document).ready(function () {
    addDocumentDeleteClick();
    addDocumentSaveClick();
});

function setButtonActions(actionSwitch) {
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
}

function attachmentsPresent() {
    return $(".temp-attachment").length + $(".attachment-row").length > 0;
}

function addDocumentSaveClick() {
    $(".save-document").unbind("click");
    $(".save-document").on("click", function () {
        hideErrorMsg();
        setButtonActions(false);
        
        var documentId = $(this).data("target");
        var workflowId = currentWorkflowId;
        var documentTypeId = $("#" + documentId + "ModalDocumentType").val();
        var documentName = $("#" + documentId + "ModalDocumentName").val();
        if (documentTypeId === "") {
            showErrMsg("You must select a Document Type before saving", $(this));
            setButtonActions(true);
        } else if (documentName === "") {
            showErrMsg("You must enter a Document Name before saving", $(this));
            setButtonActions(true)   
        }
        else if (!attachmentsPresent()) {
            showErrMsg("You must add attachments before saving", $(this));
            setButtonActions(true)
        }
        else {
            saveDocument(documentId, documentName, workflowId, documentTypeId);  
        }

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
    //alert("JBT_updateDocumentList - 1");
    $(".documents-heading-row").addClass("show").removeClass("hide");
    var docTypeNames = "";
    for (var x = 0; x < document.DocumentTypeNames.length; ++x)
    {
        docTypeNames += "<li>" + document.DocumentTypeNames[x]+ "</li>";
    }
    var tableRowString =
        "<tr id='document" + document.Id + "'><td>" +
        document.Name + "</td><td>" +
        "<ul>" + docTypeNames + "</ul></td><td>" +
        document.AttachmentCount + "</td><td>" +
        document.UserName + "</td><td>" +
        document.UploadedDate + "</td><td><a data-target='' data-toggle='modal' href='#" + document.Id + "Modal'>View</a> | <a data-toggle='modal' data-target='#" + document.Id + "ModalDelete' href='#" + document.Id + "ModalDelete'>Delete</a></td></tr>"
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
    debugAlert(url);
    $.ajax({
        type: "POST",
        url: url,
        success: function (result) {
            closeModal();
            updateDocumentList(documentId, result);
//            loadDocumentModal(result.Id, addDocumentDeleteClick, addDocumentSaveClick, window.addAddAttachmentClick, window.addDeleteAttachmentClick);
            setButtonActions(true);
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