//If the SDK namespace object is not defined, create it.
if (typeof (MCS) == "undefined") {
    MCS = {};
};
// Create Namespace container for functions in this library;
MCS.cvt_StagingResource_Buttons = {};

MCS.cvt_StagingResource_Buttons.ApproveAndComplete = function (primaryControl) {
    var primaryControl = primaryControl;
    if (MCS.cvt_StagingResource_Buttons.VerifyRequiredFormFields(primaryControl) && confirm("Do you want to approve the Inventory Resource and all the Components under it?\n\rPlease review the Inventory resource and staging components records and ensure all the required fields have the values set."))
        MCS.cvt_StagingResource_Buttons.UpdateRecord(primaryControl, 917290000);
};

MCS.cvt_StagingResource_Buttons.Reject = function (primaryControl) {
    var dialog = MCS.cvt_Common.openDialogOnCurrentRecord(primaryControl, "0463e5a0-a15c-4e95-acd4-a188e6f14eec");
    try {
        var timer = setInterval(function () { MCS.cvt_StagingResource_Buttons.OnDialogClose(primaryControl, dialog, timer) }, 1000); //Poll every second
    } catch (ex) {
        //Error Handling
        MCS.cvt_Common.RestError("An error occured while opening the dialog:" + ex);
    }
};

MCS.cvt_StagingResource_Buttons.OnDialogClose = function (primaryControl, dialog, timer) {
    if (!dialog || dialog.closed) {
        clearInterval(timer); //stop the timer
        MCS.cvt_StagingResource_Buttons.Refresh(primaryControl, false);
    }
}

MCS.cvt_StagingResource_Buttons.Refresh = function (primaryControl, isApproved) {
    var formContext = primaryControl.getFormContext();
    Xrm.WebApi.retrieveRecord("cvt_stagingresource", formContext.data.entity.getId(), "?$select=statecode,cvt_approvalresult").then(
        function success(result) {
            var resourceRecord = result; //Refresh the form when the state code has changed from Active to Inactive from the dialog
            if (isApproved && resourceRecord.cvt_approvalresult != null && resourceRecord.cvt_approvalresult === true) {
                formContext.ui.clearFormNotification("00000000-0000-0000-0000-000000000000");
                formContext.ui.setFormNotification("Please resolve all Import Field Mismatches before you proceed with the approval.", "WARNING", "00000000-0000-0000-0000-000000000000");

                //alert("Please resolve all Import Field Mismatches before you proceed with the approval.");
                var fieldMismatchValidationGrid = document.getElementById("MismatchFieldValidation");
                setTimeout(function () { fieldMismatchValidationGrid.control.Refresh() }, 500);
            }

            // Auto refreshing the page, when the status is changed to Inactive successfully, as part of "Approve and Complete".
            if (resourceRecord.statecode != null && (resourceRecord.statecode === 1)) {
                Xrm.Page.data.refresh();           
            }

            formContext.getAttribute("cvt_approvalstatus").setValue(false);
        },
        function (error) {
            MCS.cvt_Common.RestError("An error occured while retriving the record:" + error);
            Xrm.Page.data.refresh();
        }
    );

    //CrmRestKit.Retrieve("cvt_stagingresource", Xrm.Page.data.entity.getId(), ["statecode", "cvt_approvalresult"], false)
    //        .fail(function (ex) {
    //            MCS.cvt_Common.RestError("An error occured while retriving the record:" + ex);
    //            window.location.reload(true);
    //        }).done(function (result) {
    //            var resourceRecord = result.d; //Refresh the form when the state code has changed from Active to Inactive from the dialog
    //            if (isApproved && resourceRecord.cvt_approvalresult != null && resourceRecord.cvt_approvalresult === true) {
    //                Xrm.Page.ui.clearFormNotification("00000000-0000-0000-0000-000000000000");
    //                Xrm.Page.ui.setFormNotification("Please resolve all Import Field Mismatches before you proceed with the approval.", "WARNING", "00000000-0000-0000-0000-000000000000");

    //                //alert("Please resolve all Import Field Mismatches before you proceed with the approval.");
    //                var fieldMismatchValidationGrid = document.getElementById("MismatchFieldValidation");
    //                setTimeout(function () { fieldMismatchValidationGrid.control.Refresh() }, 500);
    //            }

    //            if (resourceRecord.statecode != null && (resourceRecord.statecode.Value === 1)) {
    //                window.location.reload(true);
    //            }

    //            Xrm.Page.getAttribute("cvt_approvalstatus").setValue(false);
    //        });
}

