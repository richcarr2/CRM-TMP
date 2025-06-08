//Library Name: cvt_Privilege.OnChange.js
//If the SDK namespace object is not defined, create it.
if (typeof(MCS) === "undefined") {
    MCS = {};
}
// Create Namespace container for functions in this library;
MCS.Privilege_OnChange = {};

//Toggle sections based on TypeofPrivileging
MCS.Privilege_OnChange.TypeToggle = function (executionContext) {
    "use strict";
    var formContext = executionContext.getFormContext();

    var typeOfPrivileging = (formContext.getAttribute('cvt_typeofprivileging').getValue() !== null) ? formContext.getAttribute('cvt_typeofprivileging').getSelectedOption().value : null;
    if (typeOfPrivileging !== null) {
        if (typeOfPrivileging === 917290000) { //Primary
            formContext.ui.tabs.get('tabGeneral').sections.get('cvt_home').setVisible(true);
            formContext.ui.tabs.get('tabGeneral').sections.get('cvt_home2').setVisible(true);
            formContext.ui.tabs.get('tabGeneral').sections.get('cvt_proxy').setVisible(false);
            formContext.getAttribute("cvt_notifydate").setRequiredLevel('required');

            //Clear Proxy fields
            formContext.getAttribute("cvt_referencedprivilegeid").setValue(null);
            formContext.getAttribute("cvt_referencedprivilegeid").setRequiredLevel('none');
        }
        else if (typeOfPrivileging === 917290001) { //Secondary
            //Disable
            formContext.getControl("cvt_servicetypeid").setDisabled(true);
            formContext.getControl("cvt_servicesubtypeid").setDisabled(true);
            formContext.getControl("cvt_typeofprivileging").setDisabled(true);
            formContext.getControl('statuscode').setDisabled(true);
            formContext.getControl("cvt_referencedprivilegeid").setDisabled(true);
            formContext.getControl("cvt_providerid").setDisabled(true);

            formContext.ui.tabs.get('tabGeneral').sections.get('cvt_home').setVisible(false);
            formContext.ui.tabs.get('tabGeneral').sections.get('cvt_home2').setVisible(false);
            formContext.ui.tabs.get('tabGeneral').sections.get('cvt_proxy').setVisible(true);
            //Clear Home fields
            formContext.getAttribute("cvt_expirationdate").setValue(null);
            formContext.getAttribute("cvt_expirationdate").setRequiredLevel('none');
            formContext.getAttribute("cvt_notifydate").setValue(null);
            formContext.getAttribute("cvt_notifydate").setRequiredLevel('none');

            formContext.getAttribute("cvt_referencedprivilegeid").setRequiredLevel("required");

        }
    }
    else {
        formContext.ui.tabs.get('tabGeneral').sections.get('cvt_home').setVisible(false);
        formContext.ui.tabs.get('tabGeneral').sections.get('cvt_home2').setVisible(false);
        formContext.ui.tabs.get('tabGeneral').sections.get('cvt_proxy').setVisible(false);
        //Clear both Home and Proxy fields
        formContext.getAttribute("cvt_referencedprivilegeid").setValue(null);
        formContext.getAttribute("cvt_expirationdate").setValue(null);
        formContext.getAttribute("cvt_referencedprivilegeid").setRequiredLevel('none');

        formContext.getAttribute("cvt_notifydate").setValue(null);
        formContext.getAttribute("cvt_notifydate").setRequiredLevel('none');
    }
};