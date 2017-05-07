$(document).ready(function () {

    var documentId;

    addDeleteClick();

    $(".attachments-add-btn").click(function () {
        documentId = $(this).siblings(".document-id-hidden")[0].value;
        $("#attachment-file-upload").click();
    });

    $("#attachment-file-upload").change(function (e) {
        uploadAttachment(documentId, e.target.files);
    });
});

function addRow(attachment) {
    $(".attachments > tbody:last-child").append("<tr class='temp-attachment' id='attachment" + attachment.AttachmentsId + "'><td class='file-name'>" + attachment.FileName + "</td><td class='actions'><a class='attachments-view' href='" + attachment.FilePath +"' download>View</a> | <a class='attachments-delete' data-target='" + attachment.AttachmentsId + "'>Delete</a></td></tr>");
    addDeleteClick();
}



function addDeleteClick() {
    $(".attachments-delete").click(function () {
        var attachId = $(this).data("target");
        return deleteAttachment(attachId);
    });
}

function deleteAttachment(attachId) {
    $.ajax({
        type: "POST",
        url: "/Attachments/Delete?attachmentId=" + attachId,
        success: function (result) {
            deleteRow(result.AttachmentsId);
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



function updateTempAddresses() {
    
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
                    result.forEach(function (e) {
                        addRow(e);
                    });
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
    $.ajax({
        type: "GET",
        url: "/Attachments/Download?attachmentId=" + attachmentId,
        //success: function (result) {

        //},
        error: function (xhr, status, p3, p4) {
            var err = "Error " + " " + status + " " + p3 + " " + p4;
            if (xhr.responseText && xhr.responseText[0] == "{")
                err = JSON.parse(xhr.responseText).Message;
            console.log(err);
        }
    });
}

