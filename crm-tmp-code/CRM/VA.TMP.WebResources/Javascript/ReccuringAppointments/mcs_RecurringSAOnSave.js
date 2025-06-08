//If the SDK namespace object is not defined, create it.
if (typeof (MCS) == "undefined")
{ MCS = {}; }
// Create Namespace container for functions in this library;
MCS.mcs_RecurringAppts_OnSave = {};

MCS.mcs_RecurringAppts_OnSave.FORM_TYPE_CREATE = 1;
MCS.mcs_RecurringAppts_OnSave.FORM_TYPE_UPDATE = 2;
MCS.mcs_RecurringAppts_OnSave.FORM_TYPE_READ_ONLY = 3;
MCS.mcs_RecurringAppts_OnSave.FORM_TYPE_DISABLED = 4;

MCS.mcs_RecurringAppts_OnSave.CheckDirty = function (executionContext,executionObj) {
    var formContext = executionContext.getFormContext();
    if (formContext.data.entity.getIsDirty() == false) {
        parent.formContext.data.entity.save();
    }
    else {
        
        var r = confirm("One or more resources tied to this Recurring Service Activity series are not available at the following times.");
        if (r == true) {
            x = "Save Confirmed";
            parent.formContext.data.entity.save();
        }
        else {
            x = executionObj.getEventArgs().preventDefault();
           // Xrm.Page.getAttribute("statuscode").setValue(1);
        }
    }
}