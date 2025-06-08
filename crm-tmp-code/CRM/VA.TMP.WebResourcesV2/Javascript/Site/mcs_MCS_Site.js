//If the SDK namespace object is not defined, create it.
//SD: web-use-strict-equality-operators
if (typeof (MCS) === "undefined") {
    MCS = {};
}
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
    //RM - Start
    //TMP-1304 - Implement TMP Site naming convention
    //RM - End

    var formContext = executionContext.getFormContext();
    var mcs_facility = formContext.getAttribute("mcs_facilityid").getValue();
    var hubSiteName = mcs_facility != null && mcs_facility.length > 0 ? mcs_facility[0].name.indexOf("HUB") ? mcs_facility[0].name : "" : ""
    var mcs_nameAttr = formContext.getAttribute("mcs_name");
    var mcs_type = formContext.getAttribute("mcs_type").getValue();
    var mcs_typeName = mcs_type != null ? formContext.getAttribute("mcs_type").getText() : "";
    var mcs_usernameinput = formContext.getAttribute("mcs_usernameinput").getValue();
    var mcs_stationnumber = formContext.getAttribute("mcs_stationnumber").getValue();
    var nonVASiteSubTypeName = formContext.getAttribute("tmp_nonvasubtype").getText();
    //var tmp_nonvasubtypeAttr = formContext.getAttribute("tmp_nonvasubtype");
    var tmp_nonvasubtypeCtrl = formContext.getControl("tmp_nonvasubtype");
    var mcs_businessunitidAttr = formContext.getAttribute("mcs_businessunitid");
    var visn = mcs_businessunitidAttr != null && mcs_businessunitidAttr.length > 0 && mcs_businessunitidAttr.getValue()[0].name.indexOf("VISN") >= 0
        ? mcs_businessunitidAttr.getValue()[0].name.replace("VISN ", "")
        : "";
    var derivedResultField = "";

    if (mcs_type != null) {
        if (MCS.mcs_MCS_Site.isCustomSiteType(mcs_typeName)) {
            if (mcs_typeName.toUpperCase() === "NTMHC") {
                derivedResultField = "NTMHC ";

                if (mcs_usernameinput != null) derivedResultField += mcs_usernameinput + " ";

                if (mcs_stationnumber != null) derivedResultField += "(" + mcs_stationnumber + ") ";
            }
            else {
                derivedResultField = "V";

                if (mcs_facility !== null) {
                    hubSiteName = mcs_facility != null && mcs_facility.length > 0
                        ? mcs_facility[0].name.toUpperCase().indexOf("HUB") === (mcs_facility[0].name.length - 3)
                            ? "Hub Site"
                            : ""
                        : "";

                    if (visn.length === 0) {
                        MCS.mcs_MCS_Site.getVisn(mcs_facility[0].id)
                            .done(function (result) {
                                console.log(result);
                                formContext.getAttribute("mcs_businessunitid").setValue(result);
                                visn = result[0].name.replace("VISN ", "");

                                if (visn.length < 2) visn = "0" + visn;

                                derivedResultField += visn + " ";

                                derivedResultField += mcs_typeName + " ";

                                if (mcs_usernameinput != null) derivedResultField += mcs_usernameinput + " ";

                                if (hubSiteName !== "" && mcs_typeName.toUpperCase() !== "CCC") derivedResultField += hubSiteName + " ";

                                if (mcs_stationnumber != null) derivedResultField += "(" + mcs_stationnumber + ")";

                                mcs_nameAttr.setValue(derivedResultField);
                            });
                    }
                    else {
                        if (visn.length < 2) visn = "0" + visn;

                        derivedResultField += visn + " ";

                        derivedResultField += mcs_typeName + " ";

                        if (mcs_usernameinput != null) derivedResultField += mcs_usernameinput + " ";

                        if (hubSiteName !== "" && mcs_typeName.toUpperCase() !== "CCC") derivedResultField += hubSiteName + " ";

                        if (mcs_stationnumber != null) derivedResultField += "(" + mcs_stationnumber + ")";

                        mcs_nameAttr.setValue(derivedResultField);
                    }
                }
                else {
                    if (visn !== null) {
                        derivedResultField += visn + " ";
                    }

                    derivedResultField += mcs_typeName + " ";

                    if (mcs_usernameinput != null) derivedResultField += mcs_usernameinput + " ";

                    if (hubSiteName !== "" && mcs_typeName.toUpperCase() !== "CCC") derivedResultField += hubSiteName + " ";

                    if (mcs_stationnumber != null) derivedResultField += "(" + mcs_stationnumber + ")";
                }
            }
        }
        else {
            if (mcs_usernameinput != null) derivedResultField += mcs_usernameinput + " ";

            if (mcs_typeName !== "") {
                derivedResultField += mcs_typeName;
                if (mcs_typeName.toLowerCase() === "non-va site") {
                    tmp_nonvasubtypeCtrl.setVisible(true);
                }
                else {
                    tmp_nonvasubtypeCtrl.setVisible(false);
                    nonVASiteSubTypeName = null;
                }
            }
            else {
                tmp_nonvasubtypeCtrl.setVisible(false);
                nonVASiteSubTypeName = null;
            }

            nonVASiteSubTypeName = nonVASiteSubTypeName !== null && nonVASiteSubTypeName.length > 0
                ? MCS.mcs_MCS_Site.getCustomSubType(nonVASiteSubTypeName)
                : null;

            if (nonVASiteSubTypeName != null) derivedResultField += ": " + nonVASiteSubTypeName;

            if (mcs_stationnumber != null) derivedResultField += " (" + mcs_stationnumber + ") ";
        }
    }

    if (mcs_nameAttr.getValue() !== derivedResultField) {
        mcs_nameAttr.setSubmitMode("always");
        mcs_nameAttr.setValue(derivedResultField);
    }
};

