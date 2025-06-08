
var counter = 0;
var newVODId = null;
var globalProvId = null;
var invalidEmail = "";

function requestVOD() {
    //SD - Start
    //web-use-strict-mode
    "use strict";
    //SD - End
    //Disable the buttons
    toggleIcon(false);

    //Validate the Patient E-mail field exists
    var patientEmail = $("#PatientEmailAddressTextBox").val();
    var checkValidEmail = ValidateEmail(patientEmail);

    //SD: web-use-strict-equality-operators
    if ((invalidEmail !== patientEmail && checkValidEmail === false) || patientEmail.indexOf("@") === -1) {
        invalidEmail = patientEmail;
        hideAll();

        var message = "A potentially invalid character or format was detected for ' " + patientEmail + " '.  If you would like to continue using this email address, please click the REQUEST ON DEMAND VVC button again to continue with this email.  Please note that if this email address format is incorrect, it may not work properly.";

        //SD: web-use-strict-equality-operators
        if (patientEmail.indexOf("@") === -1) message = "Invalid email format detected for: ' " + patientEmail + " '.  A valid email must contain '@' symbol.  Please correct the email and click the REQUEST ON DEMAND VVC button again.";

        $("#validationErrors").text("");
        $("#validationErrors").append(message);
        $("#msgBoxFailedValidation").show();
        toggleIcon(true);
        return;
    }
    else invalidEmail = "";

    if (globalProvId == null) {
        alert("No Provider selected, please select one before requesting a VOD.");
        return;
    }
    //Reset Counter
    counter = 0;

    //strip out {} from globalProvId
    globalProvId = globalProvId.replace(/{|}/g, '');

    //Create a cvt_VOD record
    var newVOD = {
        'cvt_name': patientEmail,
        'cvt_provider@odata.bind': '/systemusers(' + globalProvId + ')',
        'cvt_patientemail': patientEmail
    };

    Xrm.WebApi.online.createRecord('cvt_vod', newVOD).then(
    function success(result) {
        newVODId = result.id;
        waitForUpdate();
    },
    function (error) {
        console.log('error here');
    });
}

function manualCheckForVMR() {
    hideAll();
    $("#msgBoxWorking").show();
    $("#msgBoxWorking").focus();
    counter = 19;

    setTimeout(function () {
        waitForUpdate();
    },
    500);
}

function refreshForm() {
    $("#PatientEmailAddressTextBox").val("");
    toggleIcon(true);
    hideAll();
    $("#PatientEmailAddressTextBox").focus();
}

function waitForUpdate() {
    Xrm.WebApi.retrieveRecord("cvt_vod", newVODId, "?$select=statuscode").then(
    function success(result) {
        hideAll();

        //Requested
        switch (result.statuscode) {
        case 1:
            //Requested
            showExecutingSearch();
            counter++;
            if (counter < 20) {
                setTimeout(function () {
                    waitForUpdate();
                },
                5000);
            }
            else {
                //Show error message
                $("#noResultText").text("");
                $("#noResultText").append("The VMR is still being generated.");
                hideAll();
                $("#msgBoxNoResult").show();
                return;
            }
            break;
        case 917290000:
            //Success
            $("#msgBoxSuccess").show();
            break;
        case 917290001:
            //Failure
            $("#errorText").text("");
            //Find the Integration Result
            // TODO: Boris - Update to use WebAPI RetrieveMultiple
            var filter = "cvt_vod/Id eq (Guid'" + newVODId + "')";
            Xrm.WebApi.retrieveMultipleRecords("TeamMembership", "?$select=TeamId&$filter=" + filter).then(
            function success(results) {
                if (results && results.entities && results.entities.length > 0) {
                    var integrationError = results.entities[0].mcs_error;
                    $("#errorText").append(" Error: " + integrationError);
                    $("#errorText").append("<br/><br/>Please try again.");
                    $("#RequestButton").attr('disabled', false);
                    toggleIcon(true);
                }
            },
            function (error) {
                console.log(error);
                return;
            });

            $("#msgBoxFailedIntegration").show();
            break;
        }
    },
    function (error) {
        console.log('Error in waitForUpdate: ' + error);
        return;
    });
}

//Show Executing Search
function showExecutingSearch() {
    hideAll();
    toggleIcon(false);

    $("#msgBoxWorking").show();
    $("#msgBoxWorking").focus();
}

//Hide all Message Divs
function hideAll() {
    $("#msgBoxWorking").hide();
    $("#msgBoxFailedIntegration").hide();
    $("#msgBoxFailedValidation").hide();
    $("#msgBoxAccessDenied").hide();
    $("#msgBoxNoResult").hide();
    $("#msgBoxSuccess").hide();
}

function hideEverything() {
    hideAll();
    $("#searchArea").hide();
    $("#buttons").hide();
}

function toggleIcon(status) {
    $("#ProviderLookupIcon").attr('onclick', status ? 'openProviderLookup(this)' : '');
    $("#ProviderLookupIcon").attr('style', status ? 'cursor: pointer;' : '');
    $("#PatientEmailAddressTextBox").attr('disabled', !status);
    $("#RequestButton").attr('disabled', !status);
}

function ValidateEmail(mail) {
    if (/^\w+([\.-]?\w+)*@\w+([\.-]?\w+)*(\.\w{2,3})+$/.test(mail)) {
        if (mail == "" || mail == null) return false;
        return true;
    }
    return false;
}

