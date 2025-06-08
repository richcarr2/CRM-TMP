//Javascript Libary for the mvi_searchui.html

function openSelectedPerson(obj) {

    showExecutingSearch();

    //var filter = "$select=*&$filter=";

    var filter = '';
    filter += buildQueryFilter("crme_alias", obj.getAttribute("crme_alias"), false);
    filter += buildQueryFilter("crme_lastname", obj.getAttribute("crme_lastname"), true);
    filter += buildQueryFilter("crme_firstname", obj.getAttribute("crme_firstname"), true);
    filter += buildQueryFilter("crme_middlename", obj.getAttribute("crme_middlename"), true);
    filter += buildQueryFilter("crme_namesuffix", obj.getAttribute("crme_namesuffix"), true);
    filter += buildQueryFilter("crme_deceaseddate", obj.getAttribute("crme_deceaseddate"), true);
    filter += buildQueryFilter("crme_dobstring", obj.getAttribute("crme_dobstring"), true);
    filter += buildQueryFilter("crme_edipi", obj.getAttribute("crme_edipi"), true);
    filter += buildQueryFilter("crme_gender", obj.getAttribute("crme_gender"), true);
    filter += buildQueryFilter("crme_identitytheft", obj.getAttribute("crme_identitytheft"), true);
    filter += buildQueryFilter("crme_primaryphone", obj.getAttribute("crme_primaryphone"), true);
    filter += buildQueryFilter("crme_recordsource", obj.getAttribute("crme_recordsource"), true);
    filter += buildQueryFilter("crme_ssn", obj.getAttribute("crme_ssn"), true);
    filter += buildQueryFilter("crme_fulladdress", obj.getAttribute("crme_fulladdress"), true);
    filter += buildQueryFilter("crme_fullname", obj.getAttribute("crme_fullname"), true);
    filter += buildQueryFilter("crme_patientmviidentifier", obj.getAttribute("crme_patientmviidentifier"), true);
    filter += buildQueryFilter("crme_searchtype", 'SelectedPersonSearch', true);
    filter += buildQueryFilter("crme_classcode", obj.getAttribute("crme_classcode"), true);
    filter += buildQueryFilter("crme_returnmessage", obj.getAttribute("crme_returnmessage"), true);
    //filter += buildQueryFilter("crme_url", SERVER_URL, true);
    filter += buildQueryFilter("crme_url", Xrm.Utility.getGlobalContext().getClientUrl(), true);
    filter += buildOptionSetQueryFilter("cvt_integrationtype", 917290000, true);

    var contactRecord = {
        'firstname': obj.getAttribute("crme_firstname"),
        'lastname': obj.getAttribute("crme_lastname"),
        'middlename': obj.getAttribute("crme_middlename"),
        'suffix': obj.getAttribute("crme_namesuffix")
    };

    try {
        //SDK.REST.retrieveMultipleRecords("crme_person", filter, selectedPersonCallBack, searchErrorCallback, enableButtons);
        var contactid;
        if (parent.Xrm.Page.getControl("tmp_technologytype") != null) {
            var typeOptionSet = parent.Xrm.Page.getControl("tmp_technologytype");
            var typeOptionSetOption = parent.Xrm.Page.getAttribute("tmp_technologytype").getOptions();

            for (var i = 0; i < typeOptionSetOption.length; i++) {
                typeOptionSet.removeOption(typeOptionSetOption[i].value);
            }

        }

        Xrm.WebApi.retrieveMultipleRecords("crme_person", "?$select=*" + "&$filter=" + filter).then(
            function success(results) {

                if ((results !== null) && (results.entities !== null) && (results.entities[0].crme_lastname === null)) {
                    console.log("UPDATING CONTACT");
                    contactid = results.entities[0].crme_contactid;
                    Xrm.WebApi.updateRecord("contact", contactid, contactRecord).then(
                        function success(results) {
                            console.log("Contact Updated.  Retrieving updated record");
                            Xrm.WebApi.retrieveRecord("contact", contactid, "?$select=*").then(
                                function success(result) {

                                    var contactRetrieved = result;

                                    var req = new XMLHttpRequest();
                                    req.open("POST", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/contacts(" + contactid + ")/Microsoft.Dynamics.CRM.tmp_GetPatientDeviceInfo", true);
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
                                                if (tmp_GetPatientDeviceInfoRequestResults.outputJSON != null) {

                                                    var devices = JSON.parse(tmp_GetPatientDeviceInfoRequestResults.outputJSON).Devices;
                                                    if (devices !== null && devices !== undefined) {
                                                        devices.forEach((item) => {
                                                            if (item.Attributes.DeviceType.indexOf("iPad") >= 0 && (recentDeviceID == null || orderedDate < item.Attributes.OrderedDateTime)) {
                                                                recentDeviceID = item.Attributes.SerialNumber;
                                                                orderedDate = item.Attributes.OrderedDateTime;
                                                            }

                                                        })
                                                    }
                                                    if (recentDeviceID !== null) {

                                                        var entity = {};
                                                        entity.tmp_deviceserialnumber = recentDeviceID;

                                                        Xrm.WebApi.online.updateRecord("contact", contactid, entity).then(
                                                            function success(result) {
                                                                selectedPersonCallBack(contactRetrieved);
                                                                enableButtons();
                                                                typeOptionSet.addOption({
                                                                    value: 917290002,
                                                                    text: "VA Loaned Device"
                                                                });
                                                                typeOptionSet.addOption({
                                                                    value: 917290003,
                                                                    text: "Personal Device"
                                                                });
                                                                typeOptionSet.addOption({
                                                                    value: 100000000,
                                                                    text: "SIP Device"
                                                                });
                                                            },
                                                            function (error) {
                                                                Xrm.Utility.alertDialog(error.message);
                                                            });
                                                    }
                                                    else {
                                                        selectedPersonCallBack(contactRetrieved);
                                                        enableButtons();
                                                        typeOptionSet.addOption({
                                                            value: 917290003,
                                                            text: "Personal Device"
                                                        });
                                                        typeOptionSet.addOption({
                                                            value: 100000000,
                                                            text: "SIP Device"
                                                        });
                                                    }
                                                }
                                            } else {
                                                Xrm.Utility.alertDialog(this.statusText);
                                            }

                                        }
                                    };
                                    req.send();

                                },
                                function (error) {
                                    console.log("Failed contact record call after update.")
                                });

                        },
                        function (error) {
                            console.log("WebApi.updateRecord failed.")
                        });
                }
            },
            function (error) {
                searchErrorCallback(error);
                enableButtons();
            });
    }
    catch (e) {
        searchErrorCallback(e);
    }

    return false;
}

