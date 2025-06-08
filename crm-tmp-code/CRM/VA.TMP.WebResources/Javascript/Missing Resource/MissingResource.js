//If the SDK namespace object is not defined, create it.
if (typeof (MCS) == "undefined") { MCS = {}; }
// Create Namespace container for functions in this library;
MCS.MissingResource = {};

//Lookup fields should be hidden on load or change depending on the mcs_type value
MCS.MissingResource.HideLookupFields = function (executionContext) {
    var formContext = executionContext.getFormContext();
    var resourceType = formContext.getAttribute("mcs_type").getValue();
    switch (resourceType) {
        case 803750000:
            Xrm.Page.getControl("mcs_tmpresource").setVisible(true);
            Xrm.Page.getControl("mcs_user").setVisible(false);
            Xrm.Page.getControl("mcs_patient").setVisible(false);
            break;
        case 803750001:
            Xrm.Page.getControl("mcs_tmpresource").setVisible(false);
            Xrm.Page.getControl("mcs_user").setVisible(true);
            Xrm.Page.getControl("mcs_patient").setVisible(false);
            break;
        case 803750002:
            Xrm.Page.getControl("mcs_tmpresource").setVisible(false);
            Xrm.Page.getControl("mcs_user").setVisible(false);
            Xrm.Page.getControl("mcs_patient").setVisible(true);
            break;
        default:
            Xrm.Page.getControl("mcs_tmpresource").setVisible(false);
            Xrm.Page.getControl("mcs_user").setVisible(false);
            Xrm.Page.getControl("mcs_patient").setVisible(false);
    }
};

//Lookup fields should be cleared on save depending on the mcs_type value
MCS.MissingResource.ClearLookupFields = function (executionContext) {
    var formContext = executionContext.getFormContext();
    var resourceType = formContext.getAttribute("mcs_type").getValue();
    switch (resourceType) {
        case 803750000:
            formContext.getAttribute("mcs_user").setValue(null);
            formContext.getAttribute("mcs_patient").setValue(null);
            break;
        case 803750001:
            formContext.getAttribute("mcs_tmpresource").setValue(null);
            formContext.getAttribute("mcs_patient").setValue(null);
            break;
        case 803750002:
            formContext.getAttribute("mcs_tmpresource").setValue(null);
            formContext.getAttribute("mcs_user").setValue(null);
            break;
        default:
            formContext.getAttribute("mcs_tmpresource").setValue(null);
            formContext.getAttribute("mcs_user").setValue(null);
            formContext.getAttribute("mcs_patient").setValue(null);
    }
};