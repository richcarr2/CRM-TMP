//If the SDK namespace object is not defined, create it.
if (typeof MCS === "undefined")
    MCS = {};
// Create Namespace container for functions in this library;
MCS.mcs_Resource = {};

//Namespace Variables
var CachedCapacityValue = null;
var CachedVistaCapacityValue = null;
var alertOptions = { height: 200, width: 300 }; //added by Naveen Dubbaka
//Removes the Provider and Telepresenter options from the Option Set.
MCS.mcs_Resource.RemoveProTeleOptions = function (executionContext) {
    'use strict';
    var formContext = executionContext.getFormContext();
    //If mapped from Resource Group, don't allow Provider
    if (formContext.getAttribute("mcs_resourcegroupguid").getValue() != null) {
        switch (formContext.getAttribute("mcs_type").getValue()) {
            case 99999999: //Provider
                MCS.cvt_Common.closeWindow("A Provider cannot be added this way. Contact your administrator for questions.");
                break;
            case 100000000:
            case 917290000:
                formContext.getAttribute("mcs_type").setValue(null)
                break;
        }
    }
    var options = formContext.getControl("mcs_type");
    options.removeOption(100000000);
    options.removeOption(99999999);
    options.removeOption(917290000);

    //After removing options, if it still exists, lock it in.
    if (formContext.getAttribute("mcs_type").getValue() != null)
        formContext.getControl("mcs_type").setDisabled(true);

    //Submit if OnCreate
    if (formContext.ui.getFormType() === MCS.cvt_Common.FORM_TYPE_CREATE)
        formContext.getAttribute("mcs_type").setSubmitMode("always");
};

//Check Capacity Value
MCS.mcs_Resource.SetDefaultCapacity = function (executionContext) {
    //debugger;
    var formContext = executionContext.getFormContext();
    var resourceTypeValue = (formContext.getControl("mcs_type") != null) ? formContext.getAttribute("mcs_type").getValue() : null;

    if (resourceTypeValue === 251920001 && formContext.getAttribute("cvt_capacity").getValue() === null) {
        formContext.getAttribute("cvt_capacity").setValue(1);
        formContext.getAttribute("cvt_capacity").setSubmitMode("always");
    }

    if (resourceTypeValue === 251920003 && formContext.getAttribute("cvt_capacity").getValue() === null) {
        formContext.getAttribute("cvt_capacity").setValue(99999);
        formContext.getAttribute("cvt_capacity").setSubmitMode("always");
    }

    if (resourceTypeValue === 251920000 && formContext.getAttribute("cvt_vistacapacity").getValue() === null) {
        formContext.getAttribute("cvt_vistacapacity").setValue(1);
        formContext.getAttribute("cvt_vistacapacity").setSubmitMode("always");
    }
    CachedCapacityValue = formContext.getAttribute("cvt_capacity").getValue();
    CachedVistaCapacityValue = formContext.getAttribute("cvt_vistacapacity").getValue();
};

//Set grids which will retrieve all PS's Related to this Resource.
MCS.mcs_Resource.GetRelatedPSs = function (executionContext) {
    var formContext = executionContext.getFormContext();
    if (formContext.ui.getFormType() !== MCS.cvt_Common.FORM_TYPE_CREATE) {
        var PSGrid = document.getElementById("RelatedPS");
        if (PSGrid == null) { //make sure the grids have loaded 
            setTimeout(function () { MCS.mcs_Resource.GetRelatedPSs(); }, 500); //if the grid hasn’t loaded run this again when it has 
            return;
        }

        var ThisResourceName = formContext.getAttribute("mcs_name").getValue();
        var ThisResourceId = (formContext.data.entity.getId() != null) ? formContext.data.entity.getId() : MCS.cvt_Common.BlankGUID;

        PSGrid.control.SetParameter("fetchXml", MCS.mcs_Resource.GetFetchXML("cvt_schedulingresource", ThisResourceName, ThisResourceId)); //set the fetch xml to the sub grid   
        PSGrid.control.Refresh();

    }
};

