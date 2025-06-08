if (typeof (MCS) === "undefined") { MCS = {}; }

MCS.Patients = [];
// Create Namespace container for functions in this library;
MCS.AppointmentOnLoad = {};
//window.top.MCS = MCS;

MCS.AppointmentOnLoad.loadPatients = function (executionContext) {
    var formContext = executionContext.getFormContext();
    var patientObj = formContext.getAttribute("optionalattendees");
    var patients = patientObj != null ? patientObj.getValue() : [];
    MCS.Patients = patients;
}

MCS.AppointmentOnLoad.ClearOrganizer = function (executionContext) {
    var formContext = executionContext.getFormContext();
    formContext.getAttribute("organizer").setValue();
};

MCS.AppointmentOnLoad.DisplayGroupForm = function (executionContext) {
    var formContext = executionContext.getFormContext();
    var isGroup;
    var groupObj = formContext.getAttribute("cvt_serviceactivityid");
    if (groupObj != null)
        isGroup = formContext.getAttribute("cvt_serviceactivityid").getValue() != null;
    //if this is the child record of a service activity record, then hide away a number of the normal fields
    formContext.ui.tabs.get('appointment').sections.get('scheduling information').setVisible(!isGroup);
    formContext.ui.tabs.get('appointment').sections.get('appointment description').setVisible(!isGroup);
    formContext.ui.tabs.get('appointment').sections.get('GroupFields').setVisible(isGroup);
};

MCS.AppointmentOnLoad.showMVI = function (executionContext) {
    var formContext = executionContext.getFormContext();
    debugger;
    var showMVI = MCS.cvt_Common.MVIConfig() && !MCS.cvt_Common.AppointmentOccursInPast(executionContext);
    formContext.ui.tabs.get('PersonSearch').setVisible(showMVI);
    formContext.getControl('optionalattendees').setVisible(showMVI);
    if (showMVI)
        formContext.ui.tabs.get('PersonSearch').setFocus();
}

MCS.AppointmentOnLoad.ShowHideCancelRemarks = function (executionContext) {
    var formContext = executionContext.getFormContext();
    var _state = formContext.getAttribute("statecode").getValue();
    var _cancelComments = formContext.getAttribute("cvt_cancelremarks").getValue();

    //alert("State= " +_state+ "\nCancelRemarks = " + _cancelComments);

    if (_cancelComments != null) {
        formContext.getControl("cvt_cancelremarks").setVisible(true);
    }
    else {
        formContext.getControl("cvt_cancelremarks").setVisible(false);
    }
}



MCS.AppointmentOnLoad.SetDefaultReserverResource = function (executionContext) {
    var formContext = executionContext.getFormContext();
    //requiredattendees

    if (formContext.ui.getFormType() == 1) {
        var userSettings = Xrm.Utility.getGlobalContext().userSettings;

        var lookupValue = new Array();
        lookupValue[0] = new Object();
        lookupValue[0].id = userSettings.userId; // GUID of the lookup id
        lookupValue[0].name = userSettings.userName; // Name of the lookup
        lookupValue[0].entityType = "systemuser"; //Entity Type of the lookup entity
        formContext.getAttribute("requiredattendees").setValue(lookupValue); // You need to replace the lookup field 
    }

}