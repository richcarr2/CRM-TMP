if (typeof MCS == "undefined") {
    MCS = {};
}
// Create Namespace container for functions in this library;
if (typeof (MCS.ParticipatingSite) == "undefined") {
    MCS.ParticipatingSite = {};
}


//Page level variables
MCS.ParticipatingSite.SaveInProgress = false;
MCS.ParticipatingSite.Site = [];

MCS.ParticipatingSite.EntityId = null;
MCS.ParticipatingSite.PSName = null;
MCS.ParticipatingSite.Side = null;
MCS.ParticipatingSite.Group = false;
MCS.ParticipatingSite.Modality = null;

MCS.ParticipatingSite.OnLoad = function (executionContext) {
    var formContext = executionContext.getFormContext();
    MCS.ParticipatingSite.OnlyProviderHM(executionContext);

    if (formContext.ui.getFormType() !== 0) {
        formContext.getControl("cvt_resourcepackage").setDisabled(true);
        formContext.getControl("cvt_site").setDisabled(true);
        formContext.getControl("cvt_locationtype").setDisabled(true);
        formContext.getControl("cvt_scheduleable").setDisabled(false);

        Xrm.WebApi.retrieveRecord("cvt_resourcepackage", formContext.getAttribute("cvt_resourcepackage").getValue()[0].id, "?$select=cvt_usagetype,cvt_availabletelehealthmodality,cvt_groupappointment,cvt_patientlocationtype").then(
            function success(result) {
                MCS.ParticipatingSite.SchedulingPackage = result;
                //alert("cvt_resourcepackage retrieve success:\nrecord id: " + formContext.getAttribute("cvt_resourcepackage").getValue()[0].id + "\ntype: " + (result.cvt_usagetype==1)?"TSA":"Scheduling");
                if (result.cvt_usagetype == 1) {
                    //then we know this is a "TSA" Package.
                    //set 'Can be scheduled' field = no
                    formContext.getAttribute("cvt_scheduleable").setValue(false);
                    //set 'Can be scheduled' field = disabled
                    formContext.getControl("cvt_scheduleable").setDisabled(true);
                    //set 'Can be scheduled' field visibility = false (hide it)
                    formContext.getControl("cvt_scheduleable").setVisible(false);
                    //hide the "Service Details" tab
                    formContext.ui.tabs.get("tab_5").setVisible(false);
                }
            },
            function (error) {
                //alert("cvt_resourcepackage retrieve error: " + error.message);
                var alertOptions = {
                    height: 200,
                    width: 300
                };
                var alertStrings = {
                    confirmButtonLabel: "OK",
                    text: "cvt_resourcepackage retrieve error: " + error.message,
                    title: "cvt_resourcepackage retrive error."
                };
                Xrm.Navigation.openAlertDialog(alertStrings, alertOptions);
            });

        //Get the values for the ribbon
        MCS.ParticipatingSite.EntityId = formContext.data.entity.getId();
        MCS.ParticipatingSite.PSName = formContext.getAttribute("cvt_name").getValue();
        MCS.ParticipatingSite.Side = (formContext.getAttribute("cvt_locationtype").getValue() == 917290000) ? "Provider" : "Patient";

        if (formContext.getAttribute("cvt_site").getValue() !== null) {
            MCS.ParticipatingSite.Site = formContext.getAttribute("cvt_site").getValue();
        }

        if (MCS.ParticipatingSite.Side === "Provider" && MCS.ParticipatingSite.Modality === "SFT") {
            MCS.cvt_Common.Notifications("Add", 3, "Provider-side resources are not booked for asynchronous Store Forward appointments and therefore resources do not need to be added.");
            //Hide Resources tab
            //formContext.ui.tabs.get("tab_resource").setVisible(false);
        }
        if (MCS.ParticipatingSite.Group === true && MCS.ParticipatingSite.Side === "Patient") {
            formContext.getControl("cvt_providers").setVisible(false);
            formContext.getControl("cvt_providersitevistaclinics").setVisible(false);
            formContext.getControl("cvt_oppositevalidsites").setVisible(false);
            formContext.getControl("cvt_relatedservice").setVisible(false);
        }
        else if (MCS.ParticipatingSite.Group === true && MCS.ParticipatingSite.Side === "Provider") {
            //formContext.getControl("cvt_patientsitevistaclinics").setVisible(false);
        }
        else if (MCS.ParticipatingSite.Group === false && MCS.ParticipatingSite.Side === "Provider") {
            formContext.getControl("cvt_patientsitevistaclinics").setVisible(false);
            formContext.getControl("cvt_oppositevalidsites").setVisible(false);
            formContext.getControl("cvt_relatedservice").setVisible(false);
        }
        else if (MCS.ParticipatingSite.Group === false && MCS.ParticipatingSite.Side === "Patient") {
            //formContext.getControl("cvt_providersitevistaclinics").setVisible(false);
            //formContext.getControl("cvt_providers").setVisible(false);
        }
    }
    else {
        formContext.getControl("cvt_scheduleable").setDisabled(true);
    }
};

