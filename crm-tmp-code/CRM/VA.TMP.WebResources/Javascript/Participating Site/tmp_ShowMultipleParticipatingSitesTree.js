function BuildJSTree(formContext, selectedItem) {
    debugger;
    var resourcePackageId = formContext.data.entity.getId().replace("{", "").replace("}", "");
    var providerFacility = formContext.getAttribute("cvt_providerfacility");
    var patientFacility = formContext.getAttribute("cvt_patientfacility")
    var groupAppointment = formContext.getAttribute("cvt_groupappointment").getValue();
    var intraorinterFacility = formContext.getAttribute("cvt_intraorinterfacility").getValue();
    var locationTypes = { PROVIDER: 917290000, PATIENT: 917290001 };
    var popupTitle;
    var existingSites = [];

    if (selectedItem.getGrid().controlName == "relatedProviderSites") {//Provider   
        popupTitle = "Provider Participating Sites"
        if (providerFacility) {

            if (providerFacility.getValue() == null || providerFacility.getValue() == undefined) {
                return
            }

            getExistingSites(resourcePackageId, locationTypes.PROVIDER)
                .done(function (results) {
                    BuildJson(formContext, providerFacility, resourcePackageId, "provider", locationTypes.PROVIDER, popupTitle, results);
                });
        }
    }
    else if (selectedItem.getGrid().controlName == "relatedPatientSites") {//Patient
        popupTitle = "Patient Participating Sites"
        if (groupAppointment && patientFacility) {
            if (patientFacility) {

                if (patientFacility.getValue() == null || patientFacility.getValue() == undefined) {
                    return
                }

                getExistingSites(resourcePackageId, locationTypes.PATIENT)
                    .done(function (results) {
                        BuildJson(formContext, patientFacility, resourcePackageId, "patient", locationTypes.PATIENT, popupTitle, results);
                    });
            }

        } else if (!groupAppointment && intraorinterFacility == 917290000) {//Intra Limited to Provider
            if (providerFacility) {

                if (providerFacility.getValue() == null || providerFacility.getValue() == undefined) {
                    return
                }

                getExistingSites(resourcePackageId, locationTypes.PATIENT)
                    .done(function (results) {
                        BuildJson(formContext, providerFacility, resourcePackageId, "patient", locationTypes.PATIENT, popupTitle, results);
                    });
            }
        } else if (!groupAppointment && intraorinterFacility == 917290001) {//Inter
            if (providerFacility) {

                if (providerFacility.getValue() == null || providerFacility.getValue() == undefined) {
                    return
                }

                getExistingSites(resourcePackageId, locationTypes.PATIENT)
                    .done(function (results) {
                        BuildPatientInterFacilityFullJson(formContext, resourcePackageId, "patient", locationTypes.PATIENT, popupTitle, results, providerFacility);
                    });
            }
        }
    }
    else {
        return;
    }

}

