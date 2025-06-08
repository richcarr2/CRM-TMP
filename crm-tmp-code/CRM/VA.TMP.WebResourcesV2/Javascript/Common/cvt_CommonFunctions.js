//Library Name: cvt_CommonFunctions.js
//If the SDK namespace object is not defined, create it.
//debugger;
if (typeof MCS === "undefined") { MCS = {}; }
// Create Namespace container for functions in this library;
if (typeof MCS.cvt_Common === "undefined") { MCS.cvt_Common = {}; }

//Form Types
MCS.cvt_Common.FORM_TYPE_CREATE = 1;
MCS.cvt_Common.FORM_TYPE_UPDATE = 2;
MCS.cvt_Common.FORM_TYPE_READ_ONLY = 3;
MCS.cvt_Common.FORM_TYPE_DISABLED = 4;
MCS.cvt_Common.FORM_TYPE_QUICKCREATE = 5;
MCS.cvt_Common.FORM_TYPE_BULKEDIT = 6;

MCS.cvt_Common.saveFromConfirmDialog = false;

MCS.cvt_Common.BlankGUID = "00000000-0000-0000-0000-000000000000";

MCS.cvt_Common.AppointmentOccursInPast = function (executionContext) {
    'use strict';
    var formContext = executionContext.getFormContext();
    if (formContext.ui.getFormType() === MCS.cvt_Common.FORM_TYPE_CREATE)
        return false;
    var startTimeObj = formContext.getAttribute("scheduledstart");
    if (startTimeObj == null)
        return false;
    var startTime = startTimeObj.getValue();
    if (startTime == null)
        return false;
    var now = new Date();
    if (now > startTime)
        return true;
    else
        return false;
};

//Get Server URL
MCS.cvt_Common.BuildRelationshipServerUrl = function () {
    var globalContext = Xrm.Utility.getGlobalContext();
    var server = globalContext.getClientUrl();
    // var server = Xrm.Page.context.getClientUrl();
    if (server.match(/\/$/)) {
        server = server.substring(0, server.length - 1);
    }
    return server;
};

//Check if Obj is null else get Value
MCS.cvt_Common.checkNull = function (executionContext, fieldname) {
    var formContext = executionContext.getFormContext();
    var fieldObj = formContext.getAttribute(fieldname);

    if (fieldObj != null)
        return fieldObj.getValue();

    return null;
};

//Close window
MCS.cvt_Common.closeWindow = function (executionContext, msg) {
    var formContext = executionContext.getFormContext();
    //if (msg != null)
    //  alert(msg);
    //Clear all fields so there are no dirty fields
    var attributes = formContext.data.entity.attributes.get();
    for (var i in attributes) {
        attributes[i].setSubmitMode("never");
    }
    //Close record         
    formContext.ui.close();
};

MCS.cvt_Common.fireChange = function (executionContext, field) {
    var formContext = executionContext.getFormContext();
    /*********************************************************************
    USAGE:  Causes 'onChange' event to fire on a related field.  Typically
    would be called to initiate onChange event for a field changed 
    programmatically (and which would not have a "real" onChange fired)
    *********************************************************************/
    var ctlControl = formContext.getControl(field);

    formContext.getAttribute(ctlControl).fireOnChange();

}

//collapse a tab
MCS.cvt_Common.collapseTab = function (executionContext, tab, field) {
    var formContext = executionContext.getFormContext();
    /*********************************************************************
    USAGE: collapses/expands a tab based upon whether a control is empty.
    Pass in the schema name of the tab and the name of the field to check

    Example:
    //tab name: "tab_9"  <--schema name is what we want passed in
    //mcs_relatedtsa  <--pass in the field name to check

    **********************************************************************/

    var ctlControl = formContext.getControl(field);
    var atrControl = ctlControl.getAttribute();
    var valControl = atrControl.getValue();

    var tabObj = formContext.ui.tabs.get(tab);

    if (valControl !== "" && valControl !== null) {
        tabObj.setDisplayState("expanded");
    }
    else {
        tabObj.setDisplayState("collapsed");
    }
};