MCS.ParticipatingSite.ChooseSite = function (executionContext) {
    //debugger;
    var formContext = executionContext.getFormContext();
    //Make the SOS field visible and get the url
    if (formContext.getAttribute("cvt_site").getValue() !== null) {

        Xrm.WebApi.retrieveRecord("mcs_site", formContext.getAttribute("cvt_site").getValue()[0].id, "?$select=_mcs_facilityid_value").then(
            function success(result) {
                debugger;
                if (result && result._mcs_facilityid_value !== null) {
                    var lookup = new Array();
                    lookup[0] = new Object();
                    lookup[0].id = result._mcs_facilityid_value;
                    lookup[0].name = result["_mcs_facilityid_value@OData.Community.Display.V1.FormattedValue"];
                    lookup[0].entityType = "mcs_facility";
                    formContext.getAttribute("cvt_facility").setValue(lookup)
                    formContext.getAttribute("cvt_facility").setSubmitMode("always");
                }
            },
            function (error) {
                //console.log(error.message);
                // handle error conditions
            });

        //var calls = CrmRestKit.Retrieve("mcs_site", Xrm.Page.getAttribute("cvt_site").getValue()[0].id, ['mcs_FacilityId'], false);
        //calls.fail(
        //        function (error) {
        //        }).done(function (data) {
        //            if (data && data.d && data.d.mcs_FacilityId != null) {
        //                var lookup = new Array();
        //                lookup[0] = new Object();
        //                lookup[0].id = data.d.mcs_FacilityId.Id;
        //                lookup[0].name = data.d.mcs_FacilityId.Name;
        //                lookup[0].entityType = "mcs_facility";
        //                Xrm.Page.getAttribute("cvt_facility").setValue(lookup)
        //                Xrm.Page.getAttribute("cvt_facility").setSubmitMode("always");
        //            }
        //        });
    }
    else {
        //clear out Facility field
        //save
        if (formContext.getAttribute("cvt_facility").getValue() !== null) {
            formContext.getAttribute("cvt_facility").setValue(null)
        }
    }
};