function searchSucessCallback(data, textStatus, XmlHttpRequest) {
    hideAll();

    var table = $("#personSearchResultsTable");
    $("#personSearchResultsTable").find("thead, tr, th").remove();
    $("#personSearchResultsTable").find("tr:gt(0)").remove();

    if (data !== null && typeof data.entities !== 'undefined' && data.entities.length > 0) {

        //if only 1 item is returned, check to see if an error response was returned from MVI
        //if so, exit the callback
        if (data.entities.length == 1 && !evaluateMviMessage(data.entities[0])) return;

        $("#searchResultsGrid").show();

        var headers = ['Name', 'Alias', 'Gender', 'Date of Birth', 'SSN', 'EDIPI', 'Address', 'Phone', 'Deceased Date', 'Identity Theft', 'Source'];
        var thead = document.createElement('tr');

        for (var i = 0; i < headers.length; i++) {
            var th = document.createElement('th');
            th.className = "grid_title";
            th.appendChild(document.createTextNode(headers[i]));
            th.id = "th_" + headers[i].replace(' ', '');
            th.title = headers[i];
            th.scope = "col";
            thead.appendChild(th);
        }

        table.append(thead);

        for (var i = 0; i < data.entities.length; i++) {
            var row = document.createElement('tr');

            var crmValues = getCrmValues(data.entities[i], row);
            for (var j = 0; j < headers.length; j++) {
                if (j === 0) {
                    var td = document.createElement('th');
                    td.scope = "row";
                }
                else {
                    var td = document.createElement('td');
                }

                td.id = "th_" + i + "_" + j + "_" + headers[j].replace(' ', '');
                td.title = crmValues[j];

                td.appendChild(document.createTextNode(crmValues[j]));
                row.appendChild(td);
            }
            row.className = "grid_row";
            row.ondblclick = function () {
                openSelectedPerson(this);
            };
            table.append(row);
        }

        //Force focus to message
        table.focus();
    }
    else {
        enableButtons();
        $("#warningDetails").text("");
        $("#warningDetails").append("No matches where returned from MVI matching the data provided. Please refine your search and try again.");
        $("#msgBoxNoDataReturned").show();

        //Force focus to message
        $("#msgBoxNoDataReturned").focus();
    }
}

