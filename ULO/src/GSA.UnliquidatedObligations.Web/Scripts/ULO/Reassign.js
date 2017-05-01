
$('#requestForReassignModal').on('show.bs.modal', function (event) {

    $.get("/RequestForReassignments/Create",
        function(data) {
            console.log(data);
        });

})