MCS.ParticipatingSite.CreateName = function (executionContext) {
    var formContext = executionContext.getFormContext();
    var canBeScheduled = formContext.getAttribute("cvt_scheduleable").getValue();

    var derivedResultField = "";
    var site = formContext.getAttribute("cvt_site").getValue();
    var locationType = formContext.getAttribute("cvt_locationtype").getValue();

    var siteName = "";
    var locationText = "";

    if (site !== null) siteName = site[0].name;

    if (locationType !== null) {
        switch (locationType) {
            case 917290000:
                locationText += "Pro";
                break;
            case 917290001:
                locationText += "Pat";
                break;
        }
    }

    derivedResultField = locationText + " - " + siteName;

    if (formContext.getAttribute("cvt_name").getValue() !== derivedResultField) {
        formContext.getAttribute("cvt_name").setSubmitMode("always");
        formContext.getAttribute("cvt_name").setValue(derivedResultField);
    }

    if (!MCS.ParticipatingSite.SaveInProgress && canBeScheduled) {
        MCS.ParticipatingSite.SaveInProgress = true;
        executionContext.getEventArgs().preventDefault();

        MCS.ParticipatingSite.GetVistaClinics([MCS.ParticipatingSite.EntityId]).then(
            function (vistaClinics) {
                if (vistaClinics.length === 0) {
                    var groupAppointment = MCS.ParticipatingSite.SchedulingPackage.cvt_groupappointment;
                    var patientLocationType = MCS.ParticipatingSite.SchedulingPackage.cvt_patientlocationtype;
                    var telehealthModality = MCS.ParticipatingSite.SchedulingPackage.cvt_availabletelehealthmodality;
                    var standardWarning = "A {locationtype} Clinic is usually required for {AppointmentModalityEquivalent}.  If no {locationtype} Clinic is added, no {locationtype} Side location (VistA Clinic or Cerner Ambulatory Location) will be scheduled.";
                    var nationalWarning = "National Requirements are that Provider Clinic is not required for SFT.  Provider Side Resources will only be used as a reference and will not be scheduled.";
                    var locationTypeReplace = /{locationtype}/gi;

                    switch (patientLocationType) {
                        case 917290000: //Clinic Based
                            switch (telehealthModality) {
                                case 917290000: //Clinical Video Telehealth
                                    if (locationType === 917290000) {
                                        standardWarning = standardWarning.replace(locationTypeReplace, "Provider");
                                    }
                                    else {
                                        standardWarning = standardWarning.replace(locationTypeReplace, "Patient");
                                    }
                                    if (!groupAppointment) { //No
                                        standardWarning = standardWarning.replace("{AppointmentModalityEquivalent}", "CVT");
                                    }
                                    else { //Yes
                                        standardWarning = standardWarning.replace("{AppointmentModalityEquivalent}", "CVT Group");
                                    }
                                    break;
                                case 917290001: //Store and Forward
                                    if (locationType === 917290000) {
                                        standardWarning = nationalWarning;
                                    }
                                    else {
                                        standardWarning = standardWarning.replace(locationTypeReplace, "Patient");
                                    }
                                    if (!groupAppointment) { //No
                                        standardWarning = standardWarning.replace("{AppointmentModalityEquivalent}", "SFT");
                                    }
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case 917290001: //VA Video Connect/Telephone
                            switch (telehealthModality) {
                                case 917290000: //Clinical Video Telehealth
                                    if (locationType === 917290000) {
                                        standardWarning = standardWarning.replace(locationTypeReplace, "Provider");
                                    }
                                    else {
                                        standardWarning = standardWarning.replace(locationTypeReplace, "Patient");
                                    }
                                    if (!groupAppointment) { //No
                                        if (locationType === 917290000) {
                                            standardWarning = standardWarning.replace("{AppointmentModalityEquivalent}", "VVC");
                                        }
                                        else {
                                            standardWarning = standardWarning.replace("{AppointmentModalityEquivalent}", "VVC w/PSRR");
                                        }
                                    }
                                    else { //Yes
                                        standardWarning = standardWarning.replace("{AppointmentModalityEquivalent}", "VVC Group");
                                    }
                                    break;
                                case 917290002: //Telephone
                                    if (!groupAppointment) { //No
                                        if (locationType === 917290000) {
                                            standardWarning = standardWarning.replace(locationTypeReplace, "Provider");
                                            standardWarning = standardWarning.replace("{AppointmentModalityEquivalent}", "PHONE");
                                        }
                                        else {
                                            standardWarning = standardWarning.replace(locationTypeReplace, "Patient");
                                            standardWarning = standardWarning.replace("{AppointmentModalityEquivalent}", "PHONE w/PSRR");
                                        }
                                    }
                                    break;
                                default:
                                    break;
                            }
                            break;
                        default:
                            break;
                    }

                    var confirmStrings = {
                        subtitle: standardWarning,
                        text: "To continue, click OK, to abort, click Cancel.",
                        title: "Missing Vista Clinics"
                    };
                    Xrm.Navigation.openConfirmDialog(confirmStrings).then(
                        function (success) {
                            if (success.confirmed) {
                                formContext.data.save({ saveMode: executionContext.getEventArgs().getSaveMode() }).then(
                                    function (success) {
                                        MCS.ParticipatingSite.SaveInProgress = false;
                                    },
                                    function (error) {
                                        MCS.ParticipatingSite.SaveInProgress = false;
                                    }
                                );
                            }
                            else {
                                MCS.ParticipatingSite.SaveInProgress = false;
                            }
                        },
                        function (error) { }
                    );
                }
                else {
                    formContext.data.save({ saveMode: executionContext.getEventArgs().getSaveMode() }).then(
                        function (success) {
                            MCS.ParticipatingSite.SaveInProgress = false;
                        },
                        function (error) {
                            MCS.ParticipatingSite.SaveInProgress = false;
                        }
                    );
                }
            },
            function (error) {
                Xrm.Navigation.openAlertDialog(error.alertStrings, error.alertOptions);
                MCS.ParticipatingSite.SaveInProgress = false;
            }
        );
    }
};

MCS.ParticipatingSite.GetVistaClinics = function (participatingSiteIds) {
    var fetchXml = "?fetchXml=<fetch><entity name=\"cvt_schedulingresource\"><attribute name=\"cvt_resourcetype\"/><attribute name=\"cvt_name\"/><attribute name=\"cvt_participatingsite\"/><filter><condition attribute=\"cvt_participatingsite\" operator=\"in\" value=\"\">{participatingsiteids}</condition></filter><link-entity name=\"mcs_resourcegroup\" from=\"mcs_resourcegroupid\" to=\"cvt_tmpresourcegroup\" link-type=\"outer\" alias=\"mcs_resourcegroup\"><link-entity name=\"mcs_groupresource\" from=\"mcs_relatedresourcegroupid\" to=\"mcs_resourcegroupid\" link-type=\"outer\" alias=\"mcs_groupresource\"><link-entity name=\"mcs_resource\" from=\"mcs_resourceid\" to=\"mcs_relatedresourceid\" link-type=\"outer\" alias=\"tmp_resource\"><attribute name=\"mcs_type\"/><attribute name=\"mcs_name\"/></link-entity></link-entity></link-entity></entity></fetch>";
    var participatingSiteIdValues = "";
    var vistaClinics = [];

    participatingSiteIds.forEach((participatingSiteId) => {
        participatingSiteIdValues += "<value>" + participatingSiteId.replace("{", "").replace("}", "") + "</value>";
    });
    fetchXml = fetchXml.replace("{participatingsiteids}", participatingSiteIdValues);

    return new Promise(function (resolve, reject) {
        Xrm.WebApi.retrieveMultipleRecords("cvt_schedulingresource", fetchXml).then(
            function success(resources) {
                debugger;
                if (resources != null) {
                    resources.entities.forEach((resource) => {
                        if (resource.cvt_resourcetype === 251920000 || (resource["tmp_resource.mcs_type"] !== undefined && resource["tmp_resource.mcs_type"] === 251920000)) {
                            if (vistaClinics.indexOf(resource["_cvt_participatingsite_value"]) < 0) vistaClinics.push(resource["_cvt_participatingsite_value"]);
                        }
                    });
                }
                resolve(vistaClinics);
            },
            function (error) {
                var alertOptions = {
                    height: 200,
                    width: 300
                };
                var alertStrings = {
                    confirmButtonLabel: "OK",
                    text: "cvt_resourcepackage retrieve error: " + error.message,
                    title: "cvt_resourcepackage retrive error."
                };
                reject({ alertStrings: alertStrings, alertOptions: alertOptions });
            }
        );
    });
};

MCS.ParticipatingSite.OnlyProviderHM = function (executionContext) {
    var formContext = executionContext.getFormContext();
    if (formContext.getAttribute("cvt_resourcepackage").getValue() !== null) {

        Xrm.WebApi.retrieveRecord("cvt_resourcepackage", formContext.getAttribute("cvt_resourcepackage").getValue()[0].id, "?$select=cvt_patientlocationtype,cvt_availabletelehealthmodality,cvt_groupappointment").then(
            function success(result) {
                if (result) {
                    if (result.cvt_patientlocationtype !== null) {
                        if (result.cvt_patientlocationtype.Value === 917290001) {
                            //RP is H/M only Provider Site allowed.
                            //Display note at top of screen and default location type
                            formContext.getAttribute("cvt_locationtype").setValue(917290000)
                            formContext.getControl("cvt_locationtype").setDisabled(true);
                            MCS.cvt_Common.Notifications("Add", 3, "Only Provider Sites can be added to a VA Video Connect Resource Package.");

                            //Hide Related Service, Opp Valid Sites, and Patient Site Vista Clinics
                            formContext.getControl("cvt_patientsitevistaclinics").setVisible(false);
                            formContext.getControl("cvt_oppositevalidsites").setVisible(false);
                            formContext.getControl("cvt_relatedservice").setVisible(false);
                        }
                        else {
                            formContext.getControl("cvt_locationtype").setDisabled(false);
                            MCS.cvt_Common.Notifications("Hide");
                        }
                    }
                    MCS.ParticipatingSite.Group = result.cvt_groupappointment;

                    if (result.cvt_availabletelehealthmodality) {
                        if (result.cvt_availabletelehealthmodality.Value === 917290000) MCS.ParticipatingSite.Modality = "CVT";
                        else MCS.ParticipatingSite.Modality = "SFT";
                    }
                }

            },
            function (error) {
                console.log(error.message);
                // handle error conditions
            });

        //var calls = CrmRestKit.Retrieve("cvt_resourcepackage", Xrm.Page.getAttribute("cvt_resourcepackage").getValue()[0].id, ['cvt_patientlocationtype', 'cvt_availabletelehealthmodality', 'cvt_groupappointment'], false);
        //calls.fail(
        //        function (error) {
        //        }).done(function (data) {
        //            if (data && data.d) {
        //                if (data.d.cvt_patientlocationtype != null) {
        //                    if (data.d.cvt_patientlocationtype.Value == 917290001) {
        //                        //RP is H/M only Provider Site allowed.
        //                        //Display note at top of screen and default location type
        //                        Xrm.Page.getAttribute("cvt_locationtype").setValue(917290000)
        //                        Xrm.Page.getControl("cvt_locationtype").setDisabled(true);
        //                        MCS.cvt_Common.Notifications("Add", 3, "Only Provider Sites can be added to a VA Video Connect Resource Package.");
        //                        //Hide Related Service, Opp Valid Sites, and Patient Site Vista Clinics
        //                        Xrm.Page.getControl("cvt_patientsitevistaclinics").setVisible(false);
        //                        Xrm.Page.getControl("cvt_oppositevalidsites").setVisible(false);
        //                        Xrm.Page.getControl("cvt_relatedservice").setVisible(false);
        //                    }
        //                    else {
        //                        Xrm.Page.getControl("cvt_locationtype").setDisabled(false);
        //                        MCS.cvt_Common.Notifications("Hide");
        //                    }
        //                }
        //                MCS.ParticipatingSite.Group = data.d.cvt_groupappointment;
        //                if (data.d.cvt_availabletelehealthmodality) {
        //                    if (data.d.cvt_availabletelehealthmodality.Value == 917290000)
        //                        MCS.ParticipatingSite.Modality = "CVT";
        //                    else
        //                        MCS.ParticipatingSite.Modality = "SFT";
        //                }
        //            }
        //        });
    }
};
//If the SDK namespace object is not defined, create it.
if (typeof (MCS) === "undefined") {
    MCS = {};
}
// Create Namespace container for functions in this library;
if (typeof (MCS.PS_Buttons) == "undefined") {
    MCS.PS_Buttons = {};
}

//Page Scope Variables
MCS.PS_Buttons.Async = false;

function loadScript(url, callback) {
    // Adding the script tag to the head as suggested before
    var head = document.head;
    var script = document.createElement('script');
    script.type = 'text/javascript';
    script.src = url;

    // Then bind the event to the callback function.
    // There are several events for cross browser compatibility.
    script.onreadystatechange = callback;
    script.onload = callback;

    // Fire the loading
    head.appendChild(script);
}

MCS.PS_Buttons.CheckVistaClinics = function (selectedIds) {
    var warningMessage = "The maximum number of records that can be selected for this action is 15."
    if (selectedIds.length > 1) {
        MCS.ParticipatingSite.GetVistaClinics(selectedIds).then(
            function (vistaClinics) {
                if (selectedIds.length > 15) {
                    var confirmStrings = {
                        text: warningMessage,
                        title: "Too many selected Sites"
                    };
                    Xrm.Navigation.openAlertDialog(confirmStrings).then(
                        function (success) {
                        },
                        function (error) {
                            var alertOptions = {
                                height: 200,
                                width: 300
                            };
                            var alertStrings = {
                                confirmButtonLabel: "OK",
                                text: "cvt_schedulingresource retrieve error: " + error.message,
                                title: "cvt_schedulingresource retrieve error."
                            };
                            Xrm.Navigation.openAlertDialog(alertStrings, alertOptions);
                        }
                    );
                }
                else if (vistaClinics.length < selectedIds.length) {
                    var confirmStrings = {
                        subtitle: "One or more selected Participating Sites are missing Clinics and if no Clinics are added, then it will not be scheduled.",
                        text: "To continue, click OK, to abort, click Cancel.",
                        title: "Missing Vista Clinics"
                    };
                    Xrm.Navigation.openConfirmDialog(confirmStrings).then(
                        function (success) {
                            if (success.confirmed) {
                                Xrm.Navigation.navigateTo({
                                    pageType: "bulkedit",
                                    entityName: "cvt_participatingsite",
                                    entityIds: selectedIds
                                }, {
                                    target: 2,
                                    width: {
                                        value: 40,
                                        unit: '%'
                                    },
                                    position: 2
                                });
                            }
                        },
                        function (error) {
                            var alertOptions = {
                                height: 200,
                                width: 300
                            };
                            var alertStrings = {
                                confirmButtonLabel: "OK",
                                text: "cvt_schedulingresource retrieve error: " + error.message,
                                title: "cvt_schedulingresource retrieve error."
                            };
                            Xrm.Navigation.openAlertDialog(alertStrings, alertOptions);
                        }
                    );
                }
                else {
                    Xrm.Navigation.navigateTo({
                        pageType: "bulkedit",
                        entityName: "cvt_participatingsite",
                        entityIds: selectedIds
                    }, {
                        target: 2,
                        width: {
                            value: 40,
                            unit: '%'
                        },
                        position: 2
                    });
                }
            },
            function (error) {
                Xrm.Navigation.openAlertDialog(error.alertStrings, error.alertOptions);
            }
        );
    }
    else {
        Xrm.Navigation.navigateTo({
            pageType: "entityrecord",
            entityName: "cvt_participatingsite",
            entityId: selectedIds[0]
        });
    }
};

//HELPER - Unsupported method of re-creating Multi-select Lookup window in order to Create a different type of record than you are looking up
MCS.PS_Buttons.buildshowStandardDialog = function (entityName, viewName, callback) {
    //var otcCode = MCS.cvt_Common.getObjectTypeCode(entityName);
    var otcCode = "";
    if (entityName === "mcs_resourcegroup") {
        otcCode = "10011";
    }
    else if (entityName === "mcs_resource") {
        otcCode = "10010";
    }
    else if (entityName === "systemuser") {
        otcCode = "8";
    }
    if (MCS.ParticipatingSite.Site !== null) {
        var customViewId = MCS.ParticipatingSite.Site[0].id;
        var customView;

        if (MCS.ParticipatingSite.Side === "Provider" && MCS.ParticipatingSite.Modality === "SFT") {
            if (viewName == "Resources By Site") {
                //Prevent users from adding Provider Side Resources
                alert("Per Business Rules, you should enter only a Provider or Provider Group on this SFT Resource Package.");
                return;
            }
        }
        else if (MCS.ParticipatingSite.Side === "Patient" && MCS.ParticipatingSite.isGroup) {
            if (viewName !== "Resource Groups By Site") {
                //Prevent users from adding Patient Side Resources or Users
                alert("Per Business Rules, you should enter Patient Side Paired Groups on this Group Resource Package.");
                return;
            }
        }
        debugger;
        var globalContext = Xrm.Utility.getGlobalContext();
        var clientUrl = globalContext.getClientUrl();
        var FetchXml;
        var LayoutXml;
        var FetchFilters;
        var DefViewId;
        switch (viewName) {
            case "Users By Site":
                DefViewId = "00000000-0000-0000-00AA-000010001019";
                FetchFilters = getUserFilters(MCS.ParticipatingSite.Site);
                 //FetchXml = getUserFetchXml(MCS.ParticipatingSite.Site);
                // LayoutXml = getUserLayout(otcCode);
                break;
            case "Resource Groups By Site":
                DefViewId = "BF61DAD7-C2FC-430C-AB7B-04D0F5CCEAD1";
                if (MCS.ParticipatingSite.Side == "Patient" && MCS.ParticipatingSite.Group) {
                    // FetchXml = getResourceGroupFetchXml(MCS.ParticipatingSite.Site, true);
                    // LayoutXml = getResourceGroupLayout(otcCode);
                    FetchFilters = getResourceGroupFilters(MCS.ParticipatingSite.Site, true);
                }
                else {
                    // FetchXml = getResourceGroupFetchXml(MCS.ParticipatingSite.Site, false);
                    // LayoutXml = getResourceGroupLayout(otcCode);
                    FetchFilters = getResourceGroupFilters(MCS.ParticipatingSite.Site, false);
                }
                break;
            case "Resources By Site":
                // FetchXml = getResourceFetchXml(MCS.ParticipatingSite.Site);
                // LayoutXml = getResourceLayout(otcCode);
                DefViewId = "AADE3F9D-475B-EB11-A812-001DD8018831";
                FetchFilters = getResourceFilters(MCS.ParticipatingSite.Site);
                break;
        }

        // var customView = {
        // id: customViewId,//Some fake id
        // recordType: otcCode,//Entity Type Code of entity... yes, again
        // name: viewName,
        // fetchXml: FetchXml,
        // layoutXml: LayoutXml,
        // Type: 0//Hardcoded, leave it as it is
        // };
        // var lookupOptions = {
        // defaultViewId: customViewId,
        // allowMultiSelect: true,
        // defaultEntityType: entityName,
        // entityTypes: [entityName],
        // customViews: [customView]
        // };
        var lookupOptions = {
            defaultEntityType: entityName,
            entityTypes: [entityName],
            allowMultiSelect: true,
            disableMru: true,
            defaultViewId: DefViewId,
            //viewIds:["0D5D377B-5E7C-47B5-BAB1-A5CB8B4AC10","00000000-0000-0000-00AA-000010001003"],
            filters: [{
                filterXml: FetchFilters,
                entityLogicalName: entityName
            }]
        };

        Xrm.Utility.lookupObjects(lookupOptions).then(callback, null);
    }
};

MCS.PS_Buttons.BuildRelationshipExisting = function (selectedControl) {
    alert("Resource Filtered Search Button clicked. Still under development.");
    var editable = MCS.PS_Buttons.CheckForScheduled();
    if (editable) MCS.ParticipatingSite.LinkResource();
    else alert("Participating Site must be in a status of Can Be Scheduled = 'No' in order to add a Scheduling Resource.");
};

//Called By Ribbon Button - Add Resource
MCS.PS_Buttons.BuildRelationshipResourceBySite = function (primaryControl) {
    MCS.primaryControl = primaryControl;

    var editable = MCS.PS_Buttons.CheckForScheduled();
    if (editable) MCS.PS_Buttons.BuildRelationshipResourceRunner("mcs_resource");
    else alert("Participating Site must be in a status of Can Be Scheduled = 'No' in order to add a Scheduling Resource.");
};
//Called by Ribbon Button - Add Resource Groups
MCS.PS_Buttons.BuildRelationshipResourceGroupBySite = function (primaryControl) {
    debugger;
    MCS.primaryControl = primaryControl;

    var editable = MCS.PS_Buttons.CheckForScheduled();
    if (editable) MCS.PS_Buttons.BuildRelationshipResourceRunner("mcs_resourcegroup");
    else alert("Participating Site must be in a status of Can Be Scheduled = 'No' in order to add a Scheduling Resource.");
};
// Called by Ribbon Button - Add Users
MCS.PS_Buttons.BuildRelationshipUserBySite = function (primaryControl) {
    MCS.primaryControl = primaryControl;

    var editable = MCS.PS_Buttons.CheckForScheduled();
    if (editable) MCS.PS_Buttons.BuildRelationshipUserRunner(primaryControl);
    else alert("Participating Site must be in a status of Can Be Scheduled = 'No' in order to add a Scheduling Resource.");
};

MCS.PS_Buttons.BuildRelationshipUserRunnerCallback = function (results) {

    if ((results !== null) && (results !== undefined)) {
        var returnedItems = results;
        if (typeof (results) == "string") returnedItems = JSON.parse(results);
        if (returnedItems !== null) {
            for (i = 0; i < returnedItems.length; i++) {
                var systemUser = {
                    'cvt_name': returnedItems[i].name,
                    'cvt_schedulingresourcetype': MCS.schedulingResourceType,
                    'cvt_resourcetype': MCS.resourceType,
                    'cvt_participatingsite@odata.bind': "/cvt_participatingsites(" + MCS.ParticipatingSite.EntityId.replace('{', '').replace('}', '') + ")",
                    'cvt_user@odata.bind': "/systemusers(" + returnedItems[i].id.replace('{', '').replace('}', '') + ")"
                };

                Xrm.WebApi.createRecord("cvt_schedulingresource", systemUser).then(
                    function success(result) {
                        // when done?
                        //console.log('Resource User Created...');
                        var context = MCS.primaryControl.getFormContext();
                        context.getControl("subgrid_SchedulingResources").refresh();
                    },
                    function (error) {
                        alert(error.message);
                    });
            }
        }
    }
    else return null;
};

//HELPER - Add User by Site (Patient or Provider)
MCS.PS_Buttons.BuildRelationshipUserRunner = function () {

    //var formContext = primaryControl.getFormContext();
    MCS.schedulingResourceType = (MCS.ParticipatingSite.Side === "Patient") ? 3 : 2;
    MCS.resourceType = (MCS.ParticipatingSite.Side == "Patient") ? 100000000 : 99999999;

    var lookupItems = MCS.PS_Buttons.buildshowStandardDialog('systemuser', "Users By Site", MCS.PS_Buttons.BuildRelationshipUserRunnerCallback);
};

MCS.PS_Buttons.BuildRelationshipResourceRunnerCallback = function (results) {
    if ((results !== null) && (results !== undefined)) {
        var returnedItems = results;
        if (typeof (results) == "string") returnedItems = JSON.parse(results);
        if (returnedItems !== null) {
            //alert(returnedItems.length + " records selected.");
            for (i = 0; i < returnedItems.length; i++) {
                var ResourceId = returnedItems[i].id;
                var resourcename, type, relatedSite, relatedSiteName;
                //var entityName = returnedItems[i].typename;
                var entityName = returnedItems[i].entityType;
                Xrm.WebApi.retrieveRecord(entityName, ResourceId, "?$select=mcs_name,mcs_type").then(
                    function success(result) {
                        resourcename = result.mcs_name;
                        type = result.mcs_type;
                        relatedSite = entityName == "mcs_resourcegroup" ? result.mcs_relatedSiteId : result.mcs_RelatedSiteId;
                        ResourceId = entityName == "mcs_resourcegroup" ? result.mcs_resourcegroupid : result.mcs_resourceid;
                        var newResource = {
                            'cvt_name': resourcename,
                            'cvt_schedulingresourcetype': MCS.SchedulingResourceType,
                            'cvt_resourcetype': type,
                            'cvt_participatingsite@odata.bind': "/cvt_participatingsites(" + MCS.ParticipatingSite.EntityId.replace('{', '').replace('}', '') + ")",
                        };

                        var selectedID = ResourceId.replace('{', '').replace('}', '');

                        if (entityName == "mcs_resourcegroup") {
                            newResource['cvt_tmpresourcegroup@odata.bind'] = "/mcs_resourcegroups(" + selectedID + ")";
                            //alert("About to create GrRes SR for " + resourcename + ". ID: " + selectedID);
                        }
                        else {
                            newResource['cvt_tmpresource@odata.bind'] = "/mcs_resources(" + selectedID + ")";
                            //alert("About to create Res SR for " + resourcename + ". ID: " + selectedID);
                        }

                        Xrm.WebApi.createRecord("cvt_schedulingresource", newResource).then(
                            function success(result) {
                                //Nothing here for now. Moved the Subgrid Refresh outside of loop to only run once all resources are created.
                                console.log('Resource Created');

                                var context = MCS.primaryControl.getFormContext();
                                context.getControl("subgrid_SchedulingResources").refresh();
                            },
                            function (error) {
                                alert(error.message);
                            });
                    },
                    function (error) {
                        //alert("Failed retrieved" + MCS.cvt_Common.RestError(error));
                        var alertOptions = {
                            height: 200,
                            width: 300
                        };
                        var alertStrings = {
                            confirmButtonLabel: "OK",
                            text: "Failed retrieved" + MCS.cvt_Common.RestError(error),
                            title: "failed retrived rule."
                        };
                        Xrm.Navigation.openAlertDialog(alertStrings, alertOptions);
                    });
            }
        }
    }
    else return null;
};

//HELPER - Add Resource by Site (Patient or Provider AND Resource or Resource Group)
MCS.PS_Buttons.BuildRelationshipResourceRunner = function (entityName) {
    //Fields to make this function robust
    var siteField = 'mcs_RelatedSiteId';
    MCS.SchedulingResourceType = 1;
    MCS.title = "Resources";

    if (entityName === "mcs_resourcegroup") {
        MCS.siteField = 'mcs_relatedSiteId';
        MCS.SchedulingResourceType = 0;
        MCS.title = "Resource Groups";
    }

    var lookupItems = MCS.PS_Buttons.buildshowStandardDialog(entityName, MCS.title + " By Site", MCS.PS_Buttons.BuildRelationshipResourceRunnerCallback);
};

MCS.PS_Buttons.CheckForScheduled = function (primaryControl) {
    var isScheduleable = false;

    if (typeof primaryControl === 'undefined' || primaryControl == null) {
        isScheduleable = Xrm.Page.getAttribute('cvt_scheduleable').getValue();
    }
    else {
        var formContext = primaryControl.getFormContext();
        isScheduleable = formContext.getAttribute("cvt_scheduleable").getValue();
    }

    if (isScheduleable == true) return false;
    else return true;
};

MCS.PS_Buttons.buildSiteConditions = function (site, type) {
    debugger;
    var siteXml = '';
    switch (type) {
        case "U":
            siteXml = '<condition attribute="cvt_site" operator="in">';
            break;
        default:
            siteXml = '<condition attribute="mcs_relatedsiteid" operator="in">';
            break;
    }
    siteXml += '<value uiname="' + MCS.cvt_Common.formatXML(site[0].mcs_name) + '" uitype="mcs_site">' + site[0].id + '</value></condition>';
    //siteXml += '<value>' + site[0].id + '</value></condition>';
    return siteXml;
};

function displayIconTooltip(rowData, userLCID) {
    var str = JSON.parse(rowData);
    var coldata = str.tmp_warningtype_Value;
    var imgName = "";
    var tooltip = "";
    var warningVal = parseInt(coldata, 10);

    switch (warningVal) {
        case 917290000:
            imgName = "mcs_mvi_warning";
            tooltip = "Missing Clinic";
            break;
        //case 2:
        //    imgName = "new_Warm";
        //    switch (userLCID) {
        //        case 1036:
        //            tooltip = "French: Opportunity is Warm";
        //            break;
        //        default:
        //            tooltip = "Opportunity is Warm";
        //            break;
        //    }
        //    break;
        //case 3:
        //    imgName = "new_Cold";
        //    switch (userLCID) {
        //        case 1036:
        //            tooltip = "French: Opportunity is Cold";
        //            break;
        //        default:
        //            tooltip = "Opportunity is Cold";
        //            break;
        //    }
        //    break;
        default:
            imgName = "";
            tooltip = "";
            break;
    }
    var resultarray = [imgName, tooltip];
    return resultarray;
}

function getResourceGroupFilters(site, isAllReqd) {

    var conditions = [];

    if (isAllReqd) {
        conditions = ['<condition attribute="statecode" operator="eq" value="0"/>', '<condition attribute="mcs_type" operator="eq" value="917290000"/>', MCS.PS_Buttons.buildSiteConditions(site, "RG")];
    }
    else {
        conditions = ['<condition attribute="statecode" operator="eq" value="0"/>', MCS.PS_Buttons.buildSiteConditions(site, "RG")];
    }

    var filter = "<filter type='and'>";

    filter += conditions.join('');

    filter += "</filter>";

    return filter;

    return fetchXml;
};

//HELPER - creates the fetchXML to be used in grid views
function getResourceGroupFetchXml(site, isAllReqd) {
    var columns = ['mcs_name', 'mcs_type', 'mcs_relatedsiteid', 'modifiedon', 'createdon', 'mcs_resourcegroupid'];
    var conditions = [];

    if (isAllReqd) {
        conditions = ['<condition attribute="statecode" operator="eq" value="0"/>', '<condition attribute="mcs_type" operator="eq" value="917290000"/>', MCS.PS_Buttons.buildSiteConditions(site, "RG")];
    }
    else {
        conditions = ['<condition attribute="statecode" operator="eq" value="0"/>', MCS.PS_Buttons.buildSiteConditions(site, "RG")];
    }

    fetchXml = MCS.cvt_Common.CreateFetch('mcs_resourcegroup', columns, conditions, ['mcs_name', false]);
    return fetchXml;
};
function getResourceFilters(site) {

    var conditions = ['<condition attribute="statecode" operator="eq" value="0"/>', MCS.PS_Buttons.buildSiteConditions(site, "R")];

    var filter = "<filter type='and'>";

    filter += conditions.join('');

    filter += "</filter>";

    return filter;
};

function getResourceFetchXml(site) {

    var columns = ['mcs_resourceid', 'mcs_name', 'createdon'];

    var conditions = ['<condition attribute="statecode" operator="eq" value="0"/>', MCS.PS_Buttons.buildSiteConditions(site, "R")];

    fetchXml = MCS.cvt_Common.CreateFetch('mcs_resource', columns, conditions, ['mcs_name', false]);

    return fetchXml;
};

function getUserFilters(site) {
    var filter = '<filter type="and"><condition attribute="isdisabled" operator="eq" value="0"/><condition attribute="accessmode" operator="ne" value="3"/>';
  //  filter += MCS.PS_Buttons.buildSiteConditions(site, "U");
    filter += '</filter>';
    return filter;
};

function getUserFetchXml(site) {
    fetchXml = '<fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="false"><entity name="systemuser"><attribute name="fullname"/><attribute name="businessunitid"/><attribute name="cvt_type"/><attribute name="domainname"/><attribute name="jobtitle"/><attribute name="internalemailaddress"/><attribute name="modifiedon"/><attribute name="cvt_site"/><attribute name="cvt_facility"/><attribute name="systemuserid"/><order attribute="fullname" descending="false"/><filter type="and"><condition attribute="isdisabled" operator="eq" value="0"/><condition attribute="accessmode" operator="ne" value="3"/>';
    fetchXml += MCS.PS_Buttons.buildSiteConditions(site, "U");
    fetchXml += '</filter><link-entity name="team" from="teamid" to="cvt_primaryteam" visible="false" link-type="outer" alias="a_8f34fae9459de2118b0978e3b511a629"><attribute name="name"/></link-entity></entity></fetch>';
    return fetchXml;
};
//HELPER - sets the FetchXML view layout
function getResourceGroupLayout(otc) {
    return '<grid name="resultset" object="' + otc + '" jump="mcs_name" select="1" icon="0" preview="0"><row name="result" id="mcs_resourcegroupid"><cell name="mcs_name" width="300" /><cell name="mcs_relatedsiteid" width="300" /><cell name="mcs_type" width="150" /><cell name="modifiedon" width="125" /><cell name="createdon" width="125" /></row></grid>';
};
function getResourceLayout(otc) {
    return '<grid name="resultset" object="' + otc + '" jump="mcs_name" select="1" icon="0" preview="0"><row name="result" id="mcs_resourceid"><cell name="mcs_name" width="300" /><cell name="mcs_relatedsiteid" width="200" /><cell name="mcs_type" width="100" /><cell name="mcs_businessunitid" width="100" /><cell name="ownerid" width="150" /><cell name="modifiedon" width="125" /><cell name="createdon" width="125" /></row></grid>';
};
function getUserLayout(otc) {
    return '<grid name="resultset" object="8" jump="fullname" select="1" icon="0" preview="0"><row name="result" id="systemuserid"><cell name="fullname" width="200" /><cell name="jobtitle" width="150" /><cell name="domainname" width="200" /><cell name="internalemailaddress" width="150" /><cell name="cvt_type" width="125" /><cell name="cvt_facility" width="150" /><cell name="cvt_site" width="200" /><cell name="a_8f34fae9459de2118b0978e3b511a629.name" width="150" disableSorting="1" /><cell name="businessunitid" width="150" /><cell name="modifiedon" width="100" /></row></grid>';
};

function openDialogProcess(url, widthInput, heightInput) {
    var width = 400;
    var height = 400;
    var left = (screen.width - width) / 2;
    var top = (screen.height - height) / 2;
    return window.open(url, '', 'location=0,menubar=1,resizable=1,width=' + width + ',height=' + height + ',top=' + top + ',left=' + left + '');
};
//If the SDK namespace object is not defined, create it.
if (typeof (MCS) == "undefined") {
    MCS = {};
}
// Create Namespace container for functions in this library;
if (typeof (MCS.ParticipatingSite) == "undefined") {
    MCS.ParticipatingSite = {};
}

//Opens a window for a new TMP Resource Group record using the Information form.
MCS.ParticipatingSite.openNewmcs_resourcegroup = function (EntityId) {
    var globalContext = Xrm.Utility.getGlobalContext();
    if ((typeof Xrm !== "undefined") && (typeof Xrm.Utility !== "undefined")) {
        var p = {};
        p.mcs_relatedsiteid = MCS.ParticipatingSite.Site[0].id;
        p.mcs_relatedsiteidname = MCS.ParticipatingSite.Site[0].name;
        p.mcs_createproviderrg = true;
        p.mcs_tsaguid = EntityId;

        Xrm.Navigation.openForm("mcs_resourcegroup", null, p);
    }
    else {
        var features = "location=no,menubar=no,status=no,toolbar=no,resizable=yes";
        var extraqs = ["mcs_relatedsiteid=" + MCS.ParticipatingSite.Site[0].id, "mcs_relatedsiteidname=" + MCS.ParticipatingSite.Site[0].name, "mcs_createproviderrg=true", "mcs_tsaguid=" + EntityId]
        // var url = Xrm.Page.context.getClientUrl();
        var url = globalContext.getClientUrl();
        window.open(url + "/main.aspx?etn=mcs_resourcegroup&pagetype=entityrecord&extraqs=" + encodeURIComponent(extraqs.join("&")), "_blank", features, false);
    }
};

//Opens a window for a new TMP Resource record using the Information form.
MCS.ParticipatingSite.openNewmcs_resource = function (EntityId) {
    var globalContext = Xrm.Utility.getGlobalContext();
    if ((typeof Xrm !== "undefined") && (typeof Xrm.Utility !== "undefined")) {
        var p = {};
        p.mcs_relatedsiteid = MCS.ParticipatingSite.Site[0].id;
        p.mcs_relatedsiteidname = MCS.ParticipatingSite.Site[0].name;
        p.mcs_createproviderr = true;
        p.mcs_tsaguid = EntityId;
        Xrm.Navigation.openForm("mcs_resource", null, p);
    }
    else {
        var features = "location=no,menubar=no,status=no,toolbar=no,resizable=yes";
        var extraqs = ["mcs_relatedsiteid=" + MCS.ParticipatingSite.Site[0].id, "mcs_relatedsiteidname=" + MCS.ParticipatingSite.Site[0].name, "mcs_createproviderr=true", "mcs_tsaguid=" + EntityId]
        // var url = Xrm.Page.context.getClientUrl();
        var url = globalContext.getClientUrl();

        window.open(url + "/main.aspx?etn=mcs_resource&pagetype=entityrecord&extraqs=" + encodeURIComponent(extraqs.join("&")), "_blank", features, false);
    }
};

//Opens a window for a new Scheduling Resource record using the Information form.
MCS.ParticipatingSite.openNewcvt_schedulingresource = function () {
    var globalContext = Xrm.Utility.getGlobalContext();
    if ((typeof Xrm !== "undefined") && (typeof Xrm.Utility !== "undefined")) {
        //Checking which entity this is running on to determine the parameters to pass through.
        var p = {};
        p.cvt_siteid = MCS.ParticipatingSite.Site[0].id;
        p.cvt_siteidname = MCS.ParticipatingSite.Site[0].name;
        p.cvt_participatingsiteid = MCS.ParticipatingSite.EntityId;
        p.cvt_participatingsiteidname = MCS.ParticipatingSite.PSName;
        Xrm.Utility.openEntityForm("cvt_schedulingresource", null, p);
    }
    else {
        var features = "location=no,menubar=no,status=no,toolbar=no,resizable=yes";

        var extraqs = ["cvt_siteid=" + MCS.ParticipatingSite.Site[0].id, "cvt_siteidname=" + MCS.ParticipatingSite.Site[0].name, "cvt_participatingsiteid=" + MCS.ParticipatingSite.EntityId, "cvt_participatingsiteidname=" + MCS.ParticipatingSite.PSName];

        //var url = Xrm.Page.context.getClientUrl();
        var url = globalContext.getClientUrl();

        window.open(url + "/main.aspx?etn=cvt_schedulingresource&pagetype=entityrecord&extraqs=" + encodeURIComponent(extraqs.join("&")), "_blank", features, false);
    }
};

//Function called from the PS Ribbon Button
MCS.ParticipatingSite.QuickCreateResourceGroup = function () {
    MCS.ParticipatingSite.openNewmcs_resourcegroup(MCS.ParticipatingSite.EntityId);
};

//Function called from the PS Ribbon Button
MCS.ParticipatingSite.QuickCreateResource = function () {
    MCS.ParticipatingSite.openNewmcs_resource(MCS.ParticipatingSite.EntityId);
};

//Function called from the PS Ribbon Button
MCS.ParticipatingSite.AddResource = function () {
    MCS.ParticipatingSite.openNewcvt_schedulingresource(MCS.ParticipatingSite.EntityId, MCS.ParticipatingSite.PSName);
};

MCS.ParticipatingSite.LinkResource = function () {
    //if (MCS.mcs_Patient_Resource.GroupAppt == 0)
    MCS.ParticipatingSite.openNewcvt_schedulingresource();
    //else
    //    MCS.ParticipatingSite.openNewcvt_patientresourcegroupGroupAppt(1, MCS.mcs_Patient_Resource.EntityId, MCS.mcs_Patient_Resource.TSAName);
};