function addVeteranToForm(fieldName, data) {
    // debugger;
    if (data !== null) {
        //if (data.entities[0] !== null) {
        //var contactObj = { id: data.entities[0].crme_contactid, name: data.entities[0].crme_fullname };
        Xrm.WebApi.retrieveRecord('contact', data.contactid, "?$select=*").then(
            function success(result) {
                var participants = new Array();
                participants[0] = new Object();
                participants[0].id = result.contactid;
                participants[0].name = result.fullname;
                participants[0].entityType = "contact";

                var veteranAttribute = parent.Xrm.Page.getAttribute(fieldName);
                if (veteranAttribute !== null) {
                    var veterans = veteranAttribute.getValue();
                    if (veterans !== null) {
                        for (var i = 1; i <= veterans.length; i++) {
                            participants[i] = new Object();
                            //Populate optional attendees field
                            participants[i].id = veterans[i - 1].id;
                            participants[i].name = veterans[i - 1].name;
                            participants[i].entityType = "contact";
                        }
                    }
                    debugger;
                    var parentWindow;

                    for (var i = 0; i < parent.window.length; i++) {
                        if (typeof parent.window[i].MCS !== 'undefined') {
                            parentWindow = parent.window[i];
                            break;
                        }
                    }

                    if (parentWindow !== 'undefined') parentWindow.MCS.Patients = participants;
                    else if (typeof window.top.MCS !== 'undefined') window.top.MCS.Patients = participants;

                    veteranAttribute.setValue(participants);
                    veteranAttribute.fireOnChange();
                    //parent.MCS.Patients = participants;
                }
            },
            function (error) {
                alert(error.message);
            });

        //}

    }
    /****************************************************************************************/
    else if (data.fullname != null) {
        //var contactObj = { id: data.entities[0].crme_contactid, name: data.entities[0].crme_fullname };
        //function success(result){
        var participants = new Array();
        participants[0] = new Object();
        participants[0].id = data.contactid;
        participants[0].name = data.fullname;
        participants[0].entityType = "contact";

        var veteranAttribute = parent.Xrm.Page.getAttribute(fieldName);
        if (veteranAttribute !== null) {
            var veterans = veteranAttribute.getValue();
            if (veterans !== null) {
                for (var i = 1; i <= veterans.length; i++) {
                    participants[i] = new Object();
                    //Populate optional attendees field
                    participants[i].id = veterans[i - 1].id;
                    participants[i].name = veterans[i - 1].name;
                    participants[i].entityType = "contact";
                }
            }
            veteranAttribute.setValue(participants);
            //parent.parent.MCS.Patients = participants;
            if (typeof parent.MCS !== 'undefined') parent.MCS.Patients = participants;
            else if (typeof window.top.MCS !== 'undefined') window.top.MCS.Patients = participants;
        }
    }
    /****************************************************************************************/

}

function selectedPersonCallBack(data) {
    hideAll();
    enableButtons();

    if (data != null) {
        if (data.entities !== null) {
            //if (data.entities[0] !== null) {
            //if (evaluateMviMessage(data.entities[0])) {
            if (parent.Xrm.Page.data !== null && parent.Xrm.Page.data.entity !== null && parent.Xrm.Page.data.entity.getEntityName() === "serviceappointment") {
                addVeteranToForm("customers", data);
            }
            else if (parent.Xrm.Page.data !== null && parent.Xrm.Page.data.entity !== null && parent.Xrm.Page.data.entity.getEntityName() === "appointment") {
                addVeteranToForm("optionalattendees", data);
            }
            else {
                //var url = data.entities[0].crme_url;
                var entityFormOptions = {
                    "entityName": "contact",
                    "entityId": data.contactid.replace("'", ""),
                    openInNewWindow: true
                };
                parent.Xrm.Navigation.openForm(entityFormOptions).then(
                    function (success) { },
                    function (error) { });
            }
            //}
            //}
        }
        else if (data.fullname !== null) {
            if (evaluateMviMessage(data)) {
                if (parent.Xrm.Page.data !== null && parent.Xrm.Page.data.entity !== null && parent.Xrm.Page.data.entity.getEntityName() === "serviceappointment") {
                    addVeteranToForm("customers", data);
                }
                else if (parent.Xrm.Page.data !== null && parent.Xrm.Page.data.entity !== null && parent.Xrm.Page.data.entity.getEntityName() === "appointment") {
                    addVeteranToForm("optionalattendees", data);
                }
                else {
                    //var url = data.entities[0].crme_url;
                    //window.open(url);
                    var entityFormOptions = {
                        "entityName": "contact",
                        "entityId": data.contactid.replace("'", ""),
                        openInNewWindow: true
                    };
                    parent.Xrm.Navigation.openForm(entityFormOptions).then(
                        function (success) { },
                        function (error) { });
                }
            }
        }

    }
    else {
        $("#warningDetails").text("");
        $("#warningDetails").append("No data was returned for the selected person. Please try again.");
        $("#msgBoxNoDataReturned").show();
    }
}