function getCurrentUserInformation() {
    if (typeof(Xrm) == "undefined") {
        Xrm = parent.Xrm;
    }
    if (typeof(Xrm) == "undefined") {
        Xrm = window.parent.Xrm;
    }

    var context = Xrm.Utility.getGlobalContext();
    var currentUserId = context.userSettings.userId;

    currentUserId = currentUserId.replace("{", "");
    currentUserId = currentUserId.replace("}", "");

    ValidateActiveSettingSwitch(currentUserId);
}

function openProviderLookup(o) {
    var globalContext = Xrm.Utility.getGlobalContext();
    var URL = globalContext.getClientUrl();

    if (URL.match(/\/$/)) {
        URL = URL.substring(0, URL.length - 1);
    }

    //Changed url to be dynamic based on function which looks up the OTC of the specificed entity.
    URL += "/_controls/lookup/lookupinfo.aspx?LookupStyle=multi&browse=0&objecttypes=8"; //Set OTC as appropriate to relationship;
    var oUrl = Mscrm.CrmUri.create("/_controls/lookup/lookupinfo.aspx");
    oUrl.get_query()["LookupStyle"] = "single";
    oUrl.get_query()["browse"] = "0";
    oUrl.get_query()["objecttypes"] = 8;

    var lookupItems = new parent.window.Mscrm.CrmDialog(oUrl, '', '850', '700');
    lookupItems.setCallbackReference(function (retVal) {
        if ((retVal !== null) && (retVal !== undefined)) {
            var returnedItems = retVal;
            //process the selections
            if (typeof(retVal) == "string") {
                returnedItems = JSON.parse(retVal);
            }

            for (i = 0; i < returnedItems.items.length; i++) {
                //Retrieve CRM record
                var providerId = returnedItems.items[i].id;
                lookupUserRecord(providerId, 'icon', 1);
            }
        }
    });
    lookupItems.show();
}

function lookupUserRecord(id, source, switchValue) {
    Xrm.WebApi.retrieveRecord("systemuser", id, "?$select=firstname,lastname,internalemailaddress,cvt_enablevod").then(
    function success(result) {
        console.log('Successfully retrieved User record with id:' + id);
        var userRecord = result;

        if (userRecord !== null) {
            if (switchValue === 2 && userRecord.cvt_enablevod !== true) {
                console.log("The Switch Value in the Active settings is set to User-Specific and the VOD is not enabled for the user, Display the access denied friendly error message");
                $("#msgBoxAccessDenied").show();
                toggleIcon(true);
                return;
            }

            console.log("All switches are ON. Start loading the User Information");
            $("#searchArea").show();
            $("#buttons").show();

            var userEmail = userRecord.internalemailaddress;

            if (userEmail !== null) {
                $("#ProviderEmailAddressTextBox").val(userEmail);
                globalProvId = id;
                $("#PatientEmailAddressTextBox").focus();
            }
            else {
                $("#ProviderEmailAddressTextBox").val("");
                //Clear Global ProviderId variable
                globalProvId = null;

                if (source == 'onload') {
                    alert("Current user  (" + userRecord.firstname + " " + userRecord.lastname + ") has no email address, please choose a different user for the provider.");
                }
                else {
                    alert("User selected (" + userRecord.firstname + " " + userRecord.lastname + ") has no email address, please choose a different user for the provider.");
                    openProviderLookup(this);
                }
            }
        }
    },
    function (error) {
        // Failed to get User
        console.log("Error: Failed to retrieve the user record" + error.message);
    });
}

function ValidateActiveSettingSwitch(currentUserId) {
    //The retrieveRecord was failing when the VOD html web resource was accessed directly from the browser outside of TMP.
    //Hence the below fix was added to get the retrieveRecord working
    window["ENTITY_SET_NAMES"] = window["ENTITY_SET_NAMES"] || JSON.stringify({
        "systemuser": "systemusers",
        "cvt_vod": "cvt_vods",
        "mcs_setting": "mcs_settings",
        "mcs_integrationsetting": "mcs_integrationsettings"
    });

    var deferred = $.Deferred();

    var useVod = 0;
    var filter = "mcs_name eq 'Active Settings'";
    Xrm.WebApi.retrieveMultipleRecords("mcs_setting", "?$select=cvt_usevod&$filter=" + filter).then(
    function success(result) {
        if (result !== null && result.entities.length !== 0) {
            useVod = result.entities[0].cvt_usevod !== null ? result.entities[0].cvt_usevod : 0;
            if (useVod > 0) {
                console.log("The Switch is ON or User-specific, Call the function to retrieve the user information");
                lookupUserRecord(currentUserId, 'onload', useVod);
            }
            else {
                $("#msgBoxAccessDenied").show();
                toggleIcon(true);
                return;
            }
            deferred.resolve(useVod);
        }
    },
    function (error) {
        // Failed to get User
        console.log(error.message);
    });
    return deferred.promise();
}

function openVirtualCareManager() {
    var deferred = $.Deferred();

    var filter = "statuscode eq 1";
    Xrm.WebApi.retrieveMultipleRecords("mcs_setting", "?$select=cvt_virtualcaremanager&$filter=" + filter).then(
    function success(result) {
        if (result !== null && result.entities.length !== 0) {
            var url = result.entities[0].cvt_virtualcaremanager;
            if (url !== null) {
                window.open(url);
            }

            deferred.resolve(useVod);
        }
        else {
            alert("Integration Settings Record with the Name: VideoOnDemandDefaultSystemUrl is missing in this environment. Please contact TMP Support and provide this information");
        }
    },
    function (error) {
        // Failed to get User
        console.log(error.message);
    });
    return deferred.promise();
}