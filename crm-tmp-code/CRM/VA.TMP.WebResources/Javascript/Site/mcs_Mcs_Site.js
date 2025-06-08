//If the SDK namespace object is not defined, create it.
//SD: web-use-strict-equality-operators
if (typeof (MCS) === "undefined") { MCS = {}; }
// Create Namespace container for functions in this library;
MCS.mcs_MCS_Site = {};

MCS.mcs_MCS_Site.FORM_TYPE_CREATE = 1;
MCS.mcs_MCS_Site.FORM_TYPE_UPDATE = 2;
MCS.mcs_MCS_Site.FORM_TYPE_READ_ONLY = 3;
MCS.mcs_MCS_Site.FORM_TYPE_DISABLED = 4;

MCS.mcs_MCS_Site.CreateName = function (executionContext) {
    //SD - Start
    //web-use-strict-mode
    "use strict";
    //SD - End
    var formContext = executionContext.getFormContext();
    var mcs_type = formContext.getAttribute("mcs_type").getValue();
    var mcs_usernameinput = formContext.getAttribute("mcs_usernameinput").getValue();
    var mcs_stationnumber = formContext.getAttribute("mcs_stationnumber").getValue();
    var nonVASiteFieldValue = (formContext.getAttribute("cvt_nonvasite") != null) ? formContext.getAttribute("cvt_nonvasite").getValue() : null;
    var derivedResultField = "";

    if (mcs_usernameinput != null)
        derivedResultField += mcs_usernameinput + " ";

    if (mcs_type != null)
        derivedResultField += formContext.getAttribute("mcs_type").getText();

    if (nonVASiteFieldValue != null)
        derivedResultField += " : " + nonVASiteFieldValue;

    if (mcs_stationnumber != null)
        derivedResultField += " (" + mcs_stationnumber + ") ";

    if (formContext.getAttribute("mcs_name").getValue() !== derivedResultField) {
        formContext.getAttribute("mcs_name").setSubmitMode("always");
        formContext.getAttribute("mcs_name").setValue(derivedResultField);
    }

    //-To do: remove always saving
    formContext.getAttribute("cvt_nonvasite").setSubmitMode("always");
};

MCS.mcs_MCS_Site.MakeFieldsReadOnly = function (executionContext) {
    return;
    var formContext = executionContext.getFormContext();
    var systemSite = (formContext.getAttribute("mcs_relatedactualsiteid") != null) ? formContext.getAttribute("mcs_relatedactualsiteid").getValue() : null;

    if (systemSite != null) {
        formContext.getControl("mcs_facilityid").setDisabled(true);
        if (formContext.getControl("cvt_nonvasite") != null)
            formContext.getControl("cvt_nonvasite").setDisabled(true);
    }
};

//Displays different fields based on Site Type selected. 
MCS.mcs_MCS_Site.SiteType = function (executionContext) {
    var formContext = executionContext.getFormContext();
    //grab the control for the type field
    var siteTypeValue = (formContext.getAttribute("mcs_type") != null) ? formContext.getAttribute("mcs_type").getValue() : null;
    var nonVASiteField = formContext.getControl("cvt_nonvasite");

    if (siteTypeValue === 917290000)
        nonVASiteField.setVisible(true);
    else
        nonVASiteField.setVisible(false);
};