
$(document).ready(function () {
    $("#RegionId").change(function () {
        UsersRegionChanged(this.value);
    });
   
    
    addEditClick();
    addCreateClick();
});

function addSaveClick() {
    $(".save-user").unbind("click");
    $(".save-user").click(function () {
        $(this).prop("disabled", true);
        $(".close-save-user").prop("disabled", true);
        $(this).text("Saving...");
        var data = $("form[name=editUserForm]").serializeArray();

        var postData = new FormPostData(data, $("#RegionId").val());
        //console.log(postData);
        $.ajax({
            type: "POST",
            url: "/Users/Edit",
            data: JSON.stringify(postData),
            success: function (result) {
                $(".users-data").html(result);
                addEditClick();
                $("#editUserModal").modal("hide");
                $(this).prop("disabled", false);
                $(".close-save-user").prop("disabled", true);
                $(this).text("Save");
            },
            error: function (xhr, status, p3, p4) {
                var err = "Error " + " " + status + " " + p3 + " " + p4;
                if (xhr.responseText && xhr.responseText[0] == "{")
                    err = JSON.parse(xhr.responseText).Message;
                console.log(err);
            }
        });
    });
}

function addDeleteSubjectCategoryDeleteClick() {
    $(".delete-action").unbind("click");
    $(".delete-action").click(function () {
        var $element = $(this).parent().parent();
        $($element).remove();
        return false;
    });
}

function CreateUserFormData(data, regionId) {
    this.UserName = data.filter(function (a) {
        return a.name === "UserName";
    })[0].value;

    this.UserEmail = data.filter(function (a) {
        return a.name === "UserEmail";
    })[0].value;


    this.ApplicationPermissionNames = data
    .filter(function (a) {
        return a.name === "ApplicationPermission";
    })
    .map(function (a) {
        return a.value;
    });
    this.GroupIds = data
        .filter(function (a) {
            return a.name === "Groups";
        })
        .map(function (a) {
            return a.value;
        });

    this.SubjectCategoryClaims = getSubjectCategoryClaims(data);

    this.RegionId = regionId;
}

function FormPostData(data, regionId) {
    this.ApplicationPermissionNames = data
        .filter(function (a) {
            return a.name === "ApplicationPermission";
        })
        .map(function (a) {
            return a.value;
        });
    this.GroupIds = data
        .filter(function (a) {
            return a.name === "Groups";
        })
        .map(function (a) {
            return a.value;
        });

    this.SubjectCategoryClaims = getSubjectCategoryClaims(data); 
    this.UserId = data.filter(function(a) {
        return a.name === "UserId";
    })[0].value;

    this.RegionId = regionId;
}

function getSubjectCategoryClaims(data) {
    var docTypeArray = [], baCodeArray = [], orgCodeArray = [], subjectCategories = [];

    docTypeArray = data.filter(function (d) {
        return d.name === "DocType";
    });

    baCodeArray = data.filter(function(d) {
        return d.name === "BACode";
    });

    orgCodeArray = data.filter(function (d) {
        return d.name === "OrgCode";
    });

    for (var i = 0; i < docTypeArray.length; i++) {
        if (docTypeArray[i] !== "" && baCodeArray[i] !== "" && orgCodeArray[i] !== "") {
            subjectCategories.push({
                DocType: docTypeArray[i].value,
                BACode: baCodeArray[i].value,
                OrgCode: orgCodeArray[i].value
            });
        }
    }
    return subjectCategories;

}

function addAddSubjectCategoryClick() {
    $(".add-subject-category").click(function () {
        $.ajax({
            type: "Get",
            url: "/Users/AddSubjectCategoryRow",
            success: function (result) {
                $(".subject-category-claims").append(result);
                addDeleteSubjectCategoryDeleteClick();
            },
            error: function (xhr, status, p3, p4) {
                var err = "Error " + " " + status + " " + p3 + " " + p4;
                if (xhr.responseText && xhr.responseText[0] == "{")
                    err = JSON.parse(xhr.responseText).Message;
                console.log(err);
            }
        });
    });
}

function addCreateClick() {
    $(".create-user").click(function () {
        var url = "/Users/Create";
        //url += "&regionId=" + encodeURI($("#RegionId").val());
        loading(true)
        $("#createUserModal").modal("show");

        $("#createUserModalBody").load(url, function () {
            addAddSubjectCategoryClick();
            addDeleteSubjectCategoryDeleteClick();
            loading(false);
        });
        return false;
    });
}

function loading(isLoading) {
    if (isLoading) {
        $("#loadingUser").show();
        $("#userBodyData").hide();
    } else {
        $("#loadingUser").hide();
        $("#userBodyData").show();
    }
}

function addEditClick() {
    $(".edit-user").click(function () {
        var userId = $(this).attr("data-id");
        var url = "/Users/Edit?";
        url += "userId=" + encodeURI(userId);
        url += "&regionId=" + encodeURI($("#RegionId").val());
        loading(true)
        $("#editUserModal").modal("show");

        $("#editUserModalOuterContainer").load(url, function () {
            addAddSubjectCategoryClick();
            addDeleteSubjectCategoryDeleteClick();
            addSaveClick();
           
            loading(false);
        });
        return false;
    });
}

function UsersRegionChanged(regionId) {
    var url = "/Users/Search?regionId=" + regionId;
    $(".users-data").load(url, function () {
        addEditClick();
    });
}