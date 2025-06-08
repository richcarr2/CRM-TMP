//If the SDK namespace object is not defined, create it.
if (typeof (MCS) == "undefined")
    MCS = {};
// Create Namespace container for functions in this library;
if (typeof (MCS.mcs_Service_Activity_OnSave) == "undefined")
    MCS.mcs_Service_Activity_OnSave = {};

MCS.mcs_Service_Activity_OnSave.SetScheduled = function (executionContext) {
    var formContext = executionContext.getFormContext();
    var resources = formContext.getAttribute("resources").getValue();
    if (resources != null)
        formContext.getAttribute("cvt_scheduled").setValue(1);
};