//Set grid which will retrieve all SA's Related to this Resource.  
MCS.mcs_Resource.GetRelatedServiceActivities = function (executionContext) {
    var formContext = executionContext.getFormContext();
    if (formContext.ui.getFormType() !== MCS.cvt_Common.FORM_TYPE_CREATE) {
        var SAGrid = document.getElementById("RelatedServiceActivities");

        if (SAGrid == null) { //make sure the grids have loaded 
            setTimeout(function () { MCS.mcs_Resource.GetRelatedServiceActivities(); }, 500); //if the grid hasn’t loaded run this again when it has 
            return;
        }

        var ThisResourceId = (formContext.getAttribute("mcs_relatedresourceid").getValue() != null) ? formContext.getAttribute("mcs_relatedresourceid").getValue()[0].id : MCS.cvt_Common.BlankGUID;
        var ThisResourceName = (formContext.getAttribute("mcs_relatedresourceid").getValue() != null) ? formContext.getAttribute("mcs_relatedresourceid").getValue()[0].name : '';

        SAGrid.control.SetParameter("fetchXml", MCS.mcs_Resource.GetFetchXML("serviceappointment", ThisResourceName, ThisResourceId));
        SAGrid.control.Refresh();
    }
};

//TO DO Refactor into single function with above function
MCS.mcs_Resource.GetScheduledAppointments = function (executionContext) {
    var formContext = executionContext.getFormContext();
    if (formContext.ui.getFormType() !== MCS.cvt_Common.FORM_TYPE_CREATE) {
        var AptGrid = document.getElementById("Scheduled_Appointments");
        if (AptGrid == null) { //make sure the grids have loaded 
            setTimeout(function () { MCS.mcs_Resource.GetScheduledAppointments(); }, 500); //if the grid hasn’t loaded run this again when it has 
            return;
        }
        var ThisResourceId = (formContext.getAttribute("mcs_relatedresourceid").getValue() != null) ? formContext.getAttribute("mcs_relatedresourceid").getValue()[0].id : MCS.cvt_Common.BlankGUID;
        var ThisResourceName = (formContext.getAttribute("mcs_relatedresourceid").getValue() != null) ? formContext.getAttribute("mcs_relatedresourceid").getValue()[0].name : '';

        AptGrid.control.SetParameter("fetchXml", MCS.mcs_Resource.GetFetchXML("appointment", ThisResourceName, ThisResourceId));
        AptGrid.control.Refresh();
    }
};