MCS.cvt_Common.collapse2Tab = function (executionContext, tab1, tab2) {
    var formContext = executionContext.getFormContext();
    /*********************************************************************
    USAGE: collapses/expands a tab based upon whether a control is empty.
    Pass in the schema name of the tab and the name of the field to check

    Example:
    //tab name: "tab_9"  <--schema name is what we want passed in
    //mcs_relatedtsa  <--pass in the field name to check

    **********************************************************************/
    var field = "serviceid";
    var ctlControl = formContext.getControl(field);
    var atrControl = ctlControl.getAttribute();
    var valControl = atrControl.getValue();

    var tabObj1 = formContext.ui.tabs.get(tab1);
    var tabObj2 = formContext.ui.tabs.get(tab2);


    if (valControl !== "" && valControl !== null) {
        tabObj1.setDisplayState("expanded");
        tabObj2.setVisible(false);
    }
    else {
        tabObj1.setDisplayState("collapsed");
        tabObj2.setVisible(true);
    }
};


//Check if GUIDS are the same
MCS.cvt_Common.compareGUIDS = function (guid1, guid2) {
    if (guid1 == null && guid2 == null)
        return true;

    if (guid1 == null || guid2 == null)
        return false;

    var guid1Cleaned = guid1.replace(/\W/g, '');
    guid1Cleaned = guid1Cleaned.toString().toUpperCase();

    var guid2Cleaned = guid2.replace(/\W/g, '');
    guid2Cleaned = guid2Cleaned.toString().toUpperCase();

    if (guid1Cleaned === guid2Cleaned)
        return true;
    else
        return false;
};

//Venkat Comment- changed method with WebApi
//Change a Record's Status
MCS.cvt_Common.changeRecordStatus = function (executionContext, recordId, entityLogicalName, statecode, statuscode) {

    //Remove brackets from the GUID if there’s any
    var id = recordId.replace("{", "").replace("}", "");
    // Set statecode and statuscode
    var data = {
        "statecode": statecode,
        "statuscode": statuscode
    };
    // WebApi call
    Xrm.WebApi.updateRecord(entityLogicalName, id, data).then(
        function success(result) {
            executionContext.getFormContext().data.refresh(true);
        },
        function (error) {
            alert(error);
        });
};
/*    

//Change a Record's Status
MCS.cvt_Common.changeRecordStatus = function (executionContext, RECORD_ID, Entity_Name, stateCode, statusCode) {
    var formContext = executionContext.getFormContext();
    var globalContext = Xrm.Utility.getGlobalContext();
    var url = globalContext.getClientUrl();
    //var url = Xrm.Page.context.getClientUrl();

    // create the SetState request
    var request = "<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\">";
    request += "<s:Body>";
    request += "<Execute xmlns=\"http://schemas.microsoft.com/xrm/2011/Contracts/Services\" xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\">";
    request += "<request i:type=\"b:SetStateRequest\" xmlns:a=\"http://schemas.microsoft.com/xrm/2011/Contracts\" xmlns:b=\"http://schemas.microsoft.com/crm/2011/Contracts\">";
    request += "<a:Parameters xmlns:c=\"http://schemas.datacontract.org/2004/07/System.Collections.Generic\">";
    request += "<a:KeyValuePairOfstringanyType>";
    request += "<c:key>EntityMoniker</c:key>";
    request += "<c:value i:type=\"a:EntityReference\">";
    request += "<a:Id>" + RECORD_ID + "</a:Id>";
    request += "<a:LogicalName>" + Entity_Name + "</a:LogicalName>";
    request += "<a:Name i:nil=\"true\" />";
    request += "</c:value>";
    request += "</a:KeyValuePairOfstringanyType>";
    request += "<a:KeyValuePairOfstringanyType>";
    request += "<c:key>State</c:key>";
    request += "<c:value i:type=\"a:OptionSetValue\">";
    request += "<a:Value>" + stateCode + "</a:Value>";
    request += "</c:value>";
    request += "</a:KeyValuePairOfstringanyType>";
    request += "<a:KeyValuePairOfstringanyType>";
    request += "<c:key>Status</c:key>";
    request += "<c:value i:type=\"a:OptionSetValue\">";
    request += "<a:Value>" + statusCode + "</a:Value>";
    request += "</c:value>";
    request += "</a:KeyValuePairOfstringanyType>";
    request += "</a:Parameters>";
    request += "<a:RequestId i:nil=\"true\" />";
    request += "<a:RequestName>SetState</a:RequestName>";
    request += "</request>";
    request += "</Execute>";
    request += "</s:Body>";
    request += "</s:Envelope>";
    //send set state request
    $.ajax({
        type: "POST",
        contentType: "text/xml; charset=utf-8",
        datatype: "xml",
        url: url + "/XRMServices/2011/Organization.svc/web",
        data: request,
        beforeSend: function (XMLHttpRequest) {
            */
