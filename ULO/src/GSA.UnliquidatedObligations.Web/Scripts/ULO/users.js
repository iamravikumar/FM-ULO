
$(document).ready(function () {
    $("#RegionId").change(function () {
        UsersRegionChanged(this.value);
    });

    $(".edit-user").click(function() {
        var userId = $(this).attr("data-id");
        console.log(userId);
        var url = "/Users/Edit?";
        url += "userId=" + encodeURI(userId);
        url += "&regionId=" + encodeURI($("#RegionId").val());
        $("#editUserModalBody").load(url, function() {
            $("#editUserModal").modal("show");
            
        });
        return false;
    });
});


function UsersRegionChanged(regionId) {
    var url = "/Users/Search?regionId=" + regionId;
    $(".users-data").load(url);
}