//If the SDK namespace object is not defined, create it.
if (typeof (MCS) == "undefined") {
    MCS = {};
}
// Create Namespace container for functions in this library;
MCS.Patient = {};

MCS.Patient.FormOnload = function (executionContext) {
    var formContext = executionContext.getFormContext();
    MCS.Patient.SetCommunicationFieldsVisibility(executionContext);
    MCS.Patient.EmailRequiredCheck(executionContext);
    formContext.getAttribute("cvt_tablettype").addOnChange(MCS.Patient.SetCommunicationFieldsVisibility);
    formContext.getAttribute("donotemail").addOnChange(MCS.Patient.DoNotEmailOnChange);

    var fieldsToLock = ["lastname", "firstname", "suffix", "middlename", "salutation", "mcs_othernames", "telephone2", "telephone1", "address1_line1", "address1_line2", "address1_line3", "address1_city", "address1_stateorprovince", "address1_postalcode", "address1_country", "gendercode", "birthdate", "familystatuscode", "mcs_branchofservice", "mcs_deceased", "mcs_deceaseddate"];
    formContext.data.entity.attributes.forEach(function (attribute, index) {
        var control = Xrm.Page.getControl(attribute.getName());
        if (fieldsToLock.indexOf(attribute.getName()) !== -1) {
            control.setDisabled(true);
        }
    });

    MCS.Patient.HideFeature("VALD Feature").done(function (results) {
        if (results.modifyFunctions) {
            if (results.scriptToBypass.indexOf('addDeviceSerialNoToForm') < 0) {
                addDeviceSerialNoToForm(executionContext);
            }
            if (results.scriptToBypass.indexOf('MCS.Patient.ValdFeatureSwitch') < 0) {
                MCS.Patient.ValdFeatureSwitch(executionContext);
            }
        }
        else {
            addDeviceSerialNoToForm(executionContext);
            MCS.Patient.ValdFeatureSwitch(executionContext);
        }
        if (results.hideFields && results.fieldsToHide !== null) {
            results.fieldsToHide.forEach((field) => {
                if (formContext.getControl(field.trim()) != null) formContext.getControl(field.trim()).setVisible(false);
            });
        }
        if (results.optionLabelToChange) {
            if (results.optionToChangeFields.length === results.newOptionLabels.length && results.newOptionLabels.length === results.oldOptionLabels.length) {

                for (var i = 0; i < results.optionToChangeFields.length; i++) {
                    var optionFieldToChange = results.optionToChangeFields[i].trim();
                    var optionToChange = formContext.getControl(optionFieldToChange);
                    if (optionToChange === undefined || optionToChange === null) continue;

                    var newOptionLabel = results.newOptionLabels[i].trim();
                    var oldOptionLabel = results.oldOptionLabels[i].trim();

                    var optionsToChange = formContext.getAttribute(optionFieldToChange).getOptions();
                    optionToChange.clearOptions();
                    for (var i = 0; i < optionsToChange.length; i++) {
                        var option = optionsToChange[i];
                        if (option.text === oldOptionLabel) {
                            optionToChange.removeOption(option.value);
                            option.text = newOptionLabel;
                            optionToChange.addOption(option, i);
                        }
                        else {
                            optionToChange.addOption(option, i);
                        }
                    }
                }
            }
        }
    });
};

MCS.Patient.EmailRequiredCheck = function (executionContext) {
    var formContext = executionContext.getFormContext();

    var technologyType = formContext.getAttribute("cvt_tablettype").getValue();
    var doNotAllowEmail = formContext.getAttribute("donotemail").getValue();

    console.log("Tech Type: " + technologyType);

    if (technologyType !== null) {
        if (technologyType === 917290002 && !doNotAllowEmail) {
            formContext.getAttribute("emailaddress1").setRequiredLevel("required"); //set as required
        }
        else if (technologyType === 917290002 && doNotAllowEmail) {
            formContext.getAttribute("emailaddress1").setRequiredLevel("none"); //set as not required
            formContext.getControl("emailaddress1").setDisabled(false); //unlock field
        }
    }
};