function BuildJson(formContext, facilityID, resourcePackageId, locationTypeName, locationType, popupTitle, existingSites) {
    var req = new XMLHttpRequest();
    req.open("GET", Xrm.Utility.getGlobalContext().getClientUrl() + "/api/data/v9.1/mcs_sites?$select=_mcs_facilityid_value,mcs_name&$filter=_mcs_facilityid_value eq " + facilityID.getValue()[0].id.replace("{", "").replace("}", ""), false);
    req.setRequestHeader("OData-MaxVersion", "4.0");
    req.setRequestHeader("OData-Version", "4.0");
    req.setRequestHeader("Accept", "application/json");
    req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
    req.setRequestHeader("Prefer", "odata.include-annotations=OData.Community.Display.V1.FormattedValue");
    req.onreadystatechange = function () {
        if (this.readyState === 4) {
            req.onreadystatechange = null;
            if (this.status === 200) {
                var results = JSON.parse(this.response);

                var packageData = {
                    "resourcePackageId": resourcePackageId,
                    "facilityId": facilityID.getValue()[0].id.replace("{", "").replace("}", ""),
                    "locationType": locationType,
                    "tree": []
                };
                var output = [];
                var tree;
                var parent = false;
                var j;

                for (var i = 0; i < results.value.length; i++) {
                    if (!parent) {
                        j = i;
                        parent = true;
                        //if results[i] is in the existingSites array then set State to disabled
                        tree = { "id": results.value[j]._mcs_facilityid_value, "parent": "#", "text": results.value[j]["_mcs_facilityid_value@OData.Community.Display.V1.FormattedValue"] }
                        output.push(tree);
                    }
                    if (existingSites.indexOf(results.value[i].mcs_siteid) >= 0) {
                        tree = { "id": results.value[i].mcs_siteid, "parent": results.value[j]._mcs_facilityid_value, "text": results.value[i].mcs_name, "state": { "disabled": true } }
                    }
                    else {
                        tree = { "id": results.value[i].mcs_siteid, "parent": results.value[j]._mcs_facilityid_value, "text": results.value[i].mcs_name }
                    }

                    output.push(tree);
                }


                packageData.tree = output;
                // callHtmlPopup(packageData, formContext, popupTitle);

                var pageInput = {
                    pageType: "webresource",
                    webresourceName: "tmp_BuildParticipatingSitesTree",
                    data: JSON.stringify(packageData)
                };
                var navigationOptions = {
                    target: 2,
                    width: 600,
                    height: 500,
                    position: 1,
                    title: popupTitle
                };
                Xrm.Navigation.navigateTo(pageInput, navigationOptions).then(
                    function success() {
                        // Handle dialog closed
                        console.log("Inside Success");
                        if (locationTypeName === "provider")
                            formContext.getControl("relatedProviderSites").refresh();
                        else
                            formContext.getControl("relatedPatientSites").refresh();
                    },
                    function error() {
                        Xrm.Navigation.openAlertDialog({ text: error.message });
                    }
                );
            }
        }
    };
    req.send();
}