function getCrmValues(data, row) {
    //get the display values
    var fName = data.crme_firstname == null ? "" : data.crme_firstname;
    row.setAttribute("crme_firstname", fName);

    var lName = data.crme_lastname == null ? "" : data.crme_lastname;
    row.setAttribute("crme_lastname", lName);

    var mName = data.crme_middlename == null ? "" : data.crme_middlename;
    row.setAttribute("crme_middlename", mName);

    var suffix = data.crme_namesuffix == null ? "" : data.crme_namesuffix;
    row.setAttribute("crme_namesuffix", suffix);

    var alias = data.crme_alias == null ? "" : data.crme_alias;
    row.setAttribute("crme_Alias", alias);

    var deceasedDate = data.crme_deceaseddate == null ? "" : data.crme_deceaseddate;
    row.setAttribute("crme_DeceasedDate", deceasedDate);

    var dateOfBirth = data.crme_dobstring == null ? "" : data.crme_dobstring;
    row.setAttribute("crme_DOBString", dateOfBirth);

    var edipi = data.crme_edipi == null ? "" : data.crme_edipi;
    row.setAttribute("crme_EDIPI", edipi);

    var gender = data.crme_gender == null ? "" : data.crme_gender;
    row.setAttribute("crme_Gender", gender);

    var identityTheft = data.crme_identitytheft == null ? "" : data.crme_identitytheft;
    row.setAttribute("crme_IdentityTheft", identityTheft);

    var phoneNumber = data.crme_primaryphone == null ? "" : data.crme_primaryphone;
    row.setAttribute("crme_PrimaryPhone", phoneNumber);

    var recordSource = data.crme_recordsource == null ? "" : data.crme_recordsource;
    row.setAttribute("crme_RecordSource", recordSource);

    var ssn = data.crme_ssn == null ? "" : data.crme_ssn;
    row.setAttribute("crme_SSN", ssn);

    var address = ((data.crme_fulladdress == null) || (data.crme_fulladdress == "|||||")) ? "" : data.crme_fulladdress;
    row.setAttribute("crme_FullAddress", serializeAddress(data));

    var fullName = data.crme_fullname == null ? "" : data.crme_fullname;
    row.setAttribute("crme_FullName", serializeName(data));

    row.setAttribute("crme_PatientMviIdentifier", data.crme_patientmviidentifier);

    if (data.crme_classcode !== null) row.setAttribute("crme_ClassCode", data.crme_classcode);
    else row.setAttribute("crme_ClassCode", '');

    if (data.crme_returnmessage !== null) row.setAttribute("crme_ReturnMessage", data.crme_returnmessage);
    else row.setAttribute("crme_ReturnMessage", '');

    var crmValues = [fullName, alias, gender, dateOfBirth, ssn, edipi, address, phoneNumber, deceasedDate, identityTheft, recordSource];
    return crmValues;
}

//Determines if the current appointment has already occurred, in which case, do not allow Patient Search
function isAppointmentInPast() {
    if (typeof parent.Xrm.Page !== 'undefined' && parent.Xrm.Page !== null && parent.Xrm.Page.data !== null && parent.Xrm.Page.data.entity !== null) {
        if (parent.Xrm.Page.data.entity.getEntityName() === "serviceappointment" || parent.Xrm.Page.data.entity.getEntityName() === "appointment") {
            if (parent.Xrm.Page.ui.getFormType() === 1) return false;
            var startTime = parent.Xrm.Page.getAttribute("scheduledstart").getValue();
            var now = new Date();
            if (now > startTime) return true;
            else return false;
        }
        else return false;
    }
    else return false;
};

