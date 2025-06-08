//If the MCS namespace object is not defined, create it.
//SD: web-use-strict-equality-operators
if (typeof MCS === "undefined")
    MCS = {};
// Create Namespace container for functions in this library;
MCS.User_RibbonFunctions = {};

MCS.User_RibbonFunctions.FORM_TYPE_CREATE = 1;
MCS.User_RibbonFunctions.FORM_TYPE_UPDATE = 2;
MCS.User_RibbonFunctions.FORM_TYPE_READ_ONLY = 3;
MCS.User_RibbonFunctions.FORM_TYPE_DISABLED = 4;

MCS.User_RibbonFunctions.DeleteResourceConnections = function () {
    //SD - Start
    //web-use-strict-mode
    "use strict";
    //SD - End
    //SD web-use-client-context
    //'Xrm.Page' references or accesses a deprecated API in the client context object model. Replace this call with the following client context API: 'ExecutionContext.getFormContext'
    var deleteUserConnections = ExecutionContext.getFormContext().getAttribute("cvt_deleteuserconnections");
    //var deleteUserConnections = Xrm.Page.getAttribute("cvt_deleteuserconnections");
    var r = confirm("Please confirm that you would like to remove all connections to MTSA's, TSA's, and Resource Groups for this TMP User. Please Select Ok to continue or Cancel to abort.");
    //SD: web-use-strict-equality-operators
    if (r === true) {
        x = alert("All connections to MTSA's, TSA's, and Resource Groups for this TMP User are being removed. You may also choose to delete this TSS Resource by selecting the delete button in ribbon.")
        deleteUserConnections.setValue(true);
        //SD web-use-client-context
        //'Xrm.Page' references or accesses a deprecated API in the client context object model. Replace this call with the following client context API: 'ExecutionContext.getFormContext'
        ExecutionContext.getFormContext().data.entity.save();
        //Xrm.Page.data.entity.save();
    }
    else {
        x = "Aborted";
        deleteResourceConnections.setValue(false);
        //SD web-use-client-context
        //'Xrm.Page' references or accesses a deprecated API in the client context object model. Replace this call with the following client context API: 'ExecutionContext.getFormContext'
        ExecutionContext.getFormContext().data.entity.save();
        //Xrm.Page.data.entity.save();
    }
};

MCS.User_RibbonFunctions.ReplaceResource = function () {
    //SD - Start web-use-client-context
    //'Xrm.Page' references or accesses a deprecated API in the client context object model. Replace this call with the following client context API: 'ExecutionContext.getFormContext'
    var updateUserConnections = ExecutionContext.getFormContext().getAttribute("cvt_updateuserconnections");
    var EntityName = ExecutionContext.getFormContext().data.entity.getEntityName();
    var EntityId = ExecutionContext.getFormContext().data.entity.getId();
    //var updateUserConnections = Xrm.Page.getAttribute("cvt_updateuserconnections");
    //var EntityName = Xrm.Page.data.entity.getEntityName();
    //var EntityId = Xrm.Page.data.entity.getId();
    //SD - End

    var r = confirm("Please confirm that you would like to replace this TMP User on all MTSA's, TSA's, and Resource Groups. Please Select Ok to continue or Cancel to abort.");
    //SD: web-use-strict-equality-operators
    if (r === true) {
        x = MCS.User_RibbonFunctions.openDialogProcess("F18CA404-A532-4DA9-941F-866F2C9FA388", EntityName, EntityId);
    }
    else {
        x = "Aborted";
        updateUserConnections.setValue(false);
        //SD web-use-client-context
        //'Xrm.Page' references or accesses a deprecated API in the client context object model. Replace this call with the following client context API: 'ExecutionContext.getFormContext'
        ExecutionContext.getFormContext().data.entity.save();
        //Xrm.Page.data.entity.save();
    }
};

MCS.User_RibbonFunctions.openDialogProcess = function (dialogId, EntityName, objectId) {
    //SD - Start
    //web-use-client-context
    var url = Xrm.Utility.getGlobalContext().getClientUrl() +
        "/cs/dialog/rundialog.aspx?DialogId=" +
        dialogId + "&EntityName=" +
        EntityName + "&ObjectId=" +
        objectId;
    //SD - End
    //var url = Xrm.Page.context.getClientUrl() +
    //    "/cs/dialog/rundialog.aspx?DialogId=" +
    //    dialogId + "&EntityName=" +
    //    EntityName + "&ObjectId=" +
    //    objectId;
    var width = 400;
    var height = 400;
    var left = (screen.width - width) / 2;
    var top = (screen.height - height) / 2;
    window.open(url, '', 'location=0,menubar=1,resizable=0,width=' + width + ',height=' + height + ',top=' + top + ',left=' + left + '');
};

MCS.User_RibbonFunctions.openLegacy = function (formContext) {
    var url = window.top.location.href;
    if (url.indexOf("forceclassic") > -1) {
        alert("You're already in the legacy interface.");
        return;
    }
    var url = Xrm.Utility.getGlobalContext().getClientUrl() + "/main.aspx?forceclassic=1&newWindow=true&pagetype=entityrecord&etn=systemuser&id=" + formContext.data.entity.getId();
    //https://{domain}/main.aspx?forceclassic=1&newWindow=true&pagetype=entityrecord&etn=systemuser&id={guid} 
    window.open(url);
};

MCS.User_RibbonFunctions.openLegacyEnableRule = function () {
    var url = window.top.location.href;
    if (url.indexOf("forceclassic") > -1)
        return false;
    return true;
};

MCS.User_RibbonFunctions.openUCI = function () {
    try {
        window.top.opener.focus();
        window.top.opener.close();
    } catch (e) { }
};

MCS.User_RibbonFunctions.RoleCheck = function () {

    var allowedRoles = ["TMP Field Application Administrator", "System Administrator", "TMP Application Administrator"];
    var hasRole = false;

    var userSettings = Xrm.Utility.getGlobalContext().userSettings;
    userSettings.roles.get().forEach(function (item) {
        if (allowedRoles.includes(item.name)) {
            hasRole = true;
        }
    });
    return !hasRole;
};