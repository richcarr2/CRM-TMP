//If the SDK namespace object is not defined, create it.
if (typeof (MCS) == "undefined")
{ MCS = {}; }
// Create Namespace container for functions in this library;
MCS.cvt_MTSA_OnChange = {};

//VA Video Connect
MCS.cvt_MTSA_OnChange.Type = function (executionContext) {
    var formContext = executionContext.getFormContext();
    var Type = (formContext.getAttribute("cvt_type").getValue()) ? formContext.getAttribute("cvt_type").getValue() : false;

    if (Type == false) { //Clinic Based
        if (formContext.ui.getFormType() == MCS.cvt_Common.FORM_TYPE_CREATE) {
            //formContext.getControl("cvt_capacity").setDisabled(false);
            formContext.getControl("cvt_availabletelehealthmodalities").setDisabled(false);
        }
    }
    else { //true //CVT to Home
        if (formContext.ui.getFormType() == MCS.cvt_Common.FORM_TYPE_CREATE) {
            formContext.getAttribute("cvt_availabletelehealthmodalities").setValue(917290000);
            formContext.getAttribute("cvt_availabletelehealthmodalities").setSubmitMode("always");
            formContext.getAttribute("cvt_availabletelehealthmodalities").fireOnChange(); //Make sure the form displays the correct fields
        }
        
        formContext.getControl("cvt_groupappointment").setVisible(true);
        formContext.getControl("cvt_groupappointment").setDisabled(false);
        formContext.getControl("cvt_availabletelehealthmodalities").setDisabled(true);
    }
};
//Store Forward
MCS.cvt_MTSA_OnChange.StoreForward = function (executionContext) {
    var formContext = executionContext.getFormContext();
    //Check if this TSA is store forward
    var SFT = formContext.getAttribute("cvt_availabletelehealthmodalities").getValue() == 917290001;
    formContext.getControl("cvt_groupappointment").setVisible(!SFT);
    if (SFT) {
        if (formContext.getAttribute("cvt_servicelevels").getValue() == null)
            formContext.getAttribute("cvt_servicelevels").setValue(917290000);
    }
};