//Search Methods
function searchForPatient() {
    var appointmentIsInPast = isAppointmentInPast();
    if (appointmentIsInPast) {
        $("#msgBoxSearchResultError").show();
        $("#errorDetails").text("");
        $("#errorDetails").append("Appointment occurs in the past, patient search disabled");
        $("#errorDetails").focus();
        return;
    }

    var filter = "";

    if (validateSearch() === true) {
        showExecutingSearch();
        var DOB = $("#DateofBirthDateInput").val();
        var cleanDOB = new Date(DOB);
        var stringMM = cleanDOB.getMonth() + 1;
        var stringDD = cleanDOB.getDate();
        var stringDOB = "" + cleanDOB.getFullYear() + formatDatePart(stringMM.toString()) + formatDatePart(stringDD.toString());

        //Required parameters
        //filter = "$select=*&$filter=";

        filter += buildOptionSetQueryFilter("cvt_integrationtype", 917290000, false);
        filter += buildQueryFilter("crme_firstname", $("#FirstNameTextBox").val(), true);
        filter += buildQueryFilter("crme_lastname", $("#LastNameTextBox").val(), true);
        filter += buildQueryFilter("crme_ssn", $("#SocialSecurityTextBox").val(), true);
        filter += buildQueryFilter("crme_searchtype", 'SearchByFilter', true);
        filter += " and crme_isattended eq true";
        filter += stringDOB != "" ? " and crme_dobstring eq '" + stringDOB + "'" : "";
    }
    else showValidationFailed();

    if (filter !== "") {

        //try {
        //    SDK.REST.retrieveMultipleRecords("crme_person", filter, searchSucessCallback, searchErrorCallback, enableButtons);
        //}
        //catch (e) {
        //    searchErrorCallback(e);
        //}

        try {

            Xrm.WebApi.retrieveMultipleRecords("crme_person", "?$select=*" + "&$filter=" + filter).then(
                function success(results) {
                    searchSucessCallback(results);
                    enableButtons();
                },
                function (error) {
                    searchErrorCallback(error);
                    enableButtons();
                });
        }
        catch (e) {
            searchErrorCallback(e);
        }

    }
};

//Add to query filter - single quote/apostrophe requires custom encoding here in JS and then decoding in the Plugin
function buildQueryFilter(field, value, and) {
    if (and) return " and " + field + " eq '" + encodeURIComponent(value).replace("'", "%APOS") + "'";
    else return field + " eq '" + encodeURIComponent(value).replace("'", "%APOS") + "'";
};

function buildOptionSetQueryFilter(field, value, and) {
    if (and) return " and " + field + " eq " + value;
    else return field + " eq " + value;
};

//Format a Pipe Delimited Full Address from the individual address fields
function serializeAddress(data) {
    if (data === null) return "";

    var addressLine = (data.crme_Address1 != null && data.crme_Address1 != "") ? data.crme_Address1 : "";
    var cityName = (data.crme_City != null && data.crme_City != "") ? data.crme_City : "";
    var stateName = (data.crme_StateProvinceId != null && data.crme_StateProvinceId.Name != null) ? data.crme_StateProvinceId.Name : "";
    var zipName = (data.crme_ZIPPostalCodeId != null && data.crme_ZIPPostalCodeId.Name != null) ? data.crme_ZIPPostalCodeId.Name : "";
    var countryName = (data.crme_countryId != null && data.crme_countryId.Name != null) ? data.crme_countryId.Name : "";

    return addressLine + "|" + cityName + "|" + stateName + "|" + zipName + "|" + countryName;
}

//Create a Pipe Delimited Full Name from the individual name fields
function serializeName(data) {
    if (data === null) return "";

    var lastName = (data.crme_LastName != null && data.crme_LastName != "") ? data.crme_LastName : "";
    var firstName = (data.crme_FirstName != null && data.crme_FirstName != "") ? data.crme_FirstName : "";
    var middleName = (data.crme_MiddleName != null && data.crme_MiddleName != "") ? data.crme_MiddleName : "";
    var suffix = (data.crme_Suffix != null && data.crme_Suffix != "") ? data.crme_Suffix : "";

    return lastName + "|" + firstName + "|" + middleName + "|" + suffix + "|"; //Removed prefix
}

