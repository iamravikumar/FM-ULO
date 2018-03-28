var parentDocumentId;
$(document).ready(function () {

    addDeleteAttachmentClick();
    addAddAttachmentClick();
    $("#attachment-file-upload").change(function (e) {
        uploadAttachment(parentDocumentId, e.target.files);
    });
    $(".attachments-view").click(function () {
        downloadAttachment($(this).data("target"));
    })
});

function addAddAttachmentClick() {
    $(".attachments-add-btn").unbind("click");
    $(".attachments-add-btn").click(function () {
        parentDocumentId = $(this).siblings(".document-id-hidden")[0].value;
        $("#attachment-file-upload").click();
    });
}

function addRow(attachment, documentId) {
    //var dId = $(this).siblings(".document-id-hidden")[0].value;
    //alert("JBT_addRow - 1");
    //alert(attachment.AttachmentsId);
    $("#" + documentId + "Modal .attachments-heading-row").addClass("show").removeClass("hide");
    $("#" + documentId + "Modal .attachments > tbody:last-child").append(
        "<tr class='temp-attachment' id='attachment" + attachment.AttachmentsId + "'>"+
        "<td class='file-name'>" + attachment.FileName + "</td>"+
        "<td class='actions'><a class='attachments-delete' href='#' onclick='deleteAttachmentRow(" + attachment.AttachmentsId + ")' data-target='" + attachment.AttachmentsId + "'>Delete</a></td>" +
        "</tr>");
    addDeleteAttachmentClick();
}

function deleteAttachmentRow(attachId) {
    $("#attachment" + attachId).remove();
    newRemovedAttachmentIds[newRemovedAttachmentIds.length] = attachId;
    return false;
}

function addDeleteAttachmentClick() {
    $(".attachments-delete").unbind("click");
    $(".attachments-delete").click(function () {
        var attachId = $(this).data("target");
        if (attachId === 0) {
            showAttachmentErrMsg("You must save before you can delete attachments");
        } else {
            return deleteAttachment(attachId);
        }
    });
}

function deleteAttachment(attachId) {
    //alert("deleteAttachment(" + attachId + ")");
    $.ajax({
        type: "POST",
        url: "/Attachments/Delete?attachmentId=" + attachId,
        success: function (result) {
            //alert("deleteAttachment success: " + JSON.stringify(result));
            if (result.ErrorMessage != null) {
                alert(result.ErrorMessage);
                return;
            }
            deleteAttachmentRow(result.AttachmentsId);
        },
        error: function (xhr, status, p3, p4) {
            //alert("deleteAttachment fail: ");
            var err = "Error " + " " + status + " " + p3 + " " + p4;
            if (xhr.responseText && xhr.responseText[0] == "{")
                err = JSON.parse(xhr.responseText).Message;
            console.log(err);
        },
        data: appendStalenessData({})
    });
    return false;
}


function uploadAttachment(documentId, files) {
    $("[name='DocumentIdForUpload']").val(documentId);
    //$(".attachmentUploadForm").submit();
    //var myID = 3; //uncomment this to make sure the ajax URL works
    if (files.length > 0) {
        if (window.FormData !== undefined) {
            var data = new FormData();
            for (var x = 0; x < files.length; x++) {
                data.append(+x, files[x], files[x].name);
            }

            $.ajax({
                type: "POST",
                url: "/Attachments/FileUpload?documentId=" + $("[name='DocumentIdForUpload']").val(),
                contentType: false,
                processData: false,
                data: data,
                success: function (result) {
                    var errorMessage = null;
                    result.forEach(function (e) {
                        if (e.Added) {
                            addRow(e, $("[name='DocumentIdForUpload']").val());
                        }
                        else {
                            errorMessage = errorMessage == null ? "" : errorMessage + "\n\n";
                            errorMessage += e.FileName + ":\n";
                            for (i in e.ErrorMessages)
                            {
                                errorMessage += "\t" + e.ErrorMessages[i];
                            }
                        }
                    });
                    if (errorMessage != null)
                    {
                        alert(errorMessage);
                    }
                },
                error: function (xhr, status, p3, p4) {
                    var err = "Error " + " " + status + " " + p3 + " " + p4;
                    if (xhr.responseText && xhr.responseText[0] == "{")
                        err = JSON.parse(xhr.responseText).Message;
                    console.log(err);
                }
            });
        } else {
            alert("This browser doesn't support HTML5 file uploads!");
        }
    }
}

function downloadAttachment(attachmentId) {
    //$.ajax({
    //    type: "GET",
    //    url: "/Attachments/Download?attachmentId=" + attachmentId,
    //    success: function (result) {
    //        console.log(result);
    //    },
    //    error: function (xhr, status, p3, p4) {
    //        var err = "Error " + " " + status + " " + p3 + " " + p4;
    //        if (xhr.responseText && xhr.responseText[0] == "{")
    //            err = JSON.parse(xhr.responseText).Message;
    //        console.log(err);
    //    }
    //});
    return false;
}

function showAttachmentErrMsg(msg) {
    $(".document-error-msg").html(msg);
    $(".document-error-msg").show();
}

function hideErrorMsg() {
    $(".document-error-msg").hide();
}