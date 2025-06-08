//If the SDK namespace object is not defined, create it.
//SD: web-use-strict-equality-operators
if (typeof MCS === "undefined")
    MCS = {};
// Create Namespace container for functions in this library;
MCS.mcs_Resource_Buttons = {};

MCS.mcs_Resource_Buttons.openResourceCalendar = function (formContext) {
    //SD - Start
    //web-use-strict-mode
    "use strict";
    //SD - End

    //var formContext = executionContext.getFormContext();
    var relatedSystemResource = formContext.getAttribute("mcs_relatedresourceid");
    var relatedSystemResourceId;
    var relatedSystemResourceName;

    if (relatedSystemResource != null) {
        var relatedSystemResourceValue = relatedSystemResource.getValue();
        if (relatedSystemResourceValue[0] != null)
            relatedSystemResourceId = relatedSystemResourceValue[0].id;
    }

    //8-13-2020	
    //Added by Murthy for issue reported by Nilesh for INC11836273 as the original code is not working
    var entityFormOptions = {};
    entityFormOptions["entityName"] = "equipment";
    entityFormOptions["entityId"] = relatedSystemResourceId;
    entityFormOptions["openInNewWindow"] = true;

    Xrm.Navigation.openForm(entityFormOptions).then(
        function (success) {
            console.log(success);
        },
        function (error) {
            console.log(error);
        });
    //commented out for issue above
    //Xrm.Navigation.openForm("equipment", relatedSystemResourceId);
};

MCS.mcs_Resource_Buttons.UpdateResourceConnections = function (executionContext) {
    var formContext = executionContext.getFormContext();
    var updateResourceConnections = formContext.getAttribute("cvt_updateresourceconnections");
    var r = confirm("Please confirm that you would like to update this TMP Resource and all of its connections to Scheduling Packages and Resource Groups. Please Select Ok to continue or Cancel to abort.");

    //SD: web-use-strict-equality-operators
    if (r === true) {
        x = alert("All fields on this TMP Resource all now editable (Except for Site & Type). After making changes, click Save or Save & Close to initiate process of updating all connections.")
        updateResourceConnections.setValue(true);
    }
    else {
        x = "Aborted";
        updateResourceConnections.setValue(false);
    }
};

MCS.mcs_Resource_Buttons.ReplaceResource = function (primaryControl) {
    var formContext = primaryControl.getFormContext();
    if (confirm("Please confirm that you would like to replace this TMP Resource on all MTSA's, TSA's, and Resource Groups. Please Select Ok to continue or Cancel to abort."))
        MCS.cvt_Common.openDialogOnCurrentRecord(primaryControl, "26CDD7B6-7A94-47E2-BC3A-1169722E81F0");
    else
        formContext.getAttribute("cvt_replaceresourceconnections").setValue(false);
};

MCS.mcs_Resource_Buttons.DeleteResourceConnections = function (executionContext) {
    var formContext = executionContext.getFormContext();
    var deleteResourceConnections = formContext.getAttribute("cvt_deleteresourceconnections");
    var r = confirm("Please confirm that you would like to remove all connections to Scheduling Packages and Resource Groups for this TMP Resource. Please Select Ok to continue or Cancel to abort.");

    //SD: web-use-strict-equality-operators
    if (r === true) {
        x = alert("All connections to Scheduling Packages and Resource Groups for this TMP Resource are being removed. You may also choose to delete this TMP Resource by selecting the delete button in ribbon.")
        deleteResourceConnections.setValue(true);
        formContext.data.entity.save();
    }
    else {
        x = "Aborted";
        deleteResourceConnections.setValue(false);
    }
};

/* Davinder 23Apr2020: Not sure from where this is invoked. */
MCS.mcs_Resource_Buttons.openDialogProcess = function (dialogId, EntityName, objectId) {
    MCS.cvt_Common.openDialogProcess(dialogId, EntityName, objectId);
};

MCS.mcs_Resource_Buttons.runRibbonWorkflow = function (executionContext, workflowId) {
    var formContext = executionContext.getFormContext();
    MCS.GlobalFunctions.runWorkflow(formContext.data.entity.getId(), workflowId, MCS.GlobalFunctions.runWorkflowResponse);
};