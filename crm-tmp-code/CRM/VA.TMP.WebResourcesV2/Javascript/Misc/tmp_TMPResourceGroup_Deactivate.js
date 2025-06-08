//If the SDK namespace object is not defined, create it.
if (typeof(MCS) == "undefined") {
    MCS = {};
}
// Create Namespace container for functions in this library;
MCS.TMPResourceGroup = {};

MCS.TMPResourceGroup.Deactive = function (formContext) {
    var SR_Count = 0;
    var GR_Count = 0;
    var id = formContext.data.entity.getId();

    Xrm.WebApi.online.retrieveMultipleRecords("cvt_schedulingresource", "?$select=cvt_schedulingresourceid&$filter=_cvt_tmpresourcegroup_value eq " + id + " and  statuscode eq 1").then(
    function success(results) {
        SR_Count = results.entities.length;
        Xrm.WebApi.online.retrieveMultipleRecords("mcs_groupresource", "?$select=_mcs_relatedresourcegroupid_value&$filter=_mcs_relatedresourcegroupid_value eq " + id + " and  statuscode eq 1").then(
        function success(results) {
            if (results.entities.length > 0 || SR_Count > 0) {
                GR_Count = results.entities.length;
                var alertStrings = {
                    text: "Please check Related section tab on the Resource form for the following associations:\n Resource cannot be deactivated until these associations are removed.\n\nScheduling Resources(" + SR_Count + ")  \nGroup Resources(" + GR_Count + ")",
                    title: "Deactivate Process Error"
                };
                var alertOptions = {
                    height: 250,
                    width: 650
                };
                Xrm.Navigation.openAlertDialog(alertStrings, alertOptions).then(
                function success(result) {
                    //console.log("Alert dialog closed");
                },
                function (error) {
                    //console.log(error.message);
                });
                //}
            }
            else {
                var confirmStrings = {
                    text: " Do you want to deactivate the selected 1 TMP Resource? You can reactivate it later,\n if you wish.\nThis action will change the status of the selected TMP Resource to Inactive.",
                    title : "Confirm TMP Resource Deactivation"
                };
                var confirmOptions = {
                    height: 200,
                    width: 600
                };
                Xrm.Navigation.openConfirmDialog(confirmStrings, confirmOptions).then(
                function (success) {
                    if (success.confirmed) {
                        // deactivate logic for tmp resource
                        if (results.entities.length === 0) {
                            var entity = {};
                            entity.statecode = 1;
                            entity.statuscode = 2;
                            Xrm.WebApi.online.updateRecord("mcs_resourcegroup", id, entity).then(
                            function success(result) {
                                Xrm.Utility.alertDialog('TMP Resource Group has been deactivated.');
                                formContext.data.refresh();
                            },
                            function (error) {
                                Xrm.Utility.alertDialog(error.message);
                            });
                        }
                    }
                });

            }
        });
    });
};

MCS.TMPResourceGroup.DeactiveOnSelectedRecord = function (item, primarycontrol, ItemCount) {
    var selectedItem = item[0];
    var value = new Array();
    value[0] = new Object();
    value[0].id = selectedItem.Id;

    Xrm.WebApi.online.retrieveMultipleRecords("cvt_schedulingresource", "?$select=_cvt_tmpresourcegroup_value&$filter=cvt_schedulingresourceid eq  " + selectedItem.Id + "  and  statuscode eq 1").then(
    function success(results) {
        if (results.entities.length > 0) {
            // var alertStrings = { text: "This Resource Group cannot be inactivated since it is associated with " + results.entities.length + " active Resource, participating sites with Can Be Scheduled=Y. Please remove associations." };
            var alertStrings = {
                text: "One or more of the record(s) selected have associations to Scheduling Resources or Group Resources.  Please remove associations."
            };
            var alertOptions = {
                height: 200,
                width: 600
            };
            Xrm.Navigation.openAlertDialog(alertStrings, alertOptions).then(
            function success(result) {
                //console.log("Alert dialog closed");
            },
            function (error) {
                //console.log(error.message);
            });
            return;
        }
        Xrm.WebApi.online.retrieveMultipleRecords("mcs_groupresource", "?$select=_mcs_relatedresourcegroupid_value&$filter=_mcs_relatedresourcegroupid_value eq " + selectedItem.Id + " and  statuscode eq 1").then(
        function success(results) {
            if (results.entities.length > 0) {
                // var alertStrings = { text: "This resource cannot be inactivated since it is associated with " + results.entities.length + " active Resource, participating sites with Can Be Scheduled=Y. Please remove associations." };
                var alertStrings = {
                    text: "One or more of the record(s) selected have associations to Scheduling Resources or Group Resources.  Please remove associations."
                };
                var alertOptions = {
                    height: 200,
                    width: 600
                };
                Xrm.Navigation.openAlertDialog(alertStrings, alertOptions).then(
                function success(result) {
                    //console.log("Alert dialog closed");
                },
                function (error) {
                    //console.log(error.message);
                });
                return;

            }

            var confirmStrings = {
                text: " Do you want to deactivate the selected " + ItemCount + " TMP Resource? You can reactivate it later,\n if you wish.\nThis action will change the status of the selected TMP Resource to Inactive.",
                title : "Confirm TMP Resource Deactivation"
            };
            var confirmOptions = {
                height: 200,
                width: 600
            };
            Xrm.Navigation.openConfirmDialog(confirmStrings, confirmOptions).then(
            function (success) {
                if (success.confirmed) {
                    // deactivate logic for tmp resource
                    if (results.entities.length === 0) {
                        var entity = {};
                        entity.statecode = 1;
                        entity.statuscode = 2;
                        Xrm.WebApi.online.updateRecord("mcs_resourcegroup", selectedItem.Id, entity).then(
                        function success(result) {
                            Xrm.Utility.alertDialog('TMP Resource has been deactivated.');
                            primarycontrol.refresh();
                        },
                        function (error) {
                            Xrm.Utility.alertDialog(error.message);
                        });
                    }
                    /*console.log("TMP Resource has been deactivated.")*/
                    ;
                    //else
                    //        console.log("Cancel.");
                }
            });

        });

    },
    function (error) {
        Xrm.Utility.alertDialog(error.message);
    });
};