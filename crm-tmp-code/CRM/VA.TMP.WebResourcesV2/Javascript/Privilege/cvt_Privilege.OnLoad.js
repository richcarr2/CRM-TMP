 //Library Name: cvt_Privilege.OnLoad.js
//If the SDK namespace object is not defined, create it.
if (typeof(MCS) === "undefined") {
    MCS = {};
}
// Create Namespace container for functions in this library;
MCS.Privilege_OnLoad = {};

//Onload - Allow first time entry and name built
MCS.Privilege_OnLoad.OnLoad = function (executionContext) {
    "use strict";
    var formContext = executionContext.getFormContext();
    if (formContext.ui.getFormType() === 1) {
        formContext.getAttribute("cvt_name").setSubmitMode("always");
        //Check if fields are mapped, then it is Proxy; don't clear out Referenced TSS Priv
        if (formContext.getAttribute('cvt_referencedprivilegeid').getValue() !== null) {
            formContext.getAttribute('cvt_typeofprivileging').setValue(917290001);
        }
        else {
            formContext.getAttribute('cvt_typeofprivileging').setValue(917290000);
        }
        formContext.getControl("cvt_typeofprivileging").setDisabled(true);
        formContext.getAttribute("cvt_typeofprivileging").setSubmitMode("always");

        formContext.getControl('statuscode').setDisabled(true);
        formContext.getAttribute("statuscode").setSubmitMode("always");
        formContext.getControl("cvt_servicetypeid").setDisabled(false);
        formContext.getAttribute("cvt_servicetypeid").setSubmitMode("always");

    }
    else {
        MCS.Privilege_OnLoad.CheckUser(executionContext);
        var statusReason = (formContext.getAttribute('statuscode').getValue() !== null) ? formContext.getAttribute('statuscode').getSelectedOption().value : null;
        if (statusReason === 917290001) { //Privileged
            //Make entire form readonly.
            formContext.getControl("cvt_providerid").setDisabled(true);
            formContext.getControl("cvt_privilegedatid").setDisabled(true);
            formContext.getControl("cvt_typeofprivileging").setDisabled(true);
            formContext.getControl("cvt_servicetypeid").setDisabled(true);
            formContext.getControl("cvt_servicesubtypeid").setDisabled(true);
            formContext.getControl("ownerid").setDisabled(true);
            formContext.getControl("cvt_referencedprivilegeid").setDisabled(true);
        }
    }
};

//If User is disabled, display notification
MCS.Privilege_OnLoad.CheckUser = function (executionContext) {
    var formContext = executionContext.getFormContext();

    if (formContext.getAttribute('cvt_providerid').getValue() !== null) provLookup = formContext.getAttribute('cvt_providerid').getValue()[0].id;
    else return;

    var calls = CrmRestKit.Retrieve("SystemUser", provLookup, ['IsDisabled'], false);
    calls.done(function (data) {
        if (data && data.d) {
            if (data.d.IsDisabled === 1) {
                var notificationsList = Sys.Application.findComponent('crmNotifications');

                if (notificationsList) {
                    notificationsList.AddNotification('noteId1', 2, 'namespace', 'Provider record is currently deactivated.');
                }
            }
        }
    }).fail(
    function (error) {});
};