MCS.Patient.SetCommunicationFieldsVisibility = function (executionContext) {
    //Set Default Visibility
    var formContext = executionContext.getFormContext();
    var sipAddress = formContext.getControl("cvt_bltablet");
    var sipAddressAttribute = formContext.getAttribute("cvt_bltablet");
    var email = formContext.getControl("emailaddress1");
    var emailAttribute = formContext.getAttribute("emailaddress1");
    var donotemail = formContext.getControl("donotemail");
    var donotemailAttribute = formContext.getAttribute("donotemail");
    var staticvmrlinkControl = formContext.getControl("cvt_staticvmrlink");
    var staticvmrlinkControlAttribute = formContext.getAttribute("cvt_staticvmrlink");

    sipAddress.setVisible(true);
    sipAddressAttribute.setRequiredLevel("none");
    email.setVisible(true);
    emailAttribute.setRequiredLevel("none");
    donotemail.setVisible(true);
    donotemailAttribute.setRequiredLevel("required");
    staticvmrlinkControl.setVisible(true);
    staticvmrlinkControlAttribute.setRequiredLevel("none");

    var techType = formContext.getAttribute("cvt_tablettype").getValue();
    switch (techType) {
        case 100000000:
            //SIP Device (CVT Tablet and COTS Tablet)
            sipAddressAttribute.setRequiredLevel("required");
            sipAddress.setDisabled(false);
            donotemailAttribute.setValue(true);
            email.setDisabled(true);
            staticvmrlinkControl.setDisabled(true);
            break;
        case 917290002:
            //VA Issued iOS Device
            sipAddress.setDisabled(true);
            donotemail.setDisabled(false);
            MCS.Patient.DoNotEmailOnChange(executionContext); //TODO: Conditionals
            break;
        case 917290003:
            //Personal VA Video Connect Device
            sipAddress.setDisabled(true);
            donotemailAttribute.setValue(false);
            donotemail.setDisabled(true);
            emailAttribute.setRequiredLevel("required");
            email.setDisabled(false);
            staticvmrlinkControl.setDisabled(true);
            break;
    }
};

MCS.Patient.DoNotEmailOnChange = function (executionContext) {
    var formContext = executionContext.getFormContext();
    var donotemail = formContext.getAttribute("donotemail").getValue();
    var staticvmrlinkControl = formContext.getControl("cvt_staticvmrlink");
    var staticvmrlinkControlAttribute = formContext.getAttribute("cvt_staticvmrlink");

    var email = formContext.getControl("emailaddress1");
    var emailAttribute = formContext.getAttribute("emailaddress1");

    var techType = formContext.getAttribute("cvt_tablettype").getValue();
    switch (techType) {
        case 917290002:
            //VA Issued iOS Device
            staticvmrlinkControlAttribute.setRequiredLevel("none");
            staticvmrlinkControl.setDisabled(false);
            break;
        default:
            staticvmrlinkControlAttribute.setRequiredLevel(donotemail ? "required" : "none");
            staticvmrlinkControl.setDisabled(donotemail ? false : true);

            var email = formContext.getControl("emailaddress1");
            var emailAttribute = formContext.getAttribute("emailaddress1");
            emailAttribute.setRequiredLevel(donotemail ? "none" : "required");
            email.setDisabled(donotemail ? true : false);

            //run this so Email field is unlocked if certain conditions are met
            MCS.Patient.EmailRequiredCheck(executionContext);
    }
};