//Create a Full Name from the individual name fields
function formatName(data) {
    if (data.crme_FullName !== null) {
        return data.crme_FullName;
    }

    var name = "";
    name = data.crme_LastName != null ? data.crme_LastName : "";
    name += data.crme_Suffix != null ? " " + data.crme_Suffix : "";
    name += ", ";
    name += data.crme_FirstName != null ? data.crme_FirstName : "";
    name += data.crme_MiddleName != null ? " " + data.crme_MiddleName : "";
    name = name.trim();
    name = name == "," ? "" : name;

    return name;
}

//Format a Full Address from the individual address fields
function formatAddress(data) {
    if (data.crme_FullAddress !== null) {
        return data.crme_FullAddress;
    }

    var street = data.crme_Address1 != null ? data.crme_Address1 : "";
    var city = data.crme_City != null ? data.crme_City : "";
    var state = data.crme_StateProvinceId != null ? data.crme_StateProvinceId : "";
    var zip = data.crme_ZIPPostalCodeId != null ? data.crme_ZIPPostalCodeId : "";

    return street + " " + city + " " + state + " " + zip;
}

//Format a one digit month or day into a two digit part
function formatDatePart(datepart) {
    return datepart.length == 1 ? "0" + datepart : datepart;
}

//Disable two buttons
function disableButtons() {
    $("#SearchButton").attr('disabled', true);
    $("#ClearButton").attr('disabled', true);
}

//Enable two buttons
function enableButtons() {
    $("#SearchButton").attr('disabled', false);
    $("#ClearButton").attr('disabled', false);
}

//Evaluate MVI Message
function evaluateMviMessage(data) {
    if (data === null) {
        $("#msgBoxSearchResultError").show();
        $("#errorDetails").text("");
        message = "An unexpected error occurred. Please try your search again.";
        $("#errorDetails").append(message);
        return false;
    }

    var message = data.crme_mvimessage;

    if (message === null) return true;

    //check the returned message for "no match" indicators
    var noMatchMessage = "No results were returned from MVI matching the search criteria provided. Please refine your search and try again.";
    var toManyMatches = "The query returned more than the allowable number of matches. Please refine your search and try again.";

    if (message.indexOf("[NF]") >= 0) {
        $("#warningDetails").text("");
        $("#warningDetails").append(noMatchMessage);
        $("#msgBoxNoDataReturned").show();
        return false;
    }

    if (message.indexOf("[QE]") >= 0) {
        $("#warningDetails").text("");
        $("#warningDetails").append(toManyMatches);
        $("#msgBoxNoDataReturned").show();
        return false;
    }

    //check for an error - display error dialog
    if (message.indexOf("[ER]") >= 0) {
        $("#msgBoxSearchResultError").show();
        $("#errorDetails").text("");
        $("#errorDetails").append(message.replace("[ER]", ""));
        return false;
    }

    return true;
}

//Show Executing Search
function showExecutingSearch() {
    hideAll();
    disableButtons();
    $("#msgBoxWorking").show();
    $("#msgBoxWorking").focus();
}

//Show Validation Failed
function showValidationFailed() {
    hideAll();
    enableButtons();
    $("#msgBoxFailedValidation").show();
    $("#msgBoxFailedValidation").focus();
}

//Show Error Details
function searchErrorCallback(error) {
    hideAll();
    enableButtons();
    $("#msgBoxSearchResultError").show();
    $("#errorDetails").text("");
    $("#errorDetails").append(error.message);
    $("#errorDetails").focus();
}

//Hide all Message Divs
function hideAll() {
    $("#msgBoxWorking").hide();
    $("#msgBoxFailedValidation").hide();
    $("#msgBoxSearchResultError").hide();
    $("#msgBoxNoDataReturned").hide();
    $("#searchResultsGrid").hide();
}

//Clear Fields, Hide Divs
function resetForm() {
    //Clear fields
    $("#FirstNameTextBox").val("");
    $("#LastNameTextBox").val("");
    $("#DateofBirthDateInput").val("");
    $("#PhoneNoTextBox").val("");
    $("#SocialSecurityTextBox").val("");

    //clear out possible notifications
    $("#FirstNameNotification").html("");
    $("#LastNameNotification").html("");
    $("#SSNotification").html("");
    $("#DOBNotification").html("");

    //Hide grids
    hideAll()
    enableButtons();

    //Set focus to the Search Method
    document.getElementById("FirstNameTextBox").focus();
}

//Clear out default value to ""
function clearField(obj) {
    if (obj.defaultValue === obj.value) obj.value = "";
}