function BuildPatientInterFacilityFullJson(formContext, resourcePackageId, locationTypeName, locationType, popupTitle, existingSites, providerFacility) {
    debugger;
    var req = new XMLHttpRequest();
    req.open("GET", Xrm.Utility.getGlobalContext().getClientUrl() + "/api/data/v9.1/mcs_sites?$select=_mcs_facilityid_value,mcs_name,_mcs_businessunitid_value&$expand=mcs_facilityid($select=mcs_businessunitid,mcs_facilityid)&$filter=_mcs_facilityid_value ne " + providerFacility.getValue()[0].id.replace("{", "").replace("}", ""), false);
    req.setRequestHeader("OData-MaxVersion", "4.0");
    req.setRequestHeader("OData-Version", "4.0");
    req.setRequestHeader("Accept", "application/json");
    req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
    req.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
    req.onreadystatechange = function () {
        if (this.readyState === 4) {
            req.onreadystatechange = null;
            if (this.status === 200) {
                var results = JSON.parse(this.response);
                var packageData = {
                    "resourcePackageId": resourcePackageId,
                    "facilityId": [],
                    "locationType": locationType,
                    "tree": []
                };
                var businessArray = [];
                var output = [];
                var tree;
                var facilityArray = [];
                var dupes = [];
                var facilityId;
                var faciltiyandSiteArray = [];

                for (var i = 0; i < results.value.length; i++) {
                    var mcs_siteid = results.value[i]["mcs_siteid"];
                    var siteName = results.value[i].mcs_name;
                    var facilityValue = results.value[i]._mcs_facilityid_value;
                    var facilityName = results.value[i]["_mcs_facilityid_value@OData.Community.Display.V1.FormattedValue"];
                    var visnId = results.value[i]._mcs_businessunitid_value;
                    var visnName = results.value[i]["_mcs_businessunitid_value@OData.Community.Display.V1.FormattedValue"];

                    if (!businessArray.includes(visnId)) {//unique VISN
                        tree = { "id": visnId, "parent": "#", "text": visnName }
                        output.push(tree);
                    }

                    if (existingSites.indexOf(mcs_siteid) >= 0) {
                        tree = { "id": mcs_siteid, "parent": facilityValue, "text": siteName, "state": { "disabled": true } }
                    }
                    else {
                        tree = { "id": mcs_siteid, "parent": facilityValue, "text": siteName }
                    }

                    //tree = { "id": mcs_siteid, "parent": facilityValue, "text": siteName }//All Tmp site
                    // facilityId = { "facilityValue": facilityValue, "facilityName": facilityName, "siteId": mcs_siteid, "siteName": siteName}
                    output.push(tree);
                    // faciltiyandSiteArray.push(facilityId);
                    businessArray.push(visnId);
                    facilityArray.push({ "facilityValue": facilityValue, "facilityName": facilityName, "visnId": visnId });

                }

                $.each(facilityArray, function (index, entry) {//unique Facility
                    if (!dupes[entry.facilityValue]) {
                        if (dupes[entry.facilityValue] = true) {
                            tree = { "id": entry.facilityValue, "parent": entry.visnId, "text": entry.facilityName }
                            output.push(tree);
                        }
                    }
                });

                packageData.tree = output;
                packageData.facilityId = faciltiyandSiteArray;
                localStorage.setItem("packageData", JSON.stringify(packageData));
                // callHtmlPopup(packageData, formContext, popupTitle);

                var pageInput = {
                    pageType: "webresource",
                    webresourceName: "tmp_BuildParticipatingSitesTree",
                    data: "FullLoad"
                };
                var navigationOptions = {
                    target: 2,
                    width: 600,
                    height: 500,
                    position: 1,
                    title: popupTitle
                };
                Xrm.Navigation.navigateTo(pageInput, navigationOptions).then(
                    function success() {
                        // Handle dialog closed
                        console.log("Inside Success");
                        if (locationTypeName === "provider")
                            formContext.getControl("relatedProviderSites").refresh();
                        else
                            formContext.getControl("relatedPatientSites").refresh();
                    },
                    function error() {
                        Xrm.Navigation.openAlertDialog({ text: error.message });
                    }
                );


            } else {
                Xrm.Utility.alertDialog("Error at : BuildPatientInterFacilityFullJson");
            }
        }
    };
    req.send();

}

function callHtmlPopup(packageData, formContext, popupTitle) {

    var pageInput = {
        pageType: "webresource",
        webresourceName: "tmp_BuildParticipatingSitesTree",
        data: JSON.stringify(packageData)
    };
    var navigationOptions = {
        target: 2,
        width: 600,
        height: 500,
        position: 1,
        title: popupTitle
    };
    Xrm.Navigation.navigateTo(pageInput, navigationOptions).then(
        function success() {
            // Handle dialog closed
            console.log("Inside Success");
            formContext.getControl("relatedProviderSites").refresh();
        },
        function error() {
            Xrm.Navigation.openAlertDialog({ text: error.message });
        }
    );
}

function getExistingSites(schedulingPackageId, locationType) {
    var deferred = jQuery.Deferred();
    var existingSites = [];

    Xrm.WebApi.online.retrieveMultipleRecords("cvt_participatingsite", "?$select=_cvt_site_value&$filter=_cvt_resourcepackage_value eq " + schedulingPackageId + " and  cvt_locationtype eq " + locationType).then(
        function success(results) {
            for (var i = 0; i < results.entities.length; i++) {
                var _cvt_site_value = results.entities[i]["_cvt_site_value"];
                var _cvt_site_value_formatted = results.entities[i]["_cvt_site_value@OData.Community.Display.V1.FormattedValue"];
                var _cvt_site_value_lookuplogicalname = results.entities[i]["_cvt_site_value@Microsoft.Dynamics.CRM.lookuplogicalname"];

                existingSites.push(_cvt_site_value);
            }

            deferred.resolve(existingSites);
        },
        function (error) {
            Xrm.Utility.alertDialog(error.message);
            deferred.reject();
        }
    );

    return deferred.promise();
}