//added by Naveen Dubbaka for New Appointment button
MCS.Patient.NewSchedulingAppointment = function (formContext, scheduleTestCall) {
    debugger
    var params = {};

    var value = new Array();
    value[0] = new Object();
    value[0].id = formContext.data.entity.getId();
    value[0].name = formContext.getAttribute("fullname").getValue();
    value[0].entityType = "contact";

    params["customers"] = value;

    if (scheduleTestCall) {

        params["tmp_appointmentmodality"] = 178970008

        formContext.getAttribute("cvt_tablettype").setValue(917290002);
        formContext.data.save();
    }

    var entityFormOption = {};

    //entityFormOption.entityName = "serviceappointment";
    //entityFormOption.entityId = null;
    //entityFormOption.openInNewWindow = true;

    entityFormOption["entityName"] = "serviceappointment";
    entityFormOption["entityId"] = null;
    entityFormOption["openInNewWindow"] = true;

    Xrm.Navigation.openForm(entityFormOption, params);

};

//Added by Naveen  02/09/2021
MCS.Patient.NewSchedulingAppointmentOnHome = function (item) {

    var selectedItem = item[0];
    //alert("You have Select Record with Id=" + selectedItem.Id + "\nName="
    //    + selectedItem.Name + "\nEntity Type Code=" + selectedItem.TypeCode.toString() + "\nEntity=" + selectedItem.TypeName);
    var selectedItem = item[0];
    var p = {};

    var value = new Array();
    value[0] = new Object();
    value[0].id = selectedItem.Id;
    value[0].name = selectedItem.Name;
    value[0].entityType = selectedItem.TypeName;

    p["customers"] = value;

    var entityFormOption = {};

    //entityFormOption.entityName = "serviceappointment";
    //entityFormOption.entityId = null;
    //entityFormOption.openInNewWindow = true;
    entityFormOption["entityName"] = "serviceappointment";
    entityFormOption["entityId"] = null;
    entityFormOption["openInNewWindow"] = true;

    Xrm.Navigation.openForm(entityFormOption, p);

};

MCS.Patient.StaticVmrLinkValidate = function (executionContext) {
    //Set Default Visibility
    var formContext = executionContext.getFormContext();
    var staticvmrlink = formContext.getAttribute("cvt_staticvmrlink");

    if (staticvmrlink != null) {
        var staticvmrlinkValue = staticvmrlink.getValue();
        if (staticvmrlinkValue != null && staticvmrlinkValue.indexOf(' ') >= 0) {
            var message = "Enter a valid Static VMR Link without spaces.";
            formContext.getControl("cvt_staticvmrlink").setNotification(message);
        } else {
            formContext.getControl("cvt_staticvmrlink").clearNotification();
        }
    }
};

MCS.Patient.CheckTestVVCCallSwitch = function (formContext) {
    debugger
    var flag = false;
    var req = new XMLHttpRequest();
    req.open("GET", formContext.context.getClientUrl() + "/api/data/v9.1/mcs_settings?$select=cvt_componentplugin&$filter=mcs_name eq 'VALD%20Feature'", false);
    req.setRequestHeader("OData-MaxVersion", "4.0");
    req.setRequestHeader("OData-Version", "4.0");
    req.setRequestHeader("Accept", "application/json");
    req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
    req.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
    req.onreadystatechange = function () {
        if (this.readyState === 4) {
            req.onreadystatechange = null;
            if (this.status === 200) {
                var results = JSON.parse(this.response);
                for (var i = 0; i < results.value.length; i++) {
                    flag = results.value[i]["cvt_componentplugin"];
                }
            } else {
                Xrm.Utility.alertDialog(this.statusText);
            }
        }
    };
    req.send();
    return flag;
};

