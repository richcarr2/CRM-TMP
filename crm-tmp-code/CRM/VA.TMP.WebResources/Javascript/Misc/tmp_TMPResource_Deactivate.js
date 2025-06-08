//If the SDK namespace object is not defined, create it.
if (typeof (MCS) == "undefined") { MCS = {}; }
// Create Namespace container for functions in this library;
MCS.TMPResource = {};

/*function Deactive(formContext) {*/
MCS.TMPResource.Deactive = function (formContext) {
    var SR_Count = 0;
    var GR_Count = 0;
    var id = formContext.data.entity.getId();
    Xrm.WebApi.online.retrieveMultipleRecords("cvt_schedulingresource", "?$select=cvt_schedulingresourceid&$filter=_cvt_tmpresource_value eq " + id + " and  statuscode eq 1").then(
        function success(results) {
            SR_Count = results.entities.length;
            Xrm.WebApi.online.retrieveMultipleRecords("mcs_groupresource", "?$select=mcs_groupresourceid&$filter=_mcs_relatedresourceid_value eq " + id + " and  statuscode eq 1").then(
                function success(results) {
                    if (results.entities.length > 0 || SR_Count > 0) {
                        GR_Count = results.entities.length;
                        var alertStrings = { text: "Please check Related section tab on the Resource form for the following associations:\n Resource cannot be deactivated until these associations are removed.\n\nScheduling Resources(" + SR_Count + ")  \nGroup Resources(" + GR_Count + ")", title: "Deactivate Process Error" };
                        var alertOptions = { height: 250, width: 650 };
                        Xrm.Navigation.openAlertDialog(alertStrings, alertOptions).then(
                            function success(result) {
                                //console.log("Alert dialog closed");
                            },
                            function (error) {
                                //console.log(error.message);
                            }
                        );
                        //}
                    }
                    else {
                        var confirmStrings = { text: " Do you want to deactivate the selected 1 TMP Resource? You can reactivate it later,\n if you wish.\nThis action will change the status of the selected TMP Resource to Inactive.", title: "Confirm TMP Resource Deactivation" };
                        var confirmOptions = { height: 200, width: 600 };
                        Xrm.Navigation.openConfirmDialog(confirmStrings, confirmOptions).then(
                            function (success) {
                                if (success.confirmed) {
                                    // deactivate logic for tmp resource
                                    if (results.entities.length === 0) {
                                        var entity = {};
                                        entity.statecode = 1;
                                        entity.statuscode = 2;
                                        Xrm.WebApi.online.updateRecord("mcs_resource", id, entity).then(
                                            function success(result) {
                                                Xrm.Utility.alertDialog('TMP Resource has been deactivated.');
                                                formContext.data.refresh();
                                            },
                                            function (error) {
                                                Xrm.Utility.alertDialog(error.message);
                                            }
                                        );
                                    }
                                }
                            });

                    }
                }
            );
        }
    );

};

MCS.TMPResource.DeactiveOnSelectedRecord = function (item, primarycontrol, ItemCount) {
    var selectedItem = item[0];
    var value = new Array();
    value[0] = new Object();
    value[0].id = selectedItem.Id;


    Xrm.WebApi.online.retrieveMultipleRecords("cvt_schedulingresource", "?$select=cvt_schedulingresourceid&$filter=_cvt_tmpresource_value eq " + selectedItem.Id + " and  statuscode eq 1").then(
        function success(results) {
            if (results.entities.length > 0) {
                // var alertStrings = { text: "This resource cannot be inactivated since it is associated with " + results.entities.length + " active Resource, participating sites with Can Be Scheduled=Y. Please remove associations." };
                var alertStrings = { text: "One or more of the record(s) selected have associations to Scheduling Resources or Group Resources.  Please remove associations." };
                var alertOptions = { height: 200, width: 600 };
                Xrm.Navigation.openAlertDialog(alertStrings, alertOptions).then(
                    function success(result) {
                        //console.log("Alert dialog closed");
                    },
                    function (error) {
                        //console.log(error.message);
                    }
                );
                return;
            }
            Xrm.WebApi.online.retrieveMultipleRecords("mcs_groupresource", "?$select=mcs_groupresourceid&$filter=_mcs_relatedresourceid_value eq " + selectedItem.Id + " and  statuscode eq 1").then(
                function success(results) {
                    if (results.entities.length > 0) {
                        // var alertStrings = { text: "This resource cannot be inactivated since it is associated with " + results.entities.length + " active Resource, participating sites with Can Be Scheduled=Y. Please remove associations." };
                        var alertStrings = { text: "One or more of the record(s) selected have associations to Scheduling Resources or Group Resources.  Please remove associations." };
                        var alertOptions = { height: 200, width: 600 };
                        Xrm.Navigation.openAlertDialog(alertStrings, alertOptions).then(
                            function success(result) {
                                //console.log("Alert dialog closed");
                            },
                            function (error) {
                                //console.log(error.message);
                            }
                        );
                        return;
                    }

                    var confirmStrings = {
                        text: " Do you want to deactivate the selected " + ItemCount + " TMP Resource? You can reactivate it later,\n if you wish.\nThis action will change the status of the selected TMP Resource to Inactive.", title: "Confirm TMP Resource Deactivation"
                    };
                    var confirmOptions = { height: 200, width: 600 };
                    Xrm.Navigation.openConfirmDialog(confirmStrings, confirmOptions).then(
                        function (success) {
                            if (success.confirmed) {
                                // deactivate logic for tmp resource
                                if (results.entities.length === 0) {
                                    var entity = {};
                                    entity.statecode = 1;
                                    entity.statuscode = 2;
                                    Xrm.WebApi.online.updateRecord("mcs_resource", selectedItem.Id, entity).then(
                                        function success(result) {
                                            Xrm.Utility.alertDialog('TMP Resource has been deactivated.');
                                            primarycontrol.refresh();
                                        },
                                        function (error) {
                                            Xrm.Utility.alertDialog(error.message);
                                        }
                                    );
                                }

                            }
                        });

                }
            );

        },
        function (error) {
            Xrm.Utility.alertDialog(error.message);
        }
    );
};