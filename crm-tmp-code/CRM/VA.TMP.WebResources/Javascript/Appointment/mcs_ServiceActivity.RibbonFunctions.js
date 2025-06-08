///<summary>Helper function: Opens a window for a new Recurring Appointment record using the Information form.</summary>
///<param name="subject" optional="false" type="String">
///Subject associated with the recurring appointment series.
///</param>
///<param name="cvt_serviceactivityid" optional="false" type="String">
///Unique identifier for Service Activity associated with Recurring Appointment. Expected value is a String that matches the pattern for a GUID '/^{?[A-Z0-9]{8}-[A-Z0-9]{4}-[A-Z0-9]{4}-[A-Z0-9]{4}-[A-Z0-9]{12}}?$/i'.
///</param>
///<param name="cvt_serviceactivityidname" optional="false" type="String">
///The text to display for the record represented by the cvt_serviceactivityid parameter.
///</param>
//MCS.VIALogin.GettingNewUserDuz = false;

function openNewRecurringAppointmentMaster(subject, serviceActivityID, serviceActivityName) {
    //SD - Start
    //web-use-strict-mode
    "use strict";
    //SD - End
    //SD: web-use-strict-equality-operators
    if ((typeof Xrm !== "undefined") && (typeof Xrm.Utility !== "undefined")) {
        var p = {};
        p.formid = "4a8cda55-024e-419c-bbe1-9540e0b8e297"
        p.subject = subject;
        p.cvt_serviceactivityid = serviceActivityID;
        p.cvt_serviceactivityidname = serviceActivityName;
        Xrm.Navigation.openForm("recurringappointmentmaster", null, p);
    }
    else {
        var features = "location=no,menubar=no,status=no,toolbar=no,resizable=yes";
        var extraqs = ["formid=4a8cda55-024e-419c-bbe1-9540e0b8e297",
            "subject=" + subject,
            "cvt_serviceactivityid=" + serviceActivityID,
            "cvt_serviceactivityidname=" + serviceActivityName]
        var globalContext = Xrm.Utility.getGlobalContext();
        var url = globalContext.getClientUrl();
        window.open(url + "/main.aspx?etn=recurringappointmentmaster&pagetype=entityrecord&extraqs=" + encodeURIComponent(extraqs.join("&")), "_blank", features, false);
    }
}

//Functions Called by Ribbon Buttons
//Example:
//Calls openNewRecurringAppointmentMaster - called by Ribbon Button "Recurring Service Activity"
function CreateRecurringServiceActivity(primaryControl) {
    var formContext = primaryControl.getFormContext();
    var serviceActivityName = "Recurring " + formContext.getAttribute("subject").getValue();
    openNewRecurringAppointmentMaster(serviceActivityName, formContext.data.entity.getId(), serviceActivityName)
}

//Open Recurring Appointment Master Record - called by Ribbon Button "Edit Series"
EditServiceActivitySeries = function (primaryControl) {
    var formContext = primaryControl.getFormContext();
    var relatedMaster = formContext.getAttribute("cvt_recurringappointmentsmaster").getValue();
    if (relatedMaster != null)
        Xrm.Navigation.openForm("recurringappointmentmaster", relatedMaster[0].id)
}

ReOpenServiceActivity = function (primaryControl) {
    var formContext = primaryControl.getFormContext();
    var isDataAdmin = MCS.cvt_Common.userHasRoleInList("TSS Data Administrator|System Administrator|TSS Application Administrator");
    var userSettings = Xrm.Utility.getGlobalContext().userSettings;
    //SD: web-use-strict-equality-operators
    if (isDataAdmin || formContext.getAttribute("ownerid").getValue()[0].id === userSettings.userId)
        Mscrm.CommandBarActions.activate(formContext.data.entity.getId(), formContext.data.entity.getEntityName());
    else
        MCS.cvt_Common.openDialogOnCurrentRecord("00520409-98FB-4A1E-B67C-D3D6783ACB84");
}

