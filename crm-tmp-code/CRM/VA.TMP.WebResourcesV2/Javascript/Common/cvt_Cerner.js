if (typeof(MCS) === "undefined") {
    MCS = {};
}
// Create Namespace container for functions in this library;
if (typeof(MCS.Cerner) === "undefined") {
    MCS.Cerner = {};
}

// #region Cerner Suppression
MCS.Cerner.DisplayCernerNotification = function (executionContext, entity) {
    'use strict';
    var formContext = executionContext.getFormContext();
    var text;

    if (entity === "site") {
        text = "At this time, please select a TMP site that has not migrated to Cerner Millennium.";
    } else if (entity === "facility") {
        text = "At this time, please select a facility that has not migrated to Cerner Millennium";
    } else if (entity === "participating site" || entity === "resource group") {
        text = "At this time, please select a TMP resource that is not related to a Cerner Millennium migrated facility";
    }

    //Display Error Message if Facility is Cerner
    var alertStrings = {
        confirmButtonLabel: "OK",
        text: text,
        title: "Error Message"
    };
    var alertOptions = {
        height: 150,
        width: 300
    };
    Xrm.Navigation.openAlertDialog(alertStrings, alertOptions).then(
    function success(result) {
        //console.log("Alert dialog closed");
    },
    function (error) {
        //console.log(error.message);
    });

};

//On-change of Patient or Provider SITE, check if site's facility is Cerner
MCS.Cerner.SiteCheckForCerner = function (executionContext) {
    var formContext = executionContext.getFormContext();

    var site = executionContext.getEventSource().getValue();
    var siteEntityType;
    var siteID;
    var facilityType;

    if (site !== null) {
        siteEntityType = site[0].entityType;
        siteID = (site[0].id).replace(/[{}]/g, "");

        Xrm.WebApi.retrieveRecord(siteEntityType, siteID, "?$expand=mcs_facilityid($select=cvt_facilitytype)").then(
        function success(result) {
            //console.log("Retrieved values: Facility Type: " + result.mcs_facilityid.cvt_facilitytype);
            facilityType = result.mcs_facilityid.cvt_facilitytype;

            if (facilityType === 917290000) {
                MCS.Cerner.DisplayCernerNotification(executionContext, "site");
                //Clear the field
                executionContext.getEventSource().setValue(null);

            }

        },
        function (error) {
            //console.log(error.message);
        });
    }

};

//On-Change of Facility
MCS.Cerner.FacilityCheckForCerner = function (executionContext) {
    console.log("Beginning CheckForCerner");
    var formContext = executionContext.getFormContext();

    var facility = executionContext.getEventSource().getValue();
    var facilityEntityType;
    var facilityID;
    var facilityType;

    if (facility !== null) {
        facilityEntityType = facility[0].entityType;
        facilityID = (facility[0].id).replace(/[{}]/g, "");

        Xrm.WebApi.retrieveRecord(facilityEntityType, facilityID, "?$select=cvt_facilitytype").then(
        function success(result) {
            //console.log("Retrieved values: Facility Type: " + result.cvt_facilitytype);
            facilityType = result.cvt_facilitytype;
            if (facilityType === 917290000) {
                MCS.Cerner.DisplayCernerNotification(executionContext, "facility");

                //Clear the field
                if (formContext.getAttribute("cvt_providerfacility").getValue() !== null && formContext.getAttribute("cvt_patientfacility").getValue() !== null) {
                    if (formContext.getAttribute("cvt_providerfacility").getValue() === formContext.getAttribute("cvt_patientfacility").getValue()) {
                        formContext.getAttribute("cvt_providerfacility").setValue(null);
                        formContext.getAttribute("cvt_patientfacility").setValue(null);
                    }
                    else {
                        executionContext.getEventSource().setValue(null);
                    }
                } else {
                    executionContext.getEventSource().setValue(null);
                }
            }
        },
        function (error) {
            //console.log(error.message);
        });
    }

    //console.log("Ending CheckforCerner");
};

//On-Load/On-Change of Participating SITE, check if site's facility is Cerner
MCS.Cerner.ParticipatingSiteCheckForCerner = function (executionContext) {
    var formContext = executionContext.getFormContext();

    var participatingSite = formContext.getAttribute("cvt_participatingsite").getValue();
    var participatingSiteEntityType;
    var participatingSiteID;
    var facilityType;

    if (participatingSite !== null) {
        participatingSiteEntityType = participatingSite[0].entityType;
        participatingSiteID = (participatingSite[0].id).replace(/[{}]/g, "");

        Xrm.WebApi.retrieveRecord(participatingSiteEntityType, participatingSiteID, "?$expand=cvt_facility($select=cvt_facilitytype)").then(
        function success(result) {
            //console.log("Retrieved values: Facility Type: " + result.cvt_facility.cvt_facilitytype);
            facilityType = result.cvt_facility.cvt_facilitytype;

            if (facilityType === 917290000) {
                MCS.Cerner.DisplayCernerNotification(executionContext, "participating site");
                //Clear the field
                formContext.getAttribute("cvt_participatingsite").setValue(null);
                //console.log("Field Cleared");
            }

        },
        function (error) {
            //console.log(error.message);
        });
    }

};

//On-Change of Resource Group, check if site's facility is Cerner
MCS.Cerner.ResourceGroupCheckForCerner = function (executionContext) {
    var formContext = executionContext.getFormContext();

    var resourceGroup = formContext.getAttribute("cvt_resourcegroup").getValue();
    var resourceGroupEntityType;
    var resourceGroupID;
    var siteId;
    var facilityType;

    if (resourceGroup !== null) {
        resourceGroupEntityType = resourceGroup[0].entityType;
        resourceGroupID = (resourceGroup[0].id).replace(/[{}]/g, "");
        //console.log(resourceGroupEntityType + ", " + resourceGroupID);
        Xrm.WebApi.retrieveRecord(resourceGroupEntityType, resourceGroupID, "?$expand=mcs_relatedsiteid($select=mcs_siteid)").then(
        function success(result) {
            console.log("Retrieved values: Facility ID: " + result.mcs_relatedsiteid.mcs_siteid);
            siteId = result.mcs_relatedsiteid.mcs_siteid;
            Xrm.WebApi.retrieveRecord("mcs_site", siteId, "?$expand=mcs_facilityid($select=cvt_facilitytype)").then(
            function success(result) {
                //console.log("Retrieved values: Facility Type: " + result.mcs_facilityid.cvt_facilitytype);
                facilityType = result.mcs_facilityid.cvt_facilitytype;

                if (facilityType === 917290000) {
                    MCS.Cerner.DisplayCernerNotification(executionContext, "resource group");
                    //Clear the field
                    formContext.getAttribute("cvt_resourcegroup").setValue(null);
                }

            },
            function (error) {
                //console.log(error.message);
            });

        },
        function (error) {
            //console.log(error.message);
        });
    }

};

// #endregion