//Get correct FetchXML - for grids
MCS.mcs_Resource.GetFetchXML = function (entityName, ThisResourceName, ThisResourceId) {
    var FetchXML = "";
    switch (entityName) {
        case "cvt_patientresourcegroup":
        case "cvt_providerresourcegroup":
            //fetch xml code which will retrieve all TSA's Related to this Resource.  
            var FetchXML =
                "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>" +
                "<entity name='mcs_services'>" +
                "<attribute name='mcs_name' />" +
                "<attribute name='createdon' />" +
                "<attribute name='cvt_relatedprovidersiteid' />" +
                "<attribute name='cvt_relatedpatientsiteid' />" +
                "<attribute name='cvt_relatedmasterid' />" +
                "<attribute name='cvt_servicetype' />" +
                "<attribute name='cvt_servicesubtype' />" +
                "<attribute name='statuscode' />" +
                "<attribute name='ownerid' />" +
                "<attribute name='modifiedon' />" +
                "<attribute name='cvt_servicescope' />" +
                "<attribute name='cvt_groupappointment' />" +
                "<attribute name='cvt_provsitevistaclinics' />" +
                "<attribute name='cvt_patsitevistaclinics' />" +
                "<attribute name='mcs_servicesid' />" +
                "<order attribute='mcs_name' descending='false' />" +
                "<filter type='and'>" +
                "<condition attribute='statecode' operator='eq' value='0' />" +
                "</filter>" +
                "<link-entity name='" + entityName + "' from='cvt_relatedtsaid' to='mcs_servicesid' alias='ai'>" +
                "<filter type='and'>" +
                "<condition attribute='cvt_relatedresourceid' operator='eq' uiname='" + MCS.cvt_Common.formatXML(ThisResourceName) + "' uitype='mcs_resource' value='" + ThisResourceId + "' />" +
                "</filter>" +
                "</link-entity>" +
                "</entity>" +
                "</fetch>";
            break;
        case "serviceappointment":
            var FetchXML =
                "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>" +
                "<entity name='activitypointer'>" +
                "<attribute name='activitytypecode' />" +
                "<attribute name='subject' />" +
                "<attribute name='statecode' />" +
                "<attribute name='prioritycode' />" +
                "<attribute name='modifiedon' />" +
                "<attribute name='activityid' />" +
                "<attribute name='instancetypecode' />" +
                "<order attribute='modifiedon' descending='false' />" +
                "<filter type='and'>" +
                "<condition attribute='activitytypecode' operator='eq' value='4214' />" +
                "<condition attribute='scheduledstart' operator='next-x-days' value='365' />" +
                "</filter>" +
                "<link-entity name='activityparty' from='activityid' to='activityid' alias='al'>" +
                "<filter type='and'>" +
                "<condition attribute='partyid' operator='eq' uiname='" + MCS.cvt_Common.formatXML(ThisResourceName) + "' uitype='equipment' value='" + ThisResourceId + "' />" +
                "</filter>" +
                "</link-entity>" +
                "</entity>" +
                "</fetch>";
            break;
        case "appointment":
            FetchXML =
                "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>" +
                "<entity name='activitypointer'>" +
                "<attribute name='activitytypecode' />" +
                "<attribute name='subject' />" +
                "<attribute name='statecode' />" +
                "<attribute name='prioritycode' />" +
                "<attribute name='modifiedon' />" +
                "<attribute name='activityid' />" +
                "<attribute name='instancetypecode' />" +
                "<order attribute='modifiedon' descending='false' />" +
                "<filter type='and'>" +
                "<condition attribute='activitytypecode' operator='eq' value='4201' />" +
                "<condition attribute='scheduledstart' operator='next-x-days' value='365' />" +
                "</filter>" +
                "<link-entity name='activityparty' from='activityid' to='activityid' alias='al'>" +
                "<filter type='and'>" +
                "<condition attribute='partyid' operator='eq' uiname='" + MCS.cvt_Common.formatXML(ThisResourceName) + "' uitype='equipment' value='" + ThisResourceId + "' />" +
                "</filter>" +
                "</link-entity>" +
                "</entity>" +
                "</fetch>";
            break;
        case "cvt_schedulingresource":
            //fetch xml code which will retrieve all PS's Related to this Resource.  
            FetchXML = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>" +
                "<entity name='cvt_participatingsite'>" +
                "<attribute name='cvt_name' />" +
                "<attribute name='cvt_site' />" +
                "<attribute name='cvt_resourcepackageid' />" +
                "<attribute name='cvt_locationtype' />" +
                "<attribute name='cvt_scheduleable' />" +
                "<attribute name='cvt_participatingsiteid' />" +
                "<attribute name='cvt_participatingsiteid' />" +
                "<order attribute='cvt_name' descending='false' />" +
                "<order attribute='cvt_resourcepackage' descending='false' />" +
                "<link-entity name='" + entityName + "' from='cvt_participatingsite' to='cvt_participatingsiteid' alias='ad'>" +
                "<filter type='and'>" +
                "<condition attribute='cvt_tmpresource' operator='eq' uiname='" + MCS.cvt_Common.formatXML(ThisResourceName) + "' uitype='mcs_resource' value='" + ThisResourceId + "' />" +
                "</filter>" +
                "</link-entity>" +
                "</entity>" +
                "</fetch>";
            break;
    }
    return FetchXML;
};