CancelServiceActivity = function (primaryControl) {
    var formContext = primaryControl.getFormContext();
    formContext.getAttribute("createdon").fireOnChange(); //This is to trigger the MCS.VIALogin.LoginOnCancelAppointment in cvt_viaLogin.js which is registered on change of created on field. calling this way instead of direct function call would attach/register to the vialogin web resource html and users can see the login updates on screen under Vista login section.
    var runVista = MCS.VIALogin.CheckVistaSwitches();
    var dialogId = "";
    if (!runVista) {
        dialogId = "789CD165-5CAD-49B3-ACF3-42C3D5B31584";
    }
    else {
        var validDuz = MCS.VIALogin.IsValidUserDuz();
        if (!validDuz) {
            var validToken = MCS.VIALogin.IsValidSamlToken();
            if (validToken) {
                MCS.VIALogin.Login();
                alert("Unable to cancel appointment in Vista until you have logged into Vista.");
            }
            else {
                MCS.VIALogin.Saml();
                alert("Unable to cancel appointment in Vista until you have logged into Vista.");
            }
        }
        else
            dialogId = "B8A805D8-01B1-4922-80CA-D4E46F2EC836";
    }

    //SD: web-use-strict-equality-operators
    if (dialogId != "") {
        //Save the duz to the record
        var updateParam = {};
        var patDuz = formContext.getAttribute("cvt_patuserduz").getValue();
        var proDuz = formContext.getAttribute("cvt_prouserduz").getValue();

        //SD: web-use-strict-equality-operators
        if (patDuz != null || patDuz != "")
            updateParam["cvt_PatUserDuz"] = patDuz;

        //SD: web-use-strict-equality-operators
        if (proDuz != null || proDuz != "")
            updateParam["cvt_ProUserDuz"] = proDuz;

        //MCS.cvt_Common.openDialogOnCurrentRecord(primaryControl, dialogId);
        MCS.cvt_Common.openDialogOnCurrentRecord(dialogId);
    }
}


CompleteServiceActivity = function (primaryControl) {
    var formContext = primaryControl.getFormContext();
    var requestName = "cvt_CompleteAppointment";
    var requestParams =
        [{
            Key: "Target",
            Type: MCS.Scripts.Process.DataType.EntityReference,
            Value: { id: formContext.data.entity.getId(), entityType: 'serviceappointment' }
        }];

    MCS.Scripts.Process.ExecuteAction(requestName, requestParams)
        .done(function (response) {
            formContext.ui.close();
        })
        .fail(function (err) {
            alert("Failed to Complete this appointment.  Details: " + err.responseText);
        });

}

OnDialogClose = function (dialog, timer, primaryControl) {
    var formContext = primaryControl.getFormContext();
    if (!dialog || dialog.closed) {
        clearInterval(timer); //stop the timer

        Xrm.WebApi.retrieveRecord("ServiceAppointment", formContext.data.entity.getId(), "?$select=StateCode").then(
            function success(result) {
                saRecord = result;
                //Refresh the form when the state code has changed from Active to Closed/Cancelled from the dialog
                //SD: web-use-strict-equality-operators
                if (saRecord.StateCode !== null && (saRecord.StateCode.Value === 1 || saRecord.StateCode.Value === 2)) {
                    window.location.reload(true);
                }
            },
            function (error) {
                window.location.reload(true);
            }
        );

        //CrmRestKit.Retrieve('ServiceAppointment', Xrm.Page.data.entity.getId(), ['StateCode'], false)
        //    .fail(function(err) {
        //        window.location.reload(true);
        //    }).done(function(serviceActivity) {
        //        saRecord = serviceActivity.d;

        //        //Refresh the form when the state code has changed from Active to Closed/Cancelled from the dialog
        //        if (saRecord.StateCode != null && (saRecord.StateCode.Value == 1 || saRecord.StateCode.Value == 2)) {
        //            window.location.reload(true);
        //        }
        //    });
    }
}

SaveSA = function (primaryControl) {
    SaveRecord(null, primaryControl);
};

SaveAndCloseSA = function (primaryControl) {
    SaveRecord("saveandclose", primaryControl);
};

SaveAndNewSA = function (primaryControl) {
    SaveRecord("saveandnew", primaryControl);
}

