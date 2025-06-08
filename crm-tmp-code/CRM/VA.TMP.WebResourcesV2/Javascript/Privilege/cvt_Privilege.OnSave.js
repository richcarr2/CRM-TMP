//Library Name: cvt_Privilege.OnSave.js
//If the SDK namespace object is not defined, create it.
if (typeof(MCS) === "undefined") {
    MCS = {};
}
// Create Namespace container for functions in this library;
MCS.Privilege_OnSave = {};

MCS.Privilege_OnSave.CreateName = function (executionContext) {
    "use strict";
    var formContext = executionContext.getFormContext();
    var providerName = formContext.getAttribute("cvt_providerid").getValue();
    var facilityName = formContext.getAttribute("cvt_privilegedatid").getValue();
    var typeOfPrivileging = formContext.getAttribute("cvt_typeofprivileging");
    var serviceType = formContext.getAttribute("cvt_servicetypeid").getValue();

    var nameField = (providerName !== null) ? providerName[0].name : "";
    nameField += (serviceType !== null) ? " - " + serviceType[0].name : "";
    nameField += (facilityName !== null) ? " @ " + facilityName[0].name : "";
    nameField += (typeOfPrivileging !== null && typeOfPrivileging.getSelectedOption() !== null) ? " (" + typeOfPrivileging.getSelectedOption().text + ")" : "";

    if (formContext.getAttribute("cvt_name").getValue() !== nameField) {
        formContext.getAttribute("cvt_name").setSubmitMode("always");
        formContext.getAttribute("cvt_name").setValue(nameField);
    }
};