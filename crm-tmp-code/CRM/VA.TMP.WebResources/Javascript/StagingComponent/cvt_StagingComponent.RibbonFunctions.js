//If the SDK namespace object is not defined, create it.
if (typeof (MCS) == "undefined") {
    MCS = {};
};
// Create Namespace container for functions in this library;
MCS.cvt_StagingComponent_Buttons = {};

MCS.cvt_StagingComponent_Buttons.ApproveAndComplete = function () {
    MCS.cvt_StagingComponent_Buttons.UpdateRecord(917290000);
};

MCS.cvt_StagingComponent_Buttons.Reject = function (primaryControl) {
    var formContext = primaryControl.getFormContext();
    //MCS.cvt_StagingComponent_Buttons.UpdateRecord(917290001);
    var dialog = MCS.cvt_Common.openDialogOnCurrentRecord(primaryControl, "2556dfd7-a399-4a4b-b58f-48d81e1b8cf1");
    try {
        var timer = setInterval(function () { MCS.cvt_StagingComponent_Buttons.OnDialogClose(primaryControl, dialog, timer) }, 1000); //Poll every second
    } catch (e) {
        //Error Handling
    }
};

MCS.cvt_StagingComponent_Buttons.OnDialogClose = function (primaryControl,dialog, timer) {
    var formContext = primaryControl.getFormContext();
    if (!dialog || dialog.closed) {
        clearInterval(timer); //stop the timer
        Xrm.WebApi.retrieveRecord("cvt_stagingcomponent", formContext.data.entity.getId(), "?$select=statecode").then(
            function success(result) {
                var componentRecord = result; //Refresh the form when the state code has changed from Active to Inactive from the dialog
                if (componentRecord.statecode != null && (componentRecord.statecode.Value === 1)) {
                    window.location.reload(true);
                }
            },
            function (error) {
                window.location.reload(true);
                MCS.cvt_Common.RestError("An error occured while retriving the record:" + error);
            }
        );
        //CrmRestKit.Retrieve("cvt_stagingcomponent", formContext.data.entity.getId(), ["statecode"], false)
        //    .fail(function (ex) {
        //        window.location.reload(true);
        //        MCS.cvt_Common.RestError("An error occured while retriving the record:" + ex);
        //    }).done(function (result) {
        //        var componentRecord = result.d; //Refresh the form when the state code has changed from Active to Inactive from the dialog
        //        if (componentRecord.statecode != null && (componentRecord.statecode.Value === 1)) {
        //            window.location.reload(true);
        //        }
        //    });
    }
}

MCS.cvt_StagingComponent_Buttons.UpdateRecord = function (primaryControl,userSelection) {
    var formContext = primaryControl.getFormContext();
    if (formContext.ui.getFormType() !== MCS.cvt_Common.FORM_TYPE_CREATE) {
        var id = formContext.data.entity.getId();
        id = id.replace("{", "").replace("}", "");
        var record = {};

        record.cvt_approvalstatus = { Value: userSelection };
        Xrm.WebApi.updateRecord("cvt_stagingcomponent", id, record).then(
            function success(result) {
            },
            function (error) {
                 MCS.cvt_Common.RestError("An following error occured while updating the record:" + ex);
            }
        );
        //CrmRestKit.Update("cvt_stagingcomponent", id, record, false)
        //    .fail(function (ex) { MCS.cvt_Common.RestError("An following error occured while updating the record:" + ex); })
        //    .done(function () {
        //    });
    }
}