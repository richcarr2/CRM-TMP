//Library Name: mcs_TSA.OnChange.js
if (typeof MCS == "undefined")
    MCS = {};
// Create Namespace container for functions in this library;
MCS.mcs_TSA_OnChange = {};

//Called from StatusReason
MCS.mcs_TSA_OnChange.ProductionLock = function (executionContext) {
    var formContext = executionContext.getFormContext();
    var status = formContext.getAttribute("statuscode").getValue();
    var isProduction = (status == 251920000) ? true : false;
    //Draft or Revision
    var notEditable = (status != 1 && status != 917290007) ? true : false;
    var groupApptOption = (formContext.getAttribute("cvt_groupappointment") != null) ? formContext.getAttribute("cvt_groupappointment").getValue() : false;
    var label = (groupApptOption == true) ? "Group Appointment " : "";

    label += (isProduction == true) ? "TSA(Production Lock) ~ Save in Draft to Edit" : "TSA-Draft";

    formContext.ui.tabs.get("tab_Info").setLabel(label);
    var alwaysDisable = [
        'cvt_relatedprovidersiteid',
        'cvt_servicetype',
        'cvt_servicesubtype',
        'cvt_groupappointment',
        'cvt_provistaclinic',
        'cvt_patvistaclinic',
        'cvt_servicescope',
        'cvt_availabletelehealthmodalities'
    ];

    var disabledFields = [
        'cvt_servicelevels',
        'cvt_relatedpatientsiteid',
        'ownerid',
        'cvt_relatedpatientsiteid',
        'cvt_patientfacility',
        'cvt_duration',
        'cvt_startevery'
    ];

    for (fields in alwaysDisable) {
        formContext.getControl(alwaysDisable[fields]).setDisabled(true);
    }
    for (fields in disabledFields) {
        formContext.getControl(disabledFields[fields]).setDisabled(notEditable);
    }
    if (!notEditable) {
        MCS.mcs_TSA_OnChange.GroupAppt(executionContext);
        MCS.mcs_TSA_OnChange.ChangeType(executionContext);
    }

    if (notEditable && formContext.getAttribute('cvt_duration').getValue() == null)
        formContext.getAttribute("cvt_duration").setRequiredLevel("none");
    else
        formContext.getAttribute("cvt_duration").setRequiredLevel("required");

    if (notEditable && formContext.getAttribute('cvt_startevery').getValue() == null)
        formContext.getAttribute("cvt_startevery").setRequiredLevel("none");
    else
        formContext.getAttribute("cvt_startevery").setRequiredLevel("required");
};

MCS.mcs_TSA_OnChange.GroupAppt = function (executionContext) {
    var formContext = executionContext.getFormContext();
    var isHomeMobile = formContext.getAttribute("cvt_type").getValue();
    var patSite = formContext.getAttribute("cvt_relatedpatientsiteid");
    var patSiteControl = formContext.getControl("cvt_relatedpatientsiteid");
    var patFac = formContext.getAttribute("cvt_patientfacility");
    var patFacControl = formContext.getControl("cvt_patientfacility");
    var status = formContext.getAttribute("statuscode").getValue();
    var groupApptOptionValue = (formContext.getAttribute("cvt_groupappointment") != null) ? formContext.getAttribute("cvt_groupappointment").getValue() : null;
    var label = "";
    if (isHomeMobile) {
        patSite.setRequiredLevel("none");
        patSite.setValue(null);
        patSiteControl.setVisible(false);
        patFac.setRequiredLevel("none");
        patFacControl.setDisabled(true);
        label += "VA Video Connect ";
        if (groupApptOptionValue) {
            if (status == 251920000)
                label += "Group Appointment TSA(Production Lock) ~ Save in Draft to Edit";
            else
                label += "Group Appointment TSA-Draft";
        }
        else {
            if (status == 251920000)
                label += 'TSA(Production Lock) ~ Save in Draft to Edit';
            else
                label += 'TSA-Draft';
        }

    }
    else {
        if (groupApptOptionValue == 1) {
            patSite.setRequiredLevel("none");
            patSite.setValue(null);
            patSiteControl.setVisible(false);
            if (!isHomeMobile) {
                patFacControl.setDisabled(false);
                patFac.setRequiredLevel("required");
            }

            if (status == 251920000)
                label = 'Group Appointment TSA(Production Lock) ~ Save in Draft to Edit';
            else
                label = 'Group Appointment TSA-Draft';
        }
        else {
            patSiteControl.setVisible(true);
            patSite.setRequiredLevel("required");
            patFacControl.setDisabled(true);
            patFac.setRequiredLevel("none");
            //formContext.getControl("mcs_capacity").setDisabled(true);
            if (status == 251920000)
                label = 'TSA(Production Lock) ~ Save in Draft to Edit';
            else
                label = 'TSA-Draft';
        }
        //initialstatus.setValue(4);
        formContext.ui.tabs.get("tab_Info").setLabel(label);
    }
};