//   XMLHttpRequest.setRequestHeader("Accept", "application/xml, text/xml, */*");
/*
            XMLHttpRequest.setRequestHeader("SOAPAction", "http://schemas.microsoft.com/xrm/2011/Contracts/Services/IOrganizationService/Execute");
        },
        success: function (data, textStatus, XmlHttpRequest) {
            formContext.data.refresh();
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            alert(errorThrown);
        }
    });
};
*/
//Create Fetch
MCS.cvt_Common.CreateFetch = function (entityName, columns, conditions, order) {
    var formattedColumns = '';
    var formattedConditions = '';
    var formattedOrder = '';

    //columns is an array, so that we can build that string with the xml tags
    if (columns != null && columns.length > 0) {
        for (column in columns) {
            formattedColumns += '<attribute name="' + columns[column] + '" />';
        }
    }
    //prefix filter type and add conditions
    if (conditions != null && conditions.length > 0) {
        formattedConditions = "<filter type='and'>";
        for (condition in conditions) {
            formattedConditions += conditions[condition];
        }
    }
    //format order
    if (order !== null && order.length === 2)
        formattedOrder = '<order attribute="' + order[0] + '" descending="' + order[1] + '" />';

    var fetchXml = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true' aggregate='false'>";
    fetchXml += "<entity name='" + entityName + "'>";
    fetchXml += formattedColumns;
    fetchXml += formattedOrder;
    fetchXml += formattedConditions;
    fetchXml += "</filter>";
    fetchXml += "</entity>";
    fetchXml += "</fetch>";

    return fetchXml;
};

MCS.cvt_Common.DateTime = function (executionContext, attributeName, hour, minute) {
    var formContext = executionContext.getFormContext();
    var attribute = formContext.getAttribute(attributeName);
    if (attribute.getValue() == null) {
        attribute.setValue(new Date());
    }
    attribute.setValue(attribute.getValue().setHours(hour, minute, 0));
};

//Used for Specialty Subtype based off of Subtype
MCS.cvt_Common.EnableDependentLookup = function (executionContext, primaryLU, secondaryLU) {
    var formContext = executionContext.getFormContext();
    var primaryLUattribute = formContext.getAttribute(primaryLU);
    var primaryLUvalue = primaryLUattribute != null ? primaryLUattribute.getValue() : null;
    var primaryLUvalueproperty = primaryLUvalue != null ? primaryLUvalue[0].name : null;

    if (primaryLUvalueproperty != null) {
        formContext.getControl(secondaryLU).setVisible(true);
        formContext.getControl(secondaryLU).setFocus();
    }
    else {
        formContext.getControl(secondaryLU).setVisible(false);
        formContext.getAttribute(secondaryLU).setValue(null);
    }
};

MCS.cvt_Common.EnableOtherDetails = function (executionContext, source, target, value) {
    var formContext = executionContext.getFormContext();
    var targetFieldControl = formContext.ui.controls.get(target);
    var targetFieldObject = formContext.getAttribute(target);
    var sourceValue = formContext.getAttribute(source).getValue();
    if (sourceValue !== null && sourceValue.toString() === value) {
        targetFieldControl.setDisabled(false);
        targetFieldControl.setVisible(true);
        targetFieldObject.setRequiredLevel("required");
        targetFieldObject.setSubmitMode("dirty");
    }
    else {
        if (targetFieldObject.getValue() !== "") {
            targetFieldObject.setValue("");
            targetFieldObject.setSubmitMode("always");
        }
        targetFieldControl.setDisabled(true);
        targetFieldControl.setVisible(false);
        targetFieldObject.setRequiredLevel("none");
    }
};

//XML Fix - replace & with &amp;
MCS.cvt_Common.formatXML = function (str) {
    if (str) {
        str = str.replace(/&/g, "&amp;");
        return str;
    }
};