MCS.mcs_Resource.GetRequiredString = function (isRequired) {
    return isRequired ? "required" : "none";
};

//Check's Capacity Value
MCS.mcs_Resource.CheckCapacity = function (executionContext) {
    var formContext = executionContext.getFormContext();
    if (formContext.getAttribute("cvt_capacity").getValue() === 0) {
        //alert("Capacity must be greater than 0."); //Modified by Naveen Dubbaka
        var alertStrings = { confirmButtonLabel: "OK", text: "Capacity must be greater than 0.", title: "Capacity Rule" };
        Xrm.Navigation.openAlertDialog(alertStrings, alertOptions)
        formContext.getAttribute("cvt_capacity").setValue(CachedCapacityValue);
    }
};

MCS.mcs_Resource.SystemTypeOnChange = function (executionContext) {
    var formContext = executionContext.getFormContext();
    var cartType = formContext.getAttribute("cvt_carttypeid");
    cartType.setValue(null);
    MCS.mcs_Resource.SetCartTypeVisibility(executionContext);
};

MCS.mcs_Resource.SetCartTypeVisibility = function (executionContext) {
    var formContext = executionContext.getFormContext();
    var systemType = formContext.getAttribute("cvt_systemtype");
    var cartType = formContext.getAttribute("cvt_carttypeid");
    var cartTypeControl = formContext.getControl("cvt_carttypeid");
    if (cartType !== null && cartTypeControl !== null) {
        if (systemType !== null && systemType.getValue() === 917290001) {
            cartType.setRequiredLevel("required");
            cartTypeControl.setVisible(true);
        } else {
            cartType.setRequiredLevel("none");
            cartTypeControl.setVisible(false);
        }

        var msn = formContext.getAttribute("cvt_masterserialnumber");
        var msnControl = formContext.getControl("cvt_masterserialnumber");
        if (systemType !== null && systemType.getValue() === 917290003) {
            msn.setRequiredLevel("none");
            msnControl.setVisible(false);
        }
    }
    MCS.mcs_Resource.CartTypeOnChange(executionContext);
};

MCS.mcs_Resource.CartTypeOnChange = function (executionContext) {
    var formContext = executionContext.getFormContext();
    var cartType = formContext.getAttribute("cvt_carttypeid");
    var lastCalibration = formContext.getAttribute("cvt_carttypeid");
    var lastCalibrationControl = formContext.getControl("cvt_lastcalibrationdate");

    if (cartType != null && lastCalibrationControl != null) {
        if (cartType.getValue() == null) {
            lastCalibrationControl.setVisible(false);
            //lastCalibration.setRequiredLevel("none");
            //lastCalibration.setValue(null);
        }
        else {
            var cartTypeObjValue = cartType.getValue();//Check for Lookup Value
            var lookupRecordName = cartTypeObjValue[0].name; //To get record Name 
            lastCalibrationControl.setVisible((lookupRecordName.toLowerCase() === "audiology") ? true : false);
        }
    }
};

