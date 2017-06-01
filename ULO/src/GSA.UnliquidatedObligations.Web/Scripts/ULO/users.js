
$(document).ready(function () {
    $("#RegionId").change(function () {
        UsersRegionChanged(this.value);
    });

    addEditClick();
});

function addAddSubjectCategoryClick() {
    $(".add-subject-category").click(function () {
        $.ajax({
            type: "Get",
            url: "/Users/AddSubjectCategoryRow",
            success: function (result) {
                $(".subject-category-claims").append(result);

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

function addEditClick() {
    $(".edit-user").click(function () {
        var userId = $(this).attr("data-id");
        var url = "/Users/Edit?";
        url += "userId=" + encodeURI(userId);
        url += "&regionId=" + encodeURI($("#RegionId").val());
        $("#editUserModalBody").load(url, function () {
            addAddSubjectCategoryClick();
            $("#editUserModal").modal("show");
            

        });
        return false;
    });
}

function UsersRegionChanged(regionId) {
    var url = "/Users/Search?regionId=" + regionId;
    $(".users-data").load(url, function() {
        addEditClick();
    });
}