//Gets the EntityTypeCode / ObjectTypeCode of a entity
MCS.cvt_Common.getObjectTypeCode = function (entityName) {
    var lookupService = new parent.RemoteCommand("LookupService", "RetrieveTypeCode");
    lookupService.SetParameter("entityName", entityName);
    var result = lookupService.Execute();
    if (result.Success && typeof result.ReturnValue === "number") {
        return result.ReturnValue;
    } else {
        return null;
    }
};

//MCS.cvt_Common.JSDebugAlert = function (msg) {
//    Set showAlerts to false to stop showing Alerts
//    var showAlerts = false;

//    if (showAlerts == true) {
//        if (msg != null) {
//            alert("JS Debug Message: \n\n" + msg);
//        }
//    }
//};

MCS.cvt_Common.MVIConfig = function () {
    var deferred = $.Deferred();
    var returnData = {
        success: false,
        data: {}
    };
    var roles = "";
    var MVIConfig = false;
    var filter = "mcs_name eq 'Active Settings'";
    Xrm.WebApi.retrieveMultipleRecords("mcs_setting", "?$select=cvt_usemvi,cvt_mviroles&$filter=" + filter).then(
        function success(result) {
            if (result !== null && result.entities.length !== 0) {
                MVIConfig = result.entities[0].cvt_usemvi != null ? result.entities[0].cvt_usemvi : false;
                roles = result.entities[0].cvt_mviroles;
            }
            // var roleCheck = MCS.cvt_Common.userHasRoleInList(roles);
            var roleCheckretrieveTokenDeferred = MCS.cvt_Common.userHasRoleInList(roles);
            $.when(roleCheckretrieveTokenDeferred).done(function (returnData) {
                //return MVIConfig && roleCheck;
                var roleCheck = returnData.data.result;
                returnData.success = true;
                returnData.data.result = MVIConfig && roleCheck;
                deferred.resolve(returnData);
            },
                function (error) {
                    //return MVIConfig;
                    returnData.success = false;
                    deferred.resolve(returnData);

                }
            );

        }
    );
    return deferred.promise();
};


//UNSUPPORTED: Add Message to Notifications area
MCS.cvt_Common.Notifications = function (action, icon, message) {
    var notificationsList = Sys.Application.findComponent('crmNotifications');

    switch (action) {
        case "Add":
            if (notificationsList && icon && message)
                notificationsList.AddNotification('noteId1', icon, 'namespace', message);
            break;
        case "Hide":
            notificationsList.SetVisible(false);
            break;
    }
};

MCS.cvt_Common.openDialogOnCurrentRecord = function (primaryControl, dialogId) {
    //added by NAveen & Justin 2/15/2021
    //var formContext = null;
    //if (primaryControl !== null) {
    //    if (typeof primaryControl.getAttribute === 'function') {
    //        formContext = primaryControl; //called from the ribbon.
    //    } else if (typeof primaryControl.getFormContext === 'function'
    //        && typeof (primaryControl.getFormContext()).getAttribute === 'function') {
    //        formContext = primaryControl.getFormContext(); // most likely called from the form via a handler
    //    }
    //}​​​​​​​
    //var formContext = primaryControl.getFormContext(); //Commented by Naveen 2/15/2021
    //var formContext = primaryControl.getFormContext();

    var formContext;
    //check to see if the execution context or the form context was passed in
    //and set the formContext variable
    if (!primaryControl.getFormContext) {
        formContext = primaryControl;        
    }
    else {
        formContext = primaryControl.getFormContext();
    }

    EntityName = formContext.data.entity.getEntityName();
    objectId = formContext.data.entity.getId();
    return MCS.cvt_Common.openDialogProcess(formContext, dialogId, EntityName, objectId);
};

