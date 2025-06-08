//If the SDK namespace object is not defined, create it.
if (typeof (MCS) === "undefined") { MCS = {}; }
// Create Namespace container for functions in this library;
MCS.mcs_ResourceGroup = {};

MCS.mcs_ResourceGroup.CreateName = function (executionContext) {
    'use strict';
    var formContext = executionContext.getFormContext();
    var mcs_type = formContext.getAttribute("mcs_type").getValue();
    var mcs_usernameinput = formContext.getAttribute("mcs_usernameinput").getValue();
    var mcs_relatedsiteid = formContext.getAttribute("mcs_relatedsiteid").getValue();

    var derivedResultField = "";

    if (mcs_usernameinput != null)
        derivedResultField += mcs_usernameinput + " : ";

    if (mcs_type != null) {
        derivedResultField += formContext.getAttribute("mcs_type").getText();

        if (mcs_type !== 917290000 && mcs_type !== 251920002)
            derivedResultField += "s";
    }

    derivedResultField += " @ ";

    if (mcs_relatedsiteid != null) {
        derivedResultField += mcs_relatedsiteid[0].name;
    }

    formContext.getAttribute("mcs_name").setSubmitMode("always");
    formContext.getAttribute("mcs_name").setValue(derivedResultField);
};

MCS.mcs_ResourceGroup.MakeFieldsReadOnly = function (executionContext) {
    var formContext = executionContext.getFormContext();
    var Formtype = formContext.ui.getFormType();

    if (Formtype !== 1) {
        formContext.ui.controls.get("mcs_relatedsiteid").setDisabled(true);
        formContext.ui.controls.get("mcs_type").setDisabled(true);
    }
};