MCS.mcs_TSA_OnChange.StoreForward = function (executionContext) {
    var formContext = executionContext.getFormContext();
    //Check if this TSA is store forward
    var SFT = formContext.getAttribute("cvt_availabletelehealthmodalities").getValue() == 917290001;
    formContext.getControl("cvt_groupappointment").setVisible(!SFT);
};

MCS.mcs_TSA_OnChange.ChangeType = function (executionContext) {
    var formContext = executionContext.getFormContext();
    if (formContext.ui.getFormType() == MCS.cvt_Common.FORM_TYPE_CREATE) {
        formContext.getAttribute("cvt_servicescope").setValue(917290001);
        formContext.getAttribute("cvt_servicescope").setSubmitMode("always");
    }
    formContext.getAttribute("cvt_type").setSubmitMode("always");

    if (formContext.getAttribute("cvt_type").getValue() == true) { //CVT to Home
        formContext.getControl("cvt_groupappointment").setVisible(true);
        formContext.getControl("cvt_servicescope").setDisabled(true);

        formContext.getControl("cvt_relatedpatientsiteid").setVisible(false);
        formContext.getControl("cvt_patientfacility").setVisible(false);
        formContext.getAttribute("cvt_relatedpatientsiteid").setRequiredLevel("none");

        formContext.ui.tabs.get("tab_Pat").setVisible(false);
    }
};

MCS.mcs_TSA_OnChange.setInterIntraFacility = function (executionContext) {
    var formContext = executionContext.getFormContext();
    var patSiteObj = formContext.getAttribute("cvt_relatedpatientsiteid");
    var patFacObj = formContext.getAttribute("cvt_patientfacility");
    var serviceScope = formContext.getAttribute("cvt_servicescope");
    //Prov site will always be on the form, so go ahead and getValue, Pat Site or Pat Facility will be on form, don't get both
    var provSite = formContext.getAttribute("cvt_relatedprovidersiteid").getValue();

    if (patSiteObj != null)
        patSiteObj = patSiteObj.getValue();
    if (patFacObj != null)
        patFacObj = patFacObj.getValue();

    //determine if the form is displaying facility or site for patients (based on group or individual)
    var facOrSite = patSiteObj == null ? "facility" : "site";
    //if either pat site/facility or provider site are not populated, do nothing, otherwise, set intrafacility or interfacility for the user
    if (provSite == null || ((facOrSite == "site" && patSiteObj == null) || (facOrSite == "facility" && patFacObj == null)))
        return;
    else {
        //Query Facility for both records and match them
        var provFacilityId = MCS.mcs_TSA_OnChange.QueryFacility(provSite[0].id).Id;
        var patFacilityId = facOrSite == "facility" ? patFacObj[0].id : MCS.mcs_TSA_OnChange.QueryFacility(patSiteObj[0].id).Id;

        //Clean both guids for the compare
        if (MCS.cvt_Common.compareGUIDS(patFacilityId, provFacilityId) == true) { //if facility for patient and provider site are the same, then set service scope to intra-facility
            if (serviceScope.getValue() != 917290001)
                serviceScope.setValue(917290001);
        }
        else { // if facility for patient and provider site are different, then set service scope to inter-facility
            if (serviceScope.getValue() != 917290000)
                serviceScope.setValue(917290000);
        }
        serviceScope.setSubmitMode("always");
    }
};