MCS.cvt_Common.openDialogProcess = function (primaryControl, dialogId, EntityName, objectId) {
    //added by NAveen & Justin  2/15 /2021
    //var formContext = null;
    //if (primaryControl !== null) {  
    //    if (typeof primaryControl.getAttribute === 'function') {
    //        formContext = primaryControl; //called from the ribbon.
    //    } else if (typeof primaryControl.getFormContext === 'function'
    //        && typeof (primaryControl.getFormContext()).getAttribute === 'function') {
    //        formContext = primaryControl.getFormContext(); // most likely called from the form via a handler
    //    }
    //}​​​​​​​

    //var formContext = primaryControl.getFormContext();// Commented by Naveen 2/15/2021

    var formContext;
    //check to see if the execution context or the form context was passed in
    //and set the formContext variable
    if (!primaryControl.getFormContext) {
        formContext = primaryControl;
    }
    else {
        formContext = primaryControl.getFormContext();
    }

    if (EntityName === null || EntityName === "")
        EntityName = formContext.data.entity.getEntityName();
    if (objectId === null || objectId === "")
        objectId = formContext.data.entity.getId();
    var globalContext = Xrm.Utility.getGlobalContext();
    var url = globalContext.getClientUrl() +
        //var url = Xrm.Page.context.getClientUrl() +
        "/cs/dialog/rundialog.aspx?DialogId=" +
        dialogId + "&EntityName=" +
        EntityName + "&ObjectId=" +
        objectId;
    var width = 400;
    var height = 400;
    var left = (screen.width - width) / 2;
    var top = (screen.height - height) / 2;
    return window.open(url, '', 'location=0,menubar=1,resizable=1,width=' + width + ',height=' + height + ',top=' + top + ',left=' + left + '');
};

MCS.cvt_Common.RestError = function (err) {
    return JSON.parse(err.responseText).error.message.value;
};

//From the Site, Set Facility
MCS.cvt_Common.SetFacilityFromSite = function (executionContext, siteFieldName, facilityFieldName) {
    var formContext = executionContext.getFormContext();
    var siteField = formContext.getAttribute(siteFieldName);
    var facilityField = formContext.getAttribute(facilityFieldName);
    var priorFacilityValue = facilityField.getValue() != null ? facilityField.getValue()[0].id : null;
    var siteValue = siteField.getValue() != null ? siteField.getValue()[0].id : null;

    if (siteValue != null) {
        //Get Parent Facility of Site

        Xrm.WebApi.retrieveRecord("mcs_site", siteValue, "?$select=mcs_FacilityId").then(
            function success(result) {
                if (result && result.mcs_FacilityId) {
                    //Check and Set Facility
                    var value = new Array();
                    value[0] = new Object();
                    value[0].id = '{' + result.mcs_FacilityId.Id + '}';
                    value[0].name = result.mcs_FacilityId.Name;
                    value[0].entityType = "mcs_facility";

                    //Set Facility field
                    facilityField.setValue(value);
                }
            },
            function (error) {
            }
        );

        //var calls = CrmRestKit.Retrieve("mcs_site", siteValue, ['mcs_FacilityId'], false);
        //calls.fail(
        //        function (error) {
        //        }).done(function (data) {
        //            if (data && data.d && data.d.mcs_FacilityId) {
        //                //Check and Set Facility
        //                var value = new Array();
        //                value[0] = new Object();
        //                value[0].id = '{' + data.d.mcs_FacilityId.Id + '}';
        //                value[0].name = data.d.mcs_FacilityId.Name;
        //                value[0].entityType = "mcs_facility";

        //                //Set Facility field
        //                facilityField.setValue(value);
        //            }
        //        });
    }
    else {
        //Clear Facility field
        facilityField.setValue(null);
    }
    if (MCS.cvt_Common.compareGUIDS(priorFacilityValue, ((facilityField.getValue() != null) ? facilityField.getValue()[0].id : null)) !== true)
        facilityField.setSubmitMode("always");
};

MCS.cvt_Common.TrimBookendBrackets = function (stringVar) {
    if (stringVar !== null && stringVar.length > 0)
        return stringVar.charAt(0) == '{' ? stringVar.slice(1, stringVar.length - 1) : stringVar;
    else
        return "";
};


if (typeof jQuery !== 'undefined' && typeof $ !== 'undefined') {
    if (jQuery.when.all === undefined) {
        jQuery.when.all = function (deferreds) {
            var deferred = new jQuery.Deferred();
            $.when.apply(jQuery, deferreds).then(
                function () {
                    deferred.resolve(Array.prototype.slice.call(arguments));
                },
                function () {
                    deferred.fail(Array.prototype.slice.call(arguments));
                });

            return deferred;
        }
    }
}