//RM - Start
//TMP-1304 - Implement TMP Site naming convention
//RM - End
MCS.mcs_MCS_Site.getVisn = function (facilityId) {
    var deferred = new jQuery.Deferred();
    Xrm.WebApi.retrieveRecord("mcs_facility", facilityId.replace('{', '').replace('}', '')).then(
        function success(facility) {
            if (facility !== null) {
                console.log(facility);
                var visnArr = [];
                var visn = {
                    id: '{' + facility["_mcs_visn_value"] + '}',
                    entityType: "businessunit",
                    name: facility["_mcs_visn_value@OData.Community.Display.V1.FormattedValue"]
                };
                visnArr.push(visn)
                deferred.resolve(visnArr);
            }
        },
        function (error) {
            deferred.fail();
        }
    )

    return deferred.promise();
}

//RM - Start
//TMP-1304 - Implement TMP Site naming convention
//RM - End
MCS.mcs_MCS_Site.isCustomSiteType = function (typeName) {
    var customTypeNames = ['CCC', 'CRH', 'NATL', 'NTMHC'];
    var isCustomType = false;
    if (typeName != "") {
        if (customTypeNames.indexOf(typeName) >= 0) isCustomType = true;
    }

    return isCustomType;
};

//RM - Start
//TMP-1304 - Implement TMP Site naming convention
//RM - End
MCS.mcs_MCS_Site.getCustomSubType = function (subTypeName) {
    var customSubTypeNames = ['CITC', 'LGN', 'VFW', 'WAL'];
    var subTypeReplacmentNames = ['Community Care', 'ATLAS American Legion', 'ATLAS VFW', 'ATLAS Walmart']

    if (customSubTypeNames.indexOf(subTypeName) >= 0) {
        return subTypeReplacmentNames[customSubTypeNames.indexOf(subTypeName)];
    }
    else {
        return subTypeName;
    }
}

MCS.mcs_MCS_Site.MakeFieldsReadOnly = function (executionContext) {
    return;
    var formContext = executionContext.getFormContext();
    var systemSite = (formContext.getAttribute("mcs_relatedactualsiteid") != null) ? formContext.getAttribute("mcs_relatedactualsiteid").getValue() : null;

    if (systemSite != null) {
        formContext.getControl("mcs_facilityid").setDisabled(true);
        if (formContext.getControl("cvt_nonvasite") != null) formContext.getControl("cvt_nonvasite").setDisabled(true);
    }
};

//Displays different fields based on Site Type selected.
MCS.mcs_MCS_Site.SiteType = function (executionContext) {
    //RM - Start
    //TMP-1304 - Implement TMP Site naming convention
    //RM - End

    var formContext = executionContext.getFormContext();
    var mcs_facility = formContext.getAttribute("mcs_facilityid").getValue();

    //grab the control for the type field
    var siteTypeValue = (formContext.getAttribute("mcs_type") != null) ? formContext.getAttribute("mcs_type").getValue() : null;
    var nonVASiteField = formContext.getControl("cvt_nonvasite");
    var nonVASubTypeField = formContext.getControl("tmp_nonvasubtype");
    var resetValue = "";

    if (siteTypeValue === 917290000) {
        nonVASiteField.setVisible(true);
        nonVASubTypeField.setVisible(true);
    }

    else {
        nonVASiteField.setVisible(false);
        formContext.getAttribute("cvt_nonvasite").setValue(resetValue);
        nonVASubTypeField.setVisible(false);
        formContext.getAttribute("tmp_nonvasubtype").setValue(resetValue);
    }
    if (mcs_facility !== null)
        MCS.mcs_MCS_Site.getVisn(mcs_facility[0].id)
            .done(function (result) {
                //console.log(result);
                formContext.getAttribute("mcs_businessunitid").setValue(result);
            });
};