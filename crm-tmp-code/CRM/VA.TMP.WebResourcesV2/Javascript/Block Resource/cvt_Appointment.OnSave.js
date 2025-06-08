//If the MCS namespace object is not defined, create it.
//SD: web-use-strict-equality-operators
if (typeof(MCS) === "undefined") {
    MCS = {};
}
// Create Namespace container for functions in this library;
MCS.AppointmentOnSave = {};

MCS.AppointmentOnSave.CheckForActiveVistaLogin = function (executionContext, eventObj) {
    //SD - Start
    //web-use-strict-mode
    "use strict";
    //SD - End
    var formContext = executionContext.getFormContext();
    var vets = formContext.getAttribute("optionalattendees");
    if (!vets.getIsDirty()) return;
    var validDuz = MCS.VIALogin.IsValidUserDuz();
    if (validDuz) return;
    var validToken = MCS.VIALogin.IsValidSamlToken();
    if (validToken) {
        MCS.VIALogin.Login();
        alert("Unable to save changes to Vista until you have logged into Vista.");
        eventObj.getEventArgs().preventDefault();
    }
    else {
        MCS.VIALogin.Saml();
        alert("Unable to save changes to Vista until you have logged into Vista.");
        eventObj.getEventArgs().preventDefault();
    }
};