//Check if the passed in User has a particular role
MCS.cvt_Common.userHasRoleInList = function (roles) {
    var deferred = $.Deferred();
    var returnData = {
        success: false,
        data: {}
    };
    var userRoles = Xrm.Utility.getGlobalContext().userSettings.securityRoles;
    //var userRoles = Xrm.Page.context.getUserRoles();
    var hasRole = false;
    var deferreds = [];
    for (var i = 0; i < userRoles.length; i++) {
        if (hasRole) {
            return true;
        }
        var currentUserRole = userRoles[i];
        var localDeferred1 = getCurrentUserRole(roles, currentUserRole);

        deferreds.push(localDeferred1);

        //CrmRestKit.Retrieve('Role', userRoles[i], ['Name'], false).fail(
        //    function (err) {
        //        return;
        //    }).done(
        //    function (data) {
        //        if (data != null && data.d != null) {
        //            var roleName = data.d.Name.trim().toLowerCase();
        //            if (roles.toLowerCase().indexOf(roleName) != -1) {
        //                hasRole = true;
        //                return;
        //            }
        //        }
        //    });

    }
    if (typeof $.when.all === 'undefined')
        loadWhenAllDefinition()

    $.when.all(deferreds).then(function (objects) {
        //  console.log("Resolved objects:", objects);
        returnData.data.result = false;
        for (var i = 0; i < objects.length; i++) {
            if (objects[i].data.result)
                returnData.data.result = true
        }
        returnData.success = true;
        deferred.resolve(returnData)
    });

    //return hasRole;
    return deferred.promise();
};


MCS.cvt_Common.ConfirmDilogOnSave = function (executionContext) {
    // var saveFromConfirmDialog = false;  
    debugger;
    var formContext = executionContext.getFormContext();

    if (formContext.data.entity.getEntityName() === "mcs_setting") {
        var name = formContext.getAttribute("mcs_name");
        if (name == null || name.getValue() != "Active Settings") {
            return;
        }
    }

    if (!MCS.cvt_Common.saveFromConfirmDialog) {
        executionContext.getEventArgs().preventDefault();

        var confirmStrings = {
            text: "Are you sure you want to save the settings record?" + "\n" + " Updating the settings will affect the application functionality/behavior." + "\n" + "You can still revert the changes later.", title: "Save Changes", cancelButtonLabel: "Cancel", confirmButtonLabel: "Confirm"
        };
        var confirmOptions = { height: 100, width: 600 };
        Xrm.Navigation.openConfirmDialog(confirmStrings, confirmOptions).then(function (success) {
            if (success.confirmed) {
                MCS.cvt_Common.saveFromConfirmDialog = true;
                formContext.data.save();
            }

            else {
                MCS.cvt_Common.saveFromConfirmDialog = false;
            }
        });
    }
    MCS.cvt_Common.saveFromConfirmDialog = false;

};



getCurrentUserRole = function (roles, currentUserRole) {
    var localDeferred = $.Deferred();
    var returnData = {
        success: true,
        data: {}
    };
    Xrm.WebApi.retrieveRecord("Role", currentUserRole, "?$select=name").then(
        function success(result) {
            if (result != null) {
                var roleName = result.name.trim().toLowerCase();
                if (roles.toLowerCase().indexOf(roleName) !== -1) {
                    hasRole = true;
                    //return;
                    // return hasRole;

                    returnData.success = true;
                    returnData.data.result = hasRole;

                }
                localDeferred.resolve(returnData);
            }
        },
        function (error) {
            returnData.success = false;
            localDeferred.resolve(returnData);

        }
    );
    return localDeferred.promise();
}


loadWhenAllDefinition = function () {
    if (typeof jQuery !== 'undefined' && typeof $ !== 'undefined') {
        if (jQuery.when.all === undefined) {
            jQuery.when.all = function (deferreds) {
                var deferred = new jQuery.Deferred();
                $.when.apply(jQuery, deferreds).then(
                    function () {
                        deferred.resolve(Array.prototype.slice.call(arguments));
                    },
                    function () {
                        deferred.fail(Array.prototype.slice.call(arguments));
                    });

                return deferred;
            }
        }
    }
}

