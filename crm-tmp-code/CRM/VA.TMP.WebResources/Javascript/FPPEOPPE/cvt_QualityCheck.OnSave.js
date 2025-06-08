//Library Name: cvt_QualityCheck.OnSave.js
//If the SDK namespace object is not defined, create it.
if (typeof (MCS) === "undefined") { MCS = {}; }
// Create Namespace container for functions in this library;
MCS.QualityCheck_OnSave = {};

MCS.QualityCheck_OnSave.CreateName = function (executionContext) {
    'use strict';
    var formContext = executionContext.getFormContext();
    var providerName = formContext.getAttribute("cvt_providerid").getValue();
    var createdOn = formContext.getAttribute("createdon").getValue();
    var flag = formContext.getAttribute("cvt_flag");
    var nameField = "";

    if (providerName != null) {
        nameField += providerName[0].name;
    }
    if (createdOn != null) {
        nameField += " - "
        nameField += createdOn;
    }
    else {
        var todayDt = new Date();
        nameField += " - "
        nameField += todayDt;
    }
    if (flag != null && flag.getSelectedOption() != null) {
        nameField += " ("
        nameField += flag.getSelectedOption().text;
        nameField += ")"
    }

    if (formContext.getAttribute("cvt_name").getValue() !== nameField) {
        formContext.getAttribute("cvt_name").setSubmitMode("always");
        formContext.getAttribute("cvt_name").setValue(nameField);
    }
};