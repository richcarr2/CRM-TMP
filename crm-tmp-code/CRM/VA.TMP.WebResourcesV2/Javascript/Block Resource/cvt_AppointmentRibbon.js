//SD: web-use-strict-equality-operators
if (typeof MCS === "undefined") {
    MCS = {};
}

// Create Namespace container for functions in this library;
MCS.AppointmentRibbon = {};

MCS.AppointmentRibbon.GetDialogID = function () {
    //SD - Start
    //web-use-strict-mode
    "use strict";
    //SD - End
    var deferred = $.Deferred();
    var returnData = {
        dialogId: ''
    };

    var runVistaDeferred = MCS.VIALogin.CheckVistaSwitches();

    $.when(runVistaDeferred).done(function (returnedData) {
        var runVista = returnedData;

        if (!runVista) {
            returnData.dialogId = "7AB12A8A-A5BA-4ABD-9912-A9F859BCC39A";
            deferred.resolve(returnData);
        }
        else {
            var validDuzDeferred = MCS.VIALogin.IsValidUserDuz();

            $.when(validDuzDeferred).done(function (returnData) {
                var validDuz = returnData.data.duzIsValid;

                if (!validDuz) {
                    var samlTokenDeferred = MCS.VIALogin.IsValidSamlToken();

                    $.when(samlTokenDeferred).done(function (validSamlToken) {

                        if (validSamlToken) {
                            var loginDeferred = MCS.VIALogin.Login();

                            $.when(loginDeferred).done(function () {
                                returnData.dialogId = '';
                                alert("Unable to cancel appointment in Vista until you have logged into Vista.");
                                deferred.resolve(returnData);
                            });
                        }
                        else {
                            var loginSamlDeferred = MCS.VIALogin.Saml();
                            $.when(loginSamlDeferred).done(function (returnData) {
                                // done, verify returnData.success
                                returnData.dialogId = '';
                                alert("Unable to cancel appointment in Vista until you have logged into Vista.");
                                deferred.resolve(returnData);
                            });
                        }
                    });
                }
                else {
                    returnData.dialogId = "88AB1E53-757C-4CA7-9BF5-C3221FE4E9F6";
                    deferred.resolve(returnData);
                }
            });
        }
    });

    return deferred.promise();
};

MCS.AppointmentRibbon.RunCloseAppointmentDialog = function (primaryControl) {
    //var formContext = primaryControl.getFormContext();
    formContext = primaryControl;
    debugger;
    formContext.getAttribute("createdon").fireOnChange(); //This is to trigger the MCS.VIALogin.LoginOnCancelAppointment in cvt_viaLogin.js which is registered on change of created on field. calling this way instead of direct function call would attach/register to the vialogin web resource html and users can see the login updates on screen under Vista login section.
    var getDialogIDDeferred = MCS.AppointmentRibbon.GetDialogID();

    $.when(getDialogIDDeferred).done(function (returnData) {

        var dialogId = returnData.dialogId;
        //SD: web-use-strict-equality-operators
        if (dialogId !== "") {
            //Save the duz to the record
            var updateParam = {};
            var patDuz = formContext.getAttribute("cvt_patuserduz").getValue();
            var proDuz = formContext.getAttribute("cvt_prouserduz").getValue();

            //SD: web-use-strict-equality-operators
            if (patDuz !== null || patDuz !== "") updateParam["cvt_PatUserDuz"] = patDuz;

            //SD: web-use-strict-equality-operators
            if (proDuz !== null || proDuz !== "") updateParam["cvt_ProUserDuz"] = proDuz;

            MCS.cvt_Common.openDialogOnCurrentRecord(primaryControl, dialogId);
        }
    });
};