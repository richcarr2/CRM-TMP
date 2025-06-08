if (typeof MCS == "undefined")
    MCS = {};

if (typeof MCS.Team == "undefined")
    MCS.Team = {};

MCS.Team.OnChange = {}; if (typeof MCS === "undefined")
    MCS = {};

if (typeof MCS.Team === "undefined")
    MCS.Team = {};

MCS.Team.OnChange = {};

MCS.Team.OnChange.ShowHideServiceLine = function (executionContext) {
    'use strict';
    var formContext = executionContext.getFormContext();
    var specialty = formContext.getControl("cvt_servicetype");
    var roleType = formContext.getAttribute("cvt_type");
    var showSpecialty = roleType.getValue() === 917290001 || roleType.getValue() === 917290008;

    specialty.setVisible(showSpecialty);

    if (!showSpecialty) {
        formContext.getAttribute("cvt_servicetype").setValue(null);
        formContext.getAttribute('cvt_servicetype').setRequiredLevel("none");
    }
    else
        formContext.getAttribute('cvt_servicetype').setRequiredLevel("required");
};

MCS.Team.OnChange.CreateName = function (executionContext) {
    var formContext = executionContext.getFormContext();
    var fac = formContext.getAttribute("cvt_facility").getValue();
    var serviceLine = formContext.getAttribute("cvt_servicetype").getValue();
    var role = formContext.getAttribute("cvt_type").getValue();
    var facilityName = "", serviceLineName = "", roleName = "", derivedResultField = "";

    if (serviceLine != null) {
        serviceLineName = serviceLine[0].name;
        derivedResultField = serviceLineName;
    }

    switch (role) {
        case 917290000: //FTC
            roleName = " FTC Approval Group";
            break;
        case 917290001:
            roleName = " Service Chief Approval Group";
            break;
        case 917290002:
            roleName = " Chief of Staff Approval Group";
            break;
        case 917290003:
            roleName = " Credentialing and Privileging Officer Approval Group";
            break;
        case 917290004:
            roleName = "TSA Notification Group";
            break;
        case 917290005:
            roleName = "Scheduler Group";
            break;
        case 917290006:
            roleName = "Data Administrators";
            break;
        case 917290007:
            roleName = "Staff";
            break;
        case 917290008:
            roleName = " ER On-Call Providers";
            formContext.getAttribute("cvt_facility").setValue(null);
            break;
        case 917290009:
            roleName = "Hub Director";
            break;
        case 917290010:
            roleName = "Hub TSA Manager";
            break;
    }
    derivedResultField += roleName;

    if (fac !== null && role !== 917290008) {
        facilityName = fac[0].name;
        derivedResultField += " @ " + facilityName;
    }

    if (formContext.getAttribute("name").getValue() !== derivedResultField && role !== null) {
        formContext.getAttribute("name").setSubmitMode("always");
        formContext.getAttribute("name").setValue(derivedResultField);
    }
};

MCS.Team.OnChange.ShowHideServiceLine = function (executionContext) {
    var formContext = executionContext.getFormContext();
    var specialty = formContext.getControl("cvt_servicetype");
    var roleType = formContext.getAttribute("cvt_type");
    var showSpecialty = roleType.getValue() == 917290001 || roleType.getValue() == 917290008;

    specialty.setVisible(showSpecialty);

    if (!showSpecialty) {
        formContext.getAttribute("cvt_servicetype").setValue(null);
        formContext.getAttribute('cvt_servicetype').setRequiredLevel("none");
    }
    else
        formContext.getAttribute('cvt_servicetype').setRequiredLevel("required");
};

MCS.Team.OnChange.CreateName = function (executionContext) {
    var formContext = executionContext.getFormContext();
    var fac = formContext.getAttribute("cvt_facility").getValue();
    var serviceLine = formContext.getAttribute("cvt_servicetype").getValue();
    var role = formContext.getAttribute("cvt_type").getValue();
    var facilityName = "", serviceLineName = "", roleName = "", derivedResultField = "";

    if (serviceLine != null) {
        serviceLineName = serviceLine[0].name;
        derivedResultField = serviceLineName;
    }

    switch (role) {
        case 917290000: //FTC
            roleName = " FTC Approval Group";
            break;
        case 917290001:
            roleName = " Service Chief Approval Group";
            break;
        case 917290002:
            roleName = " Chief of Staff Approval Group";
            break;
        case 917290003:
            roleName = " Credentialing and Privileging Officer Approval Group";
            break;
        case 917290004:
            roleName = "TSA Notification Group";
            break;
        case 917290005:
            roleName = "Scheduler Group";
            break;
        case 917290006:
            roleName = "Data Administrators";
            break;
        case 917290007:
            roleName = "Staff";
            break;
        case 917290008:
            roleName = " ER On-Call Providers";
            formContext.getAttribute("cvt_facility").setValue(null);
            break;
        case 917290009:
            roleName = "Hub Director";
            break;
        case 917290010:
            roleName = "Hub TSA Manager";
            break;
    }
    derivedResultField += roleName;

    if (fac != null && role != 917290008) {
        facilityName = fac[0].name;
        derivedResultField += " @ " + facilityName;
    }

    if (formContext.getAttribute("name").getValue() != derivedResultField && role != null) {
        formContext.getAttribute("name").setSubmitMode("always");
        formContext.getAttribute("name").setValue(derivedResultField);
    }
};