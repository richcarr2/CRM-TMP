//If the SDK namespace object is not defined, create it.
if (typeof (MCS) == "undefined") { MCS = {}; }
// Create Namespace container for functions in this library;
MCS.MissingResourceRibbon = {};

MCS.MissingResourceRibbon.UpdateReserveResourceActivityPartyList = function (primaryControl) {
    console.log("running")
    var formContext = primaryControl;
    var entityReference = "";
    var entityReferenceDescription = "";
    var missingResourceId = formContext.data.entity.getId().replace("{", "").replace("}", "");

    //Verify that the Missing Resource record is active
    if (formContext.getAttribute("statuscode").getValue() == 1 && formContext.getAttribute("statecode").getValue() == 0) {
        Xrm.Page.ui.clearFormNotification()
        Xrm.Page.ui.setFormNotification("Please activate the record before running Update Reserve Resource", "ERROR")
        return;
    }

    //Verify that the Missing Resource record is saved
    if (Xrm.Page.data.entity.getIsDirty()) {
        Xrm.Page.ui.clearFormNotification()
        Xrm.Page.ui.setFormNotification("Please save the record before running Update Reserve Resource", "ERROR")
        return;
    }

    var resourceType = formContext.getAttribute("mcs_type").getValue();
    switch (resourceType) {
        case 803750000:
            entityReference = formContext.getAttribute("mcs_tmpresource").getValue();
            entityReferenceDescription = "TMP Resource"
            break;
        case 803750001:
            entityReference = formContext.getAttribute("mcs_user").getValue();
            entityReferenceDescription = "User"
            break;
        case 803750002:
            entityReference = formContext.getAttribute("mcs_patient").getValue();
            entityReferenceDescription = "Patient"
            break;
        default:
            Xrm.Page.ui.setFormNotification("Invalid missing resource type selected.", "ERROR")
    }
    //Verify that the Missing Resource record is saved
    if (!entityReference) {
        Xrm.Page.ui.clearFormNotification()
        Xrm.Page.ui.setFormNotification("Please populate the " + entityReferenceDescription + " lookup field before running Update Reserve Resource", "ERROR")
        return;
    }

    //Verify that the Missing Resource record has related Reserve Resources
    Xrm.WebApi.online.retrieveMultipleRecords("mcs_reserveresourcemissingresource", "?$select=mcs_reserveresourcemissingresourceid&$filter=statuscode eq 1 and _mcs_missingresource_value eq " + missingResourceId).then(
        function success(results) {
            if (results.entities.length == 0) {
                Xrm.Page.ui.clearFormNotification()
                Xrm.Page.ui.setFormNotification("No active Reserve Resource Missing Resources associated with record. Cancelling Update.", "WARNING")
                return;
            }
            Xrm.Page.ui.clearFormNotification()

            var parameters = {};
            var entity = {};
            entity.id = missingResourceId;
            entity.entityType = "mcs_missingresource";
            parameters.entity = entity;
            var mcs_MissingResourceUpdatePartiesRequest = {
                entity: parameters.entity,
                getMetadata: function () {
                    return {
                        boundParameter: "entity",
                        parameterTypes: {
                            "entity": {
                                "typeName": "mscrm.mcs_missingresource",
                                "structuralProperty": 5
                            }
                        },
                        operationType: 0,
                        operationName: "mcs_MissingResourceUpdateParties"
                    };
                }
            };
            Xrm.WebApi.online.execute(mcs_MissingResourceUpdatePartiesRequest).then(
                function success(result) {
                    if (result.ok) {
                        Xrm.Page.ui.setFormNotification("Updating Reserve Resources. Please wait.", "INFORMATION")
                    }
                },
                function (error) {
                    Xrm.Utility.alertDialog(error.message);
                }
            );
        },
        function (error) {
            Xrm.Page.ui.clearFormNotification()
            Xrm.Page.ui.setFormNotification("Error retrieving Reserve Resource Missing Resources.", "ERROR")
            return;
        }
    );
};