MCS.mcs_TSA_OnChange.QueryFacility = function (siteID) {
    var facilityId;
    Xrm.WebApi.retrieveRecord("mcs_site", siteID, "?$select=mcs_FacilityId").then(
        function success(result) {
            if (result) {
                facilityId = result.mcs_FacilityId;
            }
        },
        function (error) {
        }
    );
    //var call = CrmRestKit.Retrieve('mcs_site', siteID, ['mcs_FacilityId'], false);
    //call.fail(
    //    function (error) {
    //        return;
    //    }).done(function (site) {
    //        if (site && site.d) {
    //            facilityId = site.d.mcs_FacilityId;
    //        }
    //    });
    return facilityId;
};

//Specialty
MCS.mcs_TSA_OnChange.Specialty = function (executionContext) {
    var formContext = executionContext.getFormContext();
    //See if any sub-types exist
    if (formContext.getAttribute("cvt_servicetype").getValue() != null) {
        Xrm.WebApi.retrieveMultipleRecords("mcs_servicesubtype", "?$select=mcs_name&$filter=cvt_relatedServiceTypeId/Id eq guid" + formContext.getAttribute("cvt_servicetype").getValue()[0].id).then
            (
            function success(result) {
                if (result != null && result.entities.length != 0) {
                    formContext.getControl("cvt_servicesubtype").setVisible(true);
                    formContext.getControl("cvt_servicesubtype").setFocus();
                }
                else {
                    formContext.getControl("cvt_servicesubtype").setVisible(false);
                    formContext.getAttribute("cvt_servicesubtype").setValue(null);
                }
                MCS.mcs_TSA_OnLoad.SOS();
            },
            function (error) {
                formContext.getControl("cvt_servicesubtype").setVisible(false);
                formContext.getAttribute("cvt_servicesubtype").setValue(null);
            });

        //calls = CrmRestKit.ByQuery("mcs_servicesubtype", ['mcs_name'], "cvt_relatedServiceTypeId/Id eq guid'" + formContext.getAttribute("cvt_servicetype").getValue()[0].id + "'", false);
        //calls.fail(function (err) {
        //    formContext.getControl("cvt_servicesubtype").setVisible(false);
        //    formContext.getAttribute("cvt_servicesubtype").setValue(null);
        //}).done(function (data) {
        //    if (data && data.d && data.d.results != null && data.d.results.length != 0) {
        //        formContext.getControl("cvt_servicesubtype").setVisible(true);
        //        formContext.getControl("cvt_servicesubtype").setFocus();
        //    }
        //    else {
        //        formContext.getControl("cvt_servicesubtype").setVisible(false);
        //        formContext.getAttribute("cvt_servicesubtype").setValue(null);
        //    }
        //    MCS.mcs_TSA_OnLoad.SOS();
        //});
    }
    else {
        formContext.getControl("cvt_servicesubtype").setVisible(false);
        formContext.getAttribute("cvt_servicesubtype").setValue(null);
    }
};

//Provider Site TCT Team
MCS.mcs_TSA_OnChange.TCTTeamPro = function (executionContext) {
    var formContext = executionContext.getFormContext();
    //See if any sub-types exist
    if (formContext.getAttribute("cvt_relatedprovidersiteid").getValue() != null) {
        Xrm.WebApi.retrieveMultipleRecords("Team", "?$select=Name,TeamId&$filter=cvt_Type/Value eq 917290007 and cvt_TMPSite/Id eq" + formContext.getAttribute("cvt_relatedprovidersiteid").getValue()[0].id).then(
            function success(result) {
                if (result != null && result.entities.length != 0) {
                    //Set the TCT Team
                    var teamObj = new Array();
                    teamObj[0] = new Object();
                    teamObj[0].id = result.entities[0].TeamId;
                    teamObj[0].name = result.entities[0].Name;
                    teamObj[0].entityType = 'Team';

                    formContext.getAttribute("cvt_providersitetctteam").setValue(teamObj)
                }
                else
                    formContext.getAttribute("cvt_providersitetctteam").setValue(null);
            },
            function (error) {
            });
        //calls = CrmRestKit.ByQuery("Team", ['Name', 'TeamId'], "cvt_Type/Value eq 917290007 and cvt_TMPSite/Id eq (Guid'" + formContext.getAttribute("cvt_relatedprovidersiteid").getValue()[0].id + "')", false);
        //calls.fail(function (err) {
        //    //Fail
        //}).done(function (data) {
        //    if (data && data.d && data.d.results != null && data.d.results.length != 0) {
        //        //Set the TCT Team
        //        var teamObj = new Array();
        //        teamObj[0] = new Object();
        //        teamObj[0].id = data.d.results[0].TeamId;
        //        teamObj[0].name = data.d.results[0].Name;
        //        teamObj[0].entityType = 'Team';

        //        formContext.getAttribute("cvt_providersitetctteam").setValue(teamObj)
        //    }
        //    else
        //        formContext.getAttribute("cvt_providersitetctteam").setValue(null);
        //});
    }
    else
        formContext.getAttribute("cvt_providersitetctteam").setValue(null);
};
//Patient Site TCT Team
MCS.mcs_TSA_OnChange.TCTTeamPat = function executionContext() {
    var formContext = executionContext.getFormContext();
    //See if any sub-types exist
    if (formContext.getAttribute("cvt_relatedpatientsiteid").getValue() != null) {
        Xrm.WebApi.retrieveMultipleRecords("Team", "?$select=Name,TeamId&$filter=cvt_Type/Value eq 917290007 and cvt_TMPSite/Id eq (Guid'" + formContext.getAttribute("cvt_relatedpatientsiteid").getValue()[0].id + "')").then(
            function success(result) {
                if (result != null && result.entities.length != 0) {
                    //Set the TCT Team
                    var teamObj = new Array();
                    teamObj[0] = new Object();
                    teamObj[0].id = data.d.results[0].TeamId;
                    teamObj[0].name = data.d.results[0].Name;
                    teamObj[0].entityType = 'Team';

                    formContext.getAttribute("cvt_patientsitetctteam").setValue(teamObj)
                }
                else
                    formContext.getAttribute("cvt_patientsitetctteam").setValue(null);
            },
            function (error) { });
        //calls = CrmRestKit.ByQuery("Team", ['Name', 'TeamId'], "cvt_Type/Value eq 917290007 and cvt_TMPSite/Id eq (Guid'" + formContext.getAttribute("cvt_relatedpatientsiteid").getValue()[0].id + "')", false);
        //calls.fail(function (err) {
        //    //Fail
        //}).done(function (data) {
        //    if (data && data.d && data.d.results != null && data.d.results.length != 0) {
        //        //Set the TCT Team
        //        var teamObj = new Array();
        //        teamObj[0] = new Object();
        //        teamObj[0].id = data.d.results[0].TeamId;
        //        teamObj[0].name = data.d.results[0].Name;
        //        teamObj[0].entityType = 'Team';

        //        formContext.getAttribute("cvt_patientsitetctteam").setValue(teamObj)
        //    }
        //    else
        //        formContext.getAttribute("cvt_patientsitetctteam").setValue(null);
        //});
    }
    else
        formContext.getAttribute("cvt_patientsitetctteam").setValue(null);
};