//Create record Name
MCS.mcs_Resource.CreateName = function (executionContext) {
    //debugger;
    var formContext = executionContext.getFormContext();
    var mcs_usernameinput = formContext.getAttribute("mcs_usernameinput").getValue();
    var derivedResultField = "";

    switch (formContext.getAttribute("mcs_type").getValue()) {
        case 251920001:
            var building = formContext.getAttribute("cvt_building").getValue();
            var room = formContext.getAttribute("mcs_room").getValue();

            derivedResultField = (building !== null) ? "Bldg. " + building : "";
            derivedResultField += (derivedResultField !== "" && room !== null) ? ", " : "";
            derivedResultField += (room !== null) ? "Room " + room : "";

            if ((derivedResultField === "") && (mcs_usernameinput !== null))
                derivedResultField = mcs_usernameinput;
            break;
        case 251920002:
            var componentType = formContext.getAttribute("cvt_componenttype").getValue();
            var systemType = formContext.getAttribute("cvt_systemtype").getValue();
            var uniqueid = formContext.getAttribute("cvt_uniqueid").getValue();

            derivedResultField += (systemType != null) ? formContext.getAttribute("cvt_systemtype").getText() : "";
            derivedResultField += (componentType != null) ? formContext.getAttribute("cvt_componenttype").getText() : "";
            derivedResultField += (uniqueid != null) ? ": " + uniqueid : "";
            break;
        case 251920003:
            var cernerUniqueId = formContext.getAttribute("mcs_cerneruniqueid").getValue();
            derivedResultField = (cernerUniqueId !== null) ? cernerUniqueId : "Default Unspecified TCT/Telepresenter";
            break;
        default:
            derivedResultField += (mcs_usernameinput != null) ? mcs_usernameinput : "";
            break;
    }

    if (formContext.getAttribute("mcs_relatedsiteid").getValue() !== null)
        derivedResultField += " @ " + formContext.getAttribute("mcs_relatedsiteid").getValue()[0].name;
    if (formContext.getAttribute("mcs_type").getValue() === 251920002) {
        var roomnumber = formContext.getAttribute("cvt_room").getValue();
        derivedResultField += " Rm. " + (roomnumber === null ? "" : roomnumber);
    }

    if (formContext.getAttribute("mcs_name").getValue !== derivedResultField) {
        formContext.getAttribute("mcs_name").setSubmitMode("always");
        formContext.getAttribute("mcs_name").setValue(derivedResultField);
    }
};

//Disable fields
MCS.mcs_Resource.MakeFieldsReadOnly = function (executionContext) {
    var formContext = executionContext.getFormContext();
    if (formContext.ui.getFormType() !== MCS.cvt_Common.FORM_TYPE_CREATE) {
        formContext.getControl("mcs_type").setDisabled(true);
        if (formContext.getAttribute("mcs_type").getValue() !== 251920002)
            formContext.getControl("mcs_relatedsiteid").setDisabled(true);
    }
};

//Set standard fields to submit
//TODO: get rid of this function, make it submit pertinent fields only.
MCS.mcs_Resource.SubmitFields = function (executionContext) {
    //debugger;
    var formContext = executionContext.getFormContext();
    formContext.getAttribute("mcs_name").setSubmitMode("always");
    formContext.getAttribute("cvt_updateresourceconnections").setSubmitMode("always");
    formContext.getAttribute("cvt_replaceresourceconnections").setSubmitMode("always");
    formContext.getAttribute("cvt_deleteresourceconnections").setSubmitMode("always");
    formContext.getAttribute("cvt_replacementresource").setSubmitMode("always");
};//If the SDK namespace object is not defined, create it.

MCS.mcs_Resource.CheckIEN = function (ec) {
    var formContext = ec.getFormContext();
    var saveEvent = ec.getEventArgs();
    var regex = /^[0-9]+$/gm;
    var strIEN = formContext.getAttribute("cvt_ien").getValue();
    var rType = formContext.getAttribute("mcs_type").getValue();

    if (rType === 251920000) {
        var _result = regex.test(strIEN);
        //console.log("regex result =" + _result);//Modified by Naveen Dubbaka

        if (!_result) {
            //alert("The Vista IEN field contains non-numeric characters.  Please correct and resave."); //Modified by Naveen Dubbaka
            var alertStrings = { confirmButtonLabel: "OK", text: "The Vista IEN field contains non-numeric characters.  Please correct and resave.", title: "Vista Rule" };
            Xrm.Navigation.openAlertDialog(alertStrings, alertOptions)
            saveEvent.preventDefault();
        }
    }
};

