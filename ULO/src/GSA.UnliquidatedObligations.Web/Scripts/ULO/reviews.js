$(document).ready(function () {
    showHideRegion();
    setWorkflow();
    $("#ReviewScopeId").change(function () {
        showHideRegion();
        setWorkflow();
    });
});

function showHideRegion() {
    if ($("#ReviewScopeId").val() == "1")
        $("#reviewRegionContainer").show();
    else
        $("#reviewRegionContainer").hide();
}

function setWorkflow() {
    if ($("#ReviewScopeId").val() == 1) {
        $("#WorkflowDefinitionId").val(3)
        $("#workflowDefintionName").val("Region Workflow");
    }
    else if ($("#ReviewScopeId").val() == 2) {
        $("#WorkflowDefinitionId").val(2)
        $("#workflowDefintionName").val("Ulo Workflow");
    }

}