MCS.cvt_StagingResource_Buttons.VerifyRequiredFormFields = function (primaryControl) {
    var formContext = primaryControl.getFormContext();
    var fieldsToVerify = ['cvt_masterserialnumber', 'mcs_type', 'cvt_systemtype', 'cvt_action', 'mcs_relatedsiteid', 'mcs_facility', 'cvt_uniqueid', 'cvt_locationuse'];
    var i;
    for (i = 0; i < fieldsToVerify.length; i++) {
        var field = formContext.getControl(fieldsToVerify[i]);
        var fieldValue = formContext.getAttribute(fieldsToVerify[i]);
        if (field !== null && fieldValue !== null && fieldValue.getValue() == null) {
            //alert('Please review and set all the required fields before you proceed');
            formContext.ui.clearFormNotification("00000000-0000-0000-0000-000000000000");
            formContext.ui.setFormNotification("Please review and set all the required fields before you proceed", "WARNING", "00000000-0000-0000-0000-000000000000");

            formContext.getAttribute("cvt_approvalstatus").setValue(false);
            return false;
        }
    }
    var action = formContext.getAttribute("cvt_action").getValue();
    var resourceToUse = formContext.getAttribute("cvt_resourcetomatch").getValue();

    var systemType = formContext.getAttribute("cvt_systemtype").getValue();
    var cartType = formContext.getAttribute("cvt_carttypeid").getValue();
    if ((systemType === 917290001 && cartType == null) || (action === 917290000 && resourceToUse == null)) //Telehealth Patient Cart System
    {
        formContext.ui.clearFormNotification("00000000-0000-0000-0000-000000000000");
        formContext.ui.setFormNotification("Please review and set all the required fields before you proceed", "WARNING", "00000000-0000-0000-0000-000000000000");
        formContext.getAttribute("cvt_approvalstatus").setValue(false);
        return false;
    }

    return true;
};

MCS.cvt_StagingResource_Buttons.UpdateRecord = function (primaryControl, userSelection) {
    var formContext = primaryControl.getFormContext();
    if (formContext.ui.getFormType() !== MCS.cvt_Common.FORM_TYPE_CREATE) {
        var id = formContext.data.entity.getId();
        id = id.replace("{", "").replace("}", "");
        var record = {};

        record.cvt_approvalstatus = userSelection;
        Xrm.WebApi.updateRecord("cvt_stagingresource", id, record).then(
            function success(result) {
                setTimeout(function () { MCS.cvt_StagingResource_Buttons.Refresh(primaryControl, true); }, 1000);
            },
            function (ex) {
                if (ex != null && ex.message != null) {
                    // var errorJson = JSON.parse(ex.message);
                    //  if (errorJson != null) {
                    //alert(errorJson);
                    alert(ex.message);
                    return;
                    // }
                }
            }
        );

        //CrmRestKit.Update("cvt_stagingresource", id, record, false)
        //    .fail(function(ex) {
        //        if (ex != null && ex.responseText != null) {
        //            var errorJson = JSON.parse(ex.responseText);
        //            if (errorJson != null && errorJson.error != null && errorJson.error.message != null && errorJson.error.message.value != null) {
        //                alert(errorJson.error.message.value);
        //                return;
        //            }
        //        }
        //        //alert("Please review and resolve all Import Field Mismatches before you proceed with the approval.");
        //    })
        //    .done(function() {
        //        setTimeout(function() { MCS.cvt_StagingResource_Buttons.Refresh(true); }, 1000);
        //    });
    }
};