//Displays different tabs based on the Resource Type selected
MCS.mcs_Resource.ResourceType = function (executionContext) {
    var formContext = executionContext.getFormContext();
    var resourceTypeValue = (formContext.getControl("mcs_type") != null) ? formContext.getAttribute("mcs_type").getValue() : null;
    var uniqueIdControl = formContext.getControl("mcs_usernameinput");
    var componentType = formContext.getAttribute("cvt_componenttype");
    var componentTypeControl = formContext.getControl("cvt_componenttype");
    var systemType = formContext.getAttribute("cvt_systemtype");

    var resourceTypeIsVista = resourceTypeValue === 251920000;
    var resourceTypeIsRoom = resourceTypeValue === 251920001;
    var resourceTypeIsTech = resourceTypeValue === 251920002;
    var resourceTypeIsTCT = resourceTypeValue === 251920003;

    //Vista Clinic Fields/Controls
    uniqueIdControl.setVisible(resourceTypeIsVista);
    uniqueIdControl.setDisabled(!resourceTypeIsVista);
    formContext.getAttribute("mcs_usernameinput").setRequiredLevel(MCS.mcs_Resource.GetRequiredString(resourceTypeIsVista));
    formContext.getControl("cvt_vistacapacity").setVisible(resourceTypeIsVista);
    formContext.getControl("cvt_ien").setVisible(resourceTypeIsVista);
    formContext.ui.tabs.get("tab_vista").setVisible(resourceTypeIsVista);
    formContext.getAttribute("cvt_ien").setRequiredLevel(MCS.mcs_Resource.GetRequiredString(resourceTypeIsVista));

    //Room Based Fields/Controls
    formContext.ui.tabs.get("tab_room_information").setVisible(resourceTypeIsRoom);
    formContext.getAttribute("mcs_room").setRequiredLevel(MCS.mcs_Resource.GetRequiredString(resourceTypeIsRoom));

    //TCT Based Fields/Controls
    formContext.getAttribute("mcs_cerneruniqueid").setRequiredLevel(MCS.mcs_Resource.GetRequiredString(resourceTypeIsTCT));


    //Tech Based Fields/Controls
    formContext.ui.tabs.get("tab_technology_information").setVisible(resourceTypeIsTech);
    formContext.ui.tabs.get("tab_9").setVisible(resourceTypeIsTech);
    formContext.getAttribute("cvt_systemtype").setRequiredLevel(MCS.mcs_Resource.GetRequiredString(resourceTypeIsTech));
    formContext.getAttribute("cvt_uniqueid").setRequiredLevel(MCS.mcs_Resource.GetRequiredString(resourceTypeIsTech));
    formContext.getAttribute("cvt_masterserialnumber").setRequiredLevel(MCS.mcs_Resource.GetRequiredString(resourceTypeIsTech));
    formContext.getControl("cvt_masterserialnumber").setVisible(resourceTypeIsTech);
    if (resourceTypeIsTech) {
        var systemTypeValue = (formContext.getControl("cvt_systemtype") != null) ? systemType.getValue() : null;
        if (systemTypeValue === 100000000) {
            componentType.setRequiredLevel("required");
            componentTypeControl.setVisible(true);
        }
        else {
            componentType.setRequiredLevel("none");
            componentType.setValue(null);
            componentTypeControl.setVisible(false);
        }

        //Prepopulate for CVT Patient Tablet
        if (formContext.getAttribute("cvt_systemtype").getValue() === 917290005) {
            if (formContext.getAttribute("cvt_locationuse").getValue() != null)
                formContext.getAttribute("cvt_locationuse").setValue(917290001);

            formContext.getAttribute("cvt_room").setValue("");
            formContext.getControl("cvt_room").setDisabled(true);
            formContext.getAttribute("cvt_room").setSubmitMode("always");
        }
        else
            formContext.getControl("cvt_room").setDisabled(false);
    }
    //Xrm.Page.getAttribute("cvt_systemindex").setRequiredLevel("none");
};