GetContactDetailsDeferred = function () {
    var deferred = $.Deferred();

    var returnData = {
        success: true,
        data: {
        }
    };


    Xrm.WebApi.retrieveRecord("Contact", patients[i].id, "?$select=ContactId,EMailAddress1,DoNotEMail,cvt_TabletType").then(
        function success(result) {
            patRecord = result;
            var doNotEMail = patRecord.DoNotEMail != null ? patRecord.DoNotEMail : false;

            //SD: web-use-strict-equality-operators
            if (patRecord.EMailAddress1 !== null && patRecord.EMailAddress1 !== "")
                allowSave = true;
            //SD: web-use-strict-equality-operators
            if (doNotEMail && (!usingVMR || patRecord.cvt_TabletType === 917290002))
                allowSave = true;

            returnData.data.allowSave = allowSave;
            resolve(returnData);
        },
        function (error) {
            alert("Patient could not be found: " + patients[i].id);
            //return;

            returnData.data.success = false;
            resolve(returnData);
        }
    );

    return deferred.promise();
}

SaveRecord = function (saveOption, primaryControl) {
    alert("Executing SaveRecord from *un*bundled file");
    var formContext = primaryControl.getFormContext();

    MCS.VIALogin.isGrpClinicappt = formContext.getAttribute("mcs_groupappointment").getValue();
    return false;
    formContext.data.entity.save(saveOption);
    return;

    //web-remove-debug-script
    //debugger;

    // re-write as deferred 
    var validUserDuz = MCS.VIALogin.IsValidUserDuz();

    if ((!validUserDuz) && (!MCS.VIALogin.isGrpClinicappt)) {
        alert("Vista login has expired, attempting to get new login");
        MCS.VIALogin.Saml();
        return;
    }

    var isGroup = formContext.getAttribute("mcs_groupappointment").getValue();
    var isHomeMobile = formContext.getAttribute("cvt_type").getValue();
    var usingVMR = false;
    if (isGroup && !isHomeMobile) {
        formContext.data.entity.save(saveOption);
        return;
    }
    var patientObj = formContext.getAttribute('customers');
    var patients = patientObj != null ? patientObj.getValue() != null ? patientObj.getValue() : null : null;
    //SD: web-use-strict-equality-operators
    if (patients === null || patients.length === 0) {
        alert("You must add a patient to all individual or VA Video Connect Group appointments");
        return;
    }
    var patRecord;
    var allowSave = false;
    var currentVeteranIndex = 0;
    for (var i = 0; i < patients.length; i++) {
        allowSave = false;

        Xrm.WebApi.retrieveRecord("Contact", patients[i].id, "?$select=ContactId,EMailAddress1,DoNotEMail,cvt_TabletType").then(
            function success(result) {
                patRecord = result;
                var doNotEMail = patRecord.DoNotEMail != null ? patRecord.DoNotEMail : false;

                //SD: web-use-strict-equality-operators
                if (patRecord.EMailAddress1 != null && patRecord.EMailAddress1 !== "")
                    allowSave = true;
                //SD: web-use-strict-equality-operators
                if (doNotEMail && (!usingVMR || patRecord.cvt_TabletType === 917290002))
                    allowSave = true;
            },
            function (error) {
                alert("Patient could not be found: " + patients[i].id);
                return;
            }
        );

        if (!allowSave) {
            currentVeteranIndex = i;
            break;
        }
    }
    if (allowSave)
        formContext.data.entity.save(saveOption);
    else
        EnterEmail(patients[currentVeteranIndex].id, usingVMR);
};

EnterEmail = function (patientId, usingVMR) {
    if (usingVMR) {
        alert("All veterans using VMRs must have email addresses before they can be booked.  Enter patient's email and try to save again or else inform the veteran that he/she will need to find another video visit option.");
        MCS.cvt_Common.openDialogProcess("52e2a47a-becc-449f-821a-0b95916e1cb1", "contact", patientId);
    }
    else {
        alert("Please Enter the patient's email address or else opt them out of emails and then try to save again.");
        MCS.cvt_Common.openDialogProcess("AB9FF42A-ADAC-4C01-ADE7-01C1A1F7E320", "contact", patientId);
    }
}

schedulingAppointment = function () {
    Xrm.Navigation.openForm("serviceappointment");
}