//Required Field Validation
function CheckRequiredField(fieldName, text, displayArea) {
    //Check the field for text, if empty, pop open a new image with alt text.
    var fieldValue = $(fieldName).val();
    var msg = "";
    $("#" + displayArea)[0].setAttribute("aria-haspopup", false);

    if (fieldValue === "") msg = "Please enter the " + text + ", it is required.";
    else if (text === "Social Security Number") {
        if (fieldValue !== "" || fieldValue !== " 9 digits, no dashes") {
            var checkNumber = validateNumeric(fieldValue.trim());
            if (checkNumber !== "") msg = checkNumber;
        }
    }

    if (msg !== "") $("#" + displayArea)[0].setAttribute("aria-haspopup", true);

    msg = '<font color="red">' + msg + '</font>';
    $("#" + displayArea).html(msg);
}

//Format Date of Birth
function FormatDOB(location) {
    var rawDOB = $(DateofBirthDateInput).val();
    var cleanDOB = rawDOB.replace(/\D/g, '');
    var cleanDOBwithSlashes = rawDOB.replace(/[^0-9/]/g, '');
    var msg = "";
    var formattedDOB = cleanDOBwithSlashes;

    if (rawDOB.length !== 0) {
        if (cleanDOB.length == 8 && cleanDOBwithSlashes.length == 8 && rawDOB.length == 8) {
            msg = '<font color="blue">' + "Attempted to format DOB from MMDDYYYY to MM/DD/YYYY." + '</font>';
            formattedDOB = rawDOB.substring(0, 2) + "/" + rawDOB.substring(2, 4) + "/" + rawDOB.substring(4, 8);
        }
        else if ((rawDOB.length !== 10 && cleanDOBwithSlashes.length !== 10) || (cleanDOB.length !== 8)) {
            msg = "Date of Birth must be formatted MM/DD/YYYY.";
            formattedDOB = cleanDOBwithSlashes;
        }

        if (formattedDOB.length === 10) {
            var validateDate = new Date(formattedDOB);
            var currentDate = new Date();
            var earlyDate = new Date("01/01/1850");
            if (validateDate === "Invalid Date") msg = "Invalid Date: Cannot convert into real date.";
            else if (earlyDate > validateDate) msg = "Invalid Date: Date entered is too far in the past.";
            else if (validateDate > currentDate) msg = "Invalid Date: Date entered is too far in the future.";
        }
    }
    else {
        //Required Notification
        msg = '<font color="red">Please enter the Date of Birth, it is required.</font>';
    }

    if (location === "field") {
        $("#DOBNotification")[0].setAttribute("aria-haspopup", true);
        msg = '<font color="red">' + msg + '</font>';
        $("#DateofBirthDateInput").val(formattedDOB);
        $("#DOBNotification").html(msg);
    }
    else return msg;
}

//Validate Fields before actual search
function validateSearch() {
    //Remove existing items
    $("#validationErrors").find("ul").remove();
    var html = "";

    var ssn = $("#SocialSecurityTextBox").val();

    html += ($("#FirstNameTextBox").val() == "") ? "<li>The First Name is missing.</li>" : "";
    html += ($("#LastNameTextBox").val() == "") ? "<li>The Last Name is missing.</li>" : "";

    if (ssn === "" || ssn === " 9 digits, no dashes") {
        html += "<li>The Social Security Number is missing.</li>";
    }
    else {
        var checkNumber = validateNumeric(ssn.trim());
        html += (checkNumber != "") ? "li>" + checkNumber + "</li>" : "";
    }

    html += (!$("#DateofBirthDateInput").val()) ? "<li>The Date of Birth is missing.</li>" : "";

    if ($("#DateofBirthDateInput").val()) {
        var DOBvalidation = (FormatDOB("button"));
        if (DOBvalidation !== "") html += "<li>" + DOBvalidation + "</li>";
    }

    if (html !== "") {
        html = "<ul id='errorList'>" + html + "</ul>";
        $("#validationErrors").append(html);
        return false;
    }
    else return true;
}

//Check if Social Security Number and if correct length
function validateNumeric(value) {
    var cleanNo = value.replace(/\D/g, '');

    if (value.length !== cleanNo.length) return "The Social Security Number cannot contain non numbers.";
    else if (cleanNo.length !== 9) return "The Social Security Number must be 9 numbers in length.";
    return "";
}