function addDeviceSerialNoToForm(executionContext) {
    var formContext = executionContext.getFormContext();
    //var contactID = formContext.data.entity.getId().replace(/[{}]/g, "");
    var req = new XMLHttpRequest();
    req.open("POST", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/contacts(" + formContext.data.entity.getId().replace("{", "").replace("}", "") + ")/Microsoft.Dynamics.CRM.tmp_GetPatientDeviceInfo", true);
    req.setRequestHeader("OData-MaxVersion", "4.0");
    req.setRequestHeader("OData-Version", "4.0");
    req.setRequestHeader("Accept", "application/json");
    req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
    req.onreadystatechange = function () {
        if (this.readyState === 4) {
            req.onreadystatechange = null;
            if (this.status === 200) {

                var recentDeviceID = null;
                var tmp_GetPatientDeviceInfoRequestResults = JSON.parse(this.response);

                console.log("VA Loaned Devices");
                console.log(JSON.parse(tmp_GetPatientDeviceInfoRequestResults.outputJSON));

                var devices = JSON.parse(tmp_GetPatientDeviceInfoRequestResults.outputJSON).Devices;
                devices.forEach((item) => {
                    if (recentDeviceID == null || orderedDate < item.Attributes.OrderedDateTime) {
                        recentDeviceID = item.Attributes.SerialNumber;
                        orderedDate = item.Attributes.OrderedDateTime;
                    }
                });

                var deviceSerialAttribute = parent.Xrm.Page.getAttribute("tmp_deviceserialnumber");
                deviceSerialAttribute.setValue(recentDeviceID);
                formContext.data.save();
            } else {
                Xrm.Utility.alertDialog(this.statusText);
            }
        }
    }
    req.send();
    //    formContext.data.save();
    //req.send(JSON.stringify(parameters));
};

MCS.Patient.RemoveOptionFromTechnololgyType = function (executionContext) {
    var formContext = executionContext.getFormContext();
    var serialNum = formContext.getAttribute("tmp_deviceserialnumber");
    var typeOptionSet = formContext.getControl("cvt_tablettype");
    var typeOptionSetOption = formContext.getAttribute("cvt_tablettype").getOptions();

    // *** Clear current items
    for (var i = 0; i < typeOptionSetOption.length; i++) {
        typeOptionSet.removeOption(typeOptionSetOption[i].value);
    }

    if (serialNum !== null && serialNum.getValue() === null) {
        typeOptionSet.removeOption(917290002);
        typeOptionSet.addOption({
            value: 917290003,
            text: "Personal Device"
        });
        typeOptionSet.addOption({
            value: 100000000,
            text: "SIP Device"
        });

    } else {
        typeOptionSet.addOption({
            value: 917290003,
            text: "Personal Device"
        });
        typeOptionSet.addOption({
            value: 917290002,
            text: "VA Loaned Device"
        });
        typeOptionSet.addOption({
            value: 100000000,
            text: "SIP Device"
        });
    }

};

MCS.Patient.ValdFeatureSwitch = function (executionContext) {
    var formContext = executionContext.getFormContext();

    //pull the value of the Hide Fields from Settings entity.
    var fetchXml = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>";
    fetchXml += "<entity name='mcs_setting'>";
    fetchXml += "<attribute name='mcs_settingid' />";
    fetchXml += "<attribute name='tmp_hidefields' />";
    fetchXml += "<filter type='and'>";
    fetchXml += "<condition attribute='mcs_name' operator='eq' value='VALD Feature' />";
    fetchXml += "<condition attribute='statecode' operator='eq' value='0' />";
    fetchXml += "</filter>";
    fetchXml += "</entity>";
    fetchXml += "</fetch>";
    var fetch = encodeURI(fetchXml);

    var entityname = "mcs_settings";
    var serverURL = "https://" + location.hostname;
    var query = entityname + "?fetchXml=" + fetch;
    var req = new XMLHttpRequest();
    req.open("GET", serverURL + "/api/data/v9.1/" + query, false);
    req.setRequestHeader("Accept", "application/json");
    req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
    req.setRequestHeader("OData-MaxVersion", "4.0");
    req.setRequestHeader("OData-Version", "4.0");
    req.setRequestHeader("Prefer", "odata.include-annotations=\"OData.Community.Display.V1.FormattedValue\"");

    req.onreadystatechange = function () {
        if (this.readyState == 4
            /* complete */
        ) {
            req.onreadystatechange = null;
            if (this.status == 200) {
                var data = JSON.parse(this.response);
                if (data != null) {
                    var record = data.value[0];
                    var hideFields = record['tmp_hidefields'];

                    if (hideFields) {
                        formContext.getControl("cvt_tablettype").setVisible(false);
                        formContext.getControl("tmp_deviceserialnumber").setVisible(false);
                        formContext.getControl("cvt_staticvmrlink").setVisible(false);
                    }
                }
            }
        }
        else {
            alert("An error has occured while accessing the VALD Settings. Please contact your System Administrator.");
        }
    };
    req.send();
};

MCS.Patient.FeatureSwitchRibbonButton =
    async function () {

        return await MCS.Patient.FeatureIsEnabled();

    };

MCS.Patient.FeatureIsEnabled = function () {
    return new Promise(resolve => {
        Xrm.WebApi.online.retrieveMultipleRecords("mcs_setting", "?$select=tmp_hideribbonbuttons&$filter=mcs_name eq 'VALD Feature' and statecode eq 0").then(
            function success(results) {
                console.log(results)
                if (results.entities.length > 0) {
                    var hideButton = results.entities[0].tmp_hideribbonbuttons;

                    console.log(hideButton);

                    if (hideButton) {
                        //if we want to hide the button then
                        //return false so that Ribbon workbench will not display the button
                        resolve(false);
                    }
                    else {
                        resolve(true);
                    }
                }
                else {
                    resolve(true);
                }
            },
            function (error) {
                Xrm.Utility.alertDialog(error.message);
            });
    });
};

MCS.Patient.HideFeature = function (featureName) {
    var deferred = new $.Deferred();
    delete window.featureEnabled;
    Xrm.WebApi.online.retrieveMultipleRecords("mcs_setting", "?$select=tmp_fieldstohide,tmp_hidefields,tmp_hideoption,tmp_modifiedfunctions,tmp_modifyjavascript,tmp_optionlabeltohide,tmp_optionsetfield,tmp_optionvaluetohide,tmp_changeoptionlabel,tmp_changeoptionfield,tmp_oldoptionlabel,tmp_newoptionlabel&$filter=mcs_name eq '" + featureName + "' and statecode eq 0").then(
        function success(results) {
            console.log(results)
            if (results.entities.length > 0) {
                var featuresToHide = {
                    fieldsToHide: results.entities[0].tmp_fieldstohide !== null ? results.entities[0].tmp_fieldstohide.split(',') : null,
                    optionsToHide: results.entities[0].tmp_optionsetfield !== null ? results.entities[0].tmp_optionsetfield.split(',') : null,
                    optionLabelsToHide: results.entities[0].tmp_optionlabeltohide !== null ? results.entities[0].tmp_optionlabeltohide.split(',') : null,
                    optionValuesToHide: results.entities[0].tmp_optionvaluetohide !== null ? results.entities[0].tmp_optionvaluetohide.split(',') : null,
                    optionLabelToChange: results.entities[0].tmp_changeoptionlabel,
                    optionToChangeFields: results.entities[0].tmp_changeoptionfield !== null ? results.entities[0].tmp_changeoptionfield.split(',') : null,
                    oldOptionLabels: results.entities[0].tmp_oldoptionlabel !== null ? results.entities[0].tmp_oldoptionlabel.split(',') : null,
                    newOptionLabels: results.entities[0].tmp_newoptionlabel !== null ? results.entities[0].tmp_newoptionlabel.split(',') : null,
                    scriptToBypass: results.entities[0].tmp_modifiedfunctions,
                    hideFields: results.entities[0].tmp_hidefields,
                    hideOption: results.entities[0].tmp_hideoption,
                    modifyFunctions: results.entities[0].tmp_modifyjavascript
                };

                window.featuresToHide = [];
                window.featuresToHide.push(featuresToHide);

                deferred.resolve(featuresToHide);
            }
        },
        function (error) {
            Xrm.Utility.alertDialog(error.message);
        });

    return deferred.promise();
};