MCS.mcs_Resource.TriggerMergeButton = function (primaryControl, selectedIds) {
    //debugger;


    var selectedRecord1 = MCS.mcs_Resource.FetchRecord(selectedIds[0]);
    var selectedRecord2 = MCS.mcs_Resource.FetchRecord(selectedIds[1]);


    if (selectedRecord1 != null && selectedRecord1 != undefined && selectedRecord2 != null && selectedRecord2 != undefined && selectedRecord1['mcs_facility'] != null && selectedRecord2['mcs_facility'] != null) {

        var ien1 = selectedRecord1["cvt_ien"];
        var ien2 = selectedRecord2["cvt_ien"];

        var name1 = selectedRecord1["mcs_name"];
        var name2 = selectedRecord2["mcs_name"];

        var stationnumber1 = selectedRecord1['mcs_facility']['mcs_stationnumber'];
        var stationnumber2 = selectedRecord2['mcs_facility']['mcs_stationnumber'];



        if (ien1 === ien2 && stationnumber1 === stationnumber2) {

            window.formContext = primaryControl;
            var dialogParameters = {
                pageType: "webresource",
                webresourceName: "tmp_MergeRecords",
                data: "primaryId=" + selectedIds[0] + "&secondaryId=" + selectedIds[1] + "&entityLogicalName=mcs_resource" // + "&formContext=" + formContext
            };

            var navigationOptions = {
                target: 2,
                width: { value: 60, unit: "%" },
                position: 1,
                title: "Merge Records"
            };

            Xrm.Navigation.navigateTo(dialogParameters, navigationOptions).then(
                function (returnValue) {
                    if (!returnValue) {
                        return;
                    }
                    primaryControl.refresh();
                },
                function (e) {

                });

        } else {

            var OptionalParameter = { height: 200, width: 200 };
            var alertStrings =
            {
                confirmButtonLabel: "OK",
                text: "Only VistA Clinics from Facilities with the same station number and IEN can be merged. Please reselect ",
                title: "Business Process Error"

            };
            Xrm.Navigation.openAlertDialog(alertStrings, OptionalParameter)
        }
    }

}

MCS.mcs_Resource.ShowHideMergeButton = function (formContext, selectedIds) {

    var flag = false;
    var selectedRecord1 = MCS.mcs_Resource.FetchRecord(selectedIds[0]);
    var selectedRecord2 = MCS.mcs_Resource.FetchRecord(selectedIds[1]);


    if (selectedRecord1 != null && selectedRecord1 != undefined && selectedRecord2 != null && selectedRecord2 != undefined) {
        var type1 = selectedRecord1["mcs_type"];
        var type2 = selectedRecord2["mcs_type"];

        if (type1 === 251920000 && type2 === 251920000) {
            flag = true;
        }
    }

    var roleCheck = MCS.mcs_Resource.CheckAdmin();

    if (flag && roleCheck) {
        return true;
    } else {
        return false;
    }

};


MCS.mcs_Resource.FetchRecord = function (guid) {

    var result = null;

    var req = new XMLHttpRequest();
    req.open("GET", Xrm.Utility.getGlobalContext().getClientUrl() + "/api/data/v9.1/mcs_resources(" + guid + ")?$select=mcs_name,mcs_type,cvt_ien&$expand=mcs_facility($select=mcs_stationnumber)", false);
    req.setRequestHeader("OData-MaxVersion", "4.0");
    req.setRequestHeader("OData-Version", "4.0");
    req.setRequestHeader("Accept", "application/json");
    req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
    req.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
    req.onreadystatechange = function () {
        if (this.readyState === 4) {
            req.onreadystatechange = null;
            if (this.status === 200) {
                result = JSON.parse(this.response);

            }
        }
    };
    req.send();

    return result;
}

MCS.mcs_Resource.CheckAdmin = function () {
    "use strict";
    var flag = false;
    var currentUserRoles = Xrm.Utility.getGlobalContext().userSettings.roles._collection;
    var role = "System Administrator";
    role = role.toLowerCase();
    var lengthCount = Object.keys(currentUserRoles).length;
    if (lengthCount > 0) {
        for (var coll in currentUserRoles) {
            var userRoleId = currentUserRoles[coll];
            if (userRoleId.name.toLowerCase() == role) {
                flag = true;
            }
        }
    }
    return flag;
}