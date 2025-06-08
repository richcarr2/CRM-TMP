//If the MCS namespace object is not defined, create it.
//SD: web-use-strict-equality-operators
if (typeof MCS === "undefined") MCS = {};

MCS.User = {};

//Enable User own edit.
MCS.User.CheckEdit = function (executionContext) {
    //SD - Start
    //web-use-strict-mode
    "use strict";
    //SD - End
    var formContext = executionContext.getFormContext();
    //if form is update
    //SD: web-use-strict-equality-operators
    if (formContext.ui.getFormType() !== 1) {
        //Check if user has TMP Resource Manager role
        var isUserRM = MCS.cvt_Common.userHasRoleInList("TMP Resource Manager");
        var isSysAdmin = MCS.cvt_Common.userHasRoleInList("System Administrator");
        var isAppAdmin = MCS.cvt_Common.userHasRoleInList("TMP Application Administrator");
        var isFieldAppAdmin = MCS.cvt_Common.userHasRoleInList("TMP Field Application Administrator");

        var canEdit = isSysAdmin || isAppAdmin || isFieldAppAdmin || isUserRM;

        if (!canEdit) {
            //If no, then read only everything
            formContext.ui.controls.forEach(function (control, index) {
                try {
                    control.setDisabled(true);
                }
                catch(e) {}
            });

            //check if user is on user's own record
            var currentUserId = formContext.context.getUserId();
            var currentRecordId = formContext.data.entity.getId();
            //SD: web-use-strict-equality-operators
            var isCurrentUsersRecord = currentUserId === currentRecordId;

            //if yes, unlock provider preference field
            if (isCurrentUsersRecord) {
                if (formContext.getControl("cvt_tsaproviderpreferences") != null) formContext.getControl("cvt_tsaproviderpreferences").setDisabled(false);
            }
        }
        //If yes, do not do anything.
    }
};
//Check for Location alignment
MCS.User.SetLocationRequirement = function (executionContext) {
    var formContext = executionContext.getFormContext();
    //Check if Type is VISN Lead or Other/Support
    //SD: web-use-strict-equality-operators
    if (formContext.getAttribute('cvt_type').getValue() != null && (formContext.getAttribute('cvt_type').getValue() === 917290006 || formContext.getAttribute('cvt_type').getValue() === 917290002)) {
        //Make Facility and TMP Site not required
        formContext.getAttribute('cvt_facility').setRequiredLevel("none");
        formContext.getAttribute('cvt_site').setRequiredLevel("none");
    }
    else {
        formContext.getAttribute('cvt_facility').setRequiredLevel("required");
        formContext.getAttribute('cvt_site').setRequiredLevel("required");
    }
};
//Displays different tabs & Fields based on field values selected.
MCS.User.SetSubmit = function (executionContext) {
    var formContext = executionContext.getFormContext();
    //SD: web-use-strict-equality-operators
    if (formContext.getAttribute("cvt_deleteuserconnections").getIsDirty() === true) formContext.getAttribute("cvt_deleteuserconnections").setSubmitMode("always");

    //SD: web-use-strict-equality-operators
    if (formContext.getAttribute("cvt_updateuserconnections").getIsDirty() === true) formContext.getAttribute("cvt_updateuserconnections").setSubmitMode("always");

    //SD: web-use-strict-equality-operators
    if (formContext.getAttribute("cvt_replacementuser").getIsDirty() === true) formContext.getAttribute("cvt_replacementuser").setSubmitMode("always");
};

//Set Provider Preference field with default text
MCS.User.DefaultProviderPreferences = function (executionContext) {
    var formContext = executionContext.getFormContext();

    var defaultpreftext = "The Provider visit preferences help your virtual team to understand your expectations in the provision of care using telehealth. It defines the responsibilities and procedures involved in a remote clinical visit. Below is standard guidance you may want to provide to your team. You can edit this list as you feel fits your type of visits. \n";

    //Preferred methods of contact
    defaultpreftext += "\nA. Provider's preferred methods of communication and contact information: \n";
    defaultpreftext += "  1. For questions/issues that may not need an appointment (pre- or post-telehealth visit): \n";
    defaultpreftext += "  2. For questions while the visit is in progress about the care or recommendation given by the provider/consultant: \n";
    defaultpreftext += "  3. For immediate needs/urgent care situations pre- or post-telehealth visit (e.g., critical lab values or time-dependent diagnoses): \n";
    defaultpreftext += "  4. For last-minute clinic/patient cancellations: \n";
    defaultpreftext += "  5.Other (specify): \n";

    //Clinical information
    defaultpreftext += "\nB. Clinical information you will routinely require and that the virtual team needs to ensure was completed based on your orders: \n";
    defaultpreftext += "  1. Specific clinical history: \n";
    defaultpreftext += "  2. Labs: \n";
    defaultpreftext += "  3. Imaging: \n";
    defaultpreftext += "  4. Studies: \n";
    defaultpreftext += "  5. Screenings/vital signs: \n";
    defaultpreftext += "  6. Other: \n";

    //Clinical scheduling
    defaultpreftext += "\nC. Clinical scheduling (CVT or SFT, as applicable): \n";
    defaultpreftext += "  1. Days/times clinic will be held: \n";
    defaultpreftext += "  2. Length of new (initial) patient appointment: \n";
    defaultpreftext += "  3. Length of established (returning) patient appointment (in minutes):  \n";

    //Technology requirements
    defaultpreftext += "\nD. Telehealth technology requirements (Clinic, Non-VA, Home): \n";
    defaultpreftext += "  1. Provider: \n";
    defaultpreftext += "    a. PC/Laptop: \n";
    defaultpreftext += "    b. Web camera: \n";
    defaultpreftext += "    c. Desktop codec: \n";
    defaultpreftext += "    d. Other: \n";
    defaultpreftext += "  2. Patient (Clinic-Based): \n";
    defaultpreftext += "    a. Clinical Cart: \n";
    defaultpreftext += "    b. Stethoscope: \n";
    defaultpreftext += "    c. Exam Camera: \n";
    defaultpreftext += "    d. Otoscope: \n";
    defaultpreftext += "    e. Simple Video Monitor: \n";

    //TC vs. TM
    defaultpreftext += "\nE. Teleconsultation vs. Telemedicine: \n";

    //TP skill
    defaultpreftext += "\nF. Telepresenter/Telehealth staff skill level and requirements to support clinical service being provided: \n";

    //Patient safety
    defaultpreftext += "\nG. Patient Safety:  \n";
    defaultpreftext += "  Requirements for service-specific emergency and disaster plan should include a patient medical or behavioral emergency and how to manage during a telehealth visit, as well as what actions/activities are expected to be performed by the provider site staff and the patient site staff. The virtual team should know the location of emergency and disaster plan/emergency procedure. There should be predetermined dates and times for practice drills. \n";

    //Critical labs
    defaultpreftext += "\nH. Critical Lab and Abnormal Finding Notifications: \n";
    defaultpreftext += "  Frequency that the telehealth provider (or surrogate) logs in to Patient Site CPRS to receive/view/respond to notifications based on the site's critical value policy. Provide your guidance here: \n";

    //Record info
    defaultpreftext += "\nI. The appropriate use of telehealth for each individual patient should be noted in the patient's record and expected discharge if episodic care. Also an initial verbal consent should be annotated in the patient record. \n";

    var providerPreference = formContext.getAttribute("cvt_tsaproviderpreferences");
    var userType = formContext.getAttribute("cvt_type").getValue();

    //SD: web-use-strict-equality-operators
    if (userType === 917290001) {
        formContext.ui.tabs.get('SUMMARY_TAB').sections.get('ProviderPreferences').setVisible(true);

        //SD: web-use-strict-equality-operators
        if ((providerPreference.getValue() === "") || (providerPreference.getValue() === null)) providerPreference.setValue(defaultpreftext);
    }
    else formContext.ui.tabs.get('SUMMARY_TAB').sections.get('ProviderPreferences').setVisible(false);
};

//Check for Location alignment
MCS.User.VerifyLocations = function (executionContext) {
    var formContext = executionContext.getFormContext();
    var visn = "";
    var facility = "";
    var site = "";
    var event = executionContext.getEventArgs();

    if (formContext.getAttribute('businessunitid').getValue() != null) visn = formContext.getAttribute('businessunitid').getValue()[0].id;
    else {
        alert("Please select a VISN.");
        event.preventDefault();
        return;
    }
    //Check if Type is VISN Lead or Other/Support
    //SD: web-use-strict-equality-operators
    if (formContext.getAttribute('cvt_type').getValue() != null && (formContext.getAttribute('cvt_type').getValue() === 917290006 || formContext.getAttribute('cvt_type').getValue() === 917290002)) {
        return;
    }
    if (formContext.getAttribute('cvt_facility').getValue() != null) facility = formContext.getAttribute('cvt_facility').getValue()[0].id;
    else {
        alert("Please select a Facility.");
        event.preventDefault();
        return;
    }
    if (formContext.getAttribute('cvt_site').getValue() != null) site = formContext.getAttribute('cvt_site').getValue()[0].id;
    else {
        alert("Please select a TMP Site.");
        event.preventDefault();
        return;
    }
    //Make two query
    //Verify that TMP Site's (1)Facility and (2)BU match
    Xrm.WebApi.retrieveRecord("mcs_site", site, "?$select=mcs_FacilityId,mcs_BusinessUnitId").then(
    function success(result) {
        if (result) {
            //Check Facility
            //SD: web-use-strict-equality-operators
            if (MCS.cvt_Common.compareGUIDS(facility, result.mcs_FacilityId.Id) === false) {
                alert("Save prevented. Please make sure the TMP Site, from " + result.mcs_FacilityId.Name + ", belongs to the Facility listed on the form: " + formContext.getAttribute('cvt_facility').getValue()[0].name);
                event.preventDefault();
                return;
            }
            Xrm.WebApi.retrieveRecord("mcs_facility", result.mcs_FacilityId.Id, "?$select=mcs_BusinessUnitId").then(
            function success(result1) {
                if (result1) {
                    //Check BU
                    //SD: web-use-strict-equality-operators
                    if (MCS.cvt_Common.compareGUIDS(visn, result1.mcs_BusinessUnitId.Id) === false) {
                        alert("Save prevented. Please make sure the Facility, from " + result1.mcs_BusinessUnitId.Name + ", belongs to the Business Unit listed on the form: " + formContext.getAttribute('businessunitid').getValue()[0].name);
                        event.preventDefault();
                        return;
                    }
                }
            },
            function (error) {
                //SD: web-remove-debug-script
                //console.log(error.message);
                // handle error conditions
            });
        }
    },
    function (error) {
        //SD: web-remove-debug-script
        //console.log(error.message);
        // handle error conditions
    });

    //var calls = CrmRestKit.Retrieve("mcs_site", site, ['mcs_FacilityId', 'mcs_BusinessUnitId'], false);
    //calls.fail(
    //    function (error) {
    //    }).done(function (data) {
    //        if (data && data.d) {
    //            //Check Facility
    //            if (MCS.cvt_Common.compareGUIDS(facility, data.d.mcs_FacilityId.Id) == false) {
    //                alert("Save prevented. Please make sure the TMP Site, from " + data.d.mcs_FacilityId.Name + ", belongs to the Facility listed on the form: " + Xrm.Page.getAttribute('cvt_facility').getValue()[0].name);
    //                event.preventDefault();
    //                return;
    //            }
    //            var callsFacility = CrmRestKit.Retrieve("mcs_facility", data.d.mcs_FacilityId.Id, ['mcs_BusinessUnitId'], false);
    //            callsFacility.fail(
    //                function (error) {
    //                }).done(function (result) {
    //                    if (result && result.d) {
    //                        //Check BU
    //                        if (MCS.cvt_Common.compareGUIDS(visn, result.d.mcs_BusinessUnitId.Id) == false) {
    //                            alert("Save prevented. Please make sure the Facility, from " + result.d.mcs_BusinessUnitId.Name + ", belongs to the Business Unit listed on the form: " + Xrm.Page.getAttribute('businessunitid').getValue()[0].name);
    //                            event.preventDefault();
    //                            return;
    //                        }
    //                    }
    //                });
    //        }
    //    });
};

//Check for Location alignment
MCS.User.onLoad = function (executionContext) {
    var formContext = executionContext.getFormContext();
    //Check if Type is Clinician/Provider, TCT/Staff, Telepresenter
    //SD: web-use-strict-equality-operators
    if (formContext.getAttribute('cvt_type').getValue() != null && (formContext.getAttribute('cvt_type').getValue() === 917290001 || formContext.getAttribute('cvt_type').getValue() === 917290005 || formContext.getAttribute('cvt_type').getValue() === 917290000)) {

        //Show field
        formContext.getControl('cvt_staticvmrlink').setVisible(true);
    }
    else {
        //Hide field and clear value
        formContext.getControl('cvt_staticvmrlink').setVisible(false);
        formContext.getAttribute('cvt_staticvmrlink').setValue(null);
    }
    MCS.User.IsUserAdmin(formContext);
};

MCS.User.ReadOnlyEmailCheck = function (formContext, canEditEmail) {
    //Only System Admins and TMP App Admins should be able to edit the Primary Email field
    //the field should be read-only for all other users.
    if (!canEditEmail) {
        //if no, make the Primary Email field read-only
        console.log("User cannot edit the email field. Making the field read only...");
        formContext.getControl("internalemailaddress").setDisabled(true);
    }
};

MCS.User.IsUserAdmin = function (formContext) {
    var userId = Xrm.Utility.getGlobalContext().userSettings.userId;
    userId = userId.replace("{", "");
    userId = userId.replace("}", "");

    //user the user's ID to see if they have the TMP App Admin or Sys Admin security roles
    var fetchXml = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>";
    fetchXml += "<entity name='systemuser'>";
    fetchXml += "<attribute name='systemuserid'/>";
    fetchXml += "<filter type='and'>";
    fetchXml += "<condition attribute='systemuserid' operator='eq' value='" + userId + "' />";
    fetchXml += "</filter>";
    fetchXml += "<link-entity name='systemuserroles' from='systemuserid' to='systemuserid' intersect='true'>";
    fetchXml += "<link-entity name='role' from='roleid' to='roleid'>";
    fetchXml += "<filter type='or'>";
    fetchXml += "<condition attribute='name' operator='eq' value='TMP Application Administrator'/>";
    fetchXml += "<condition attribute='name' operator='eq' value='System Administrator'/>";
    fetchXml += "</filter>";
    fetchXml += "</link-entity>";
    fetchXml += "</link-entity>";
    fetchXml += "</entity>";
    fetchXml += "</fetch>";
    console.log(fetchXml);

    fetchXml = "?fetchXml=" + encodeURIComponent(fetchXml);
    console.log(fetchXml);
    Xrm.WebApi.retrieveMultipleRecords("systemuser", fetchXml).then(
        function success(result) {
            var countValue = result.entities.length;
            console.log(countValue);
            if (countValue > 0) { //user is a TMP App Admin or a Sys Admin
                MCS.User.ReadOnlyEmailCheck(formContext, true)
            }
            else {
                MCS.User.ReadOnlyEmailCheck(formContext, false);
            }
        },
        function (error) {
            console.log("Error accessing user roles:");
            console.log(error);
            //if an error is thrown, make the email field read only
            //since we can't verify the user's role
            MCS.User.ReadOnlyEmailCheck(formContext, false);
        },        
    );
};