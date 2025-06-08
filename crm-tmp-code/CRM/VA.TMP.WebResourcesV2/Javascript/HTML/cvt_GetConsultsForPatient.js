//Establish the Namespace if it doesn't already exist
if (typeof GetConsultsForPatient == "undefined") GetConsultsForPatient = {};
if (typeof parent.Xrm != "undefined") Xrm = parent.Xrm;
if (typeof MCS == "undefined" && typeof window.parent.MCS == "undefined") {
    MCS = Xrm.Page.getControl("WebResource_vialogin").getObject().contentWindow.MCS;
    window.parent.MCS = MCS;
}

GetConsultsForPatient.Rows = [];
GetConsultsForPatient.SeeMore = '<font color="blue"> See More</font>';
GetConsultsForPatient.SeeLess = '<font color="blue"> See Less</font>';

var TsaProviderSiteId;
var TsaPatientSiteId;
var isIntrafacility = false;

GetConsultsForPatient.HookToCrmPage = function () {
    Xrm.Page.getAttribute("cvt_rungetconsults").addOnChange(GetConsultsForPatient.RetrieveConsults);
};

GetConsultsForPatient.RefreshResults = function () {
    MCS = window.parent.MCS;
    var filterVal = $('#FilterType').val();
    var error = $("#errorDetails").text();
    if (error.indexOf("queryBean.provider.userId value has expired") != -1) {
        if (MCS.VIALogin.IsValidSamlToken()) Xrm.Page.getAttribute("cvt_samltoken").fireOnChange();
        else MCS.VIALogin.Saml();
    }
    else {
        GetConsultsForPatient.RetrieveConsults();
    }
    GetConsultsForPatient.FilterCombined();
};

GetConsultsForPatient.FilterResults = function () {
    var filterVal = $('#FilterOption').val();
    var rowVisibility = (filterVal == "P") ? "display:none": "";
    $('tr[ConsultType="NP"]').attr("style", rowVisibility);
};

GetConsultsForPatient.FilterTypeResults = function () {
    var filterVal = $('#FilterType').val();
    if (filterVal == "RTC") {
        $("#ConsultResultsGrid").hide();
        $("#RtcResultsGrid").show();
        document.getElementById("FilterOption").disabled = true;
    }
    else {
        $("#ConsultResultsGrid").show();
        $("#RtcResultsGrid").hide();
        document.getElementById("FilterOption").disabled = false;
    }
};

GetConsultsForPatient.FilterDateResults = function () {
    var filterVal = $('#FilterDate').val();
    if (filterVal == "Past30") {
        $('tr[Age="30"]').attr("style", "");
        $('tr[Age="90"]').attr("style", "display:none");
        $('tr[Age="91"]').attr("style", "display:none");
    }
    else if (filterVal == "Past90") {
        $('tr[Age="30"]').attr("style", "");
        $('tr[Age="90"]').attr("style", "");
        $('tr[Age="91"]').attr("style", "display:none");
    }
    else {
        $('tr[Age="30"]').attr("style", "");
        $('tr[Age="90"]').attr("style", "");
        $('tr[Age="91"]').attr("style", "");
    }
};

GetConsultsForPatient.FilterCombined = function () {
    var filterOption = $('#FilterOption').val();
    var filterVal = $('#FilterDate').val();
    var filterType = $('#FilterType').val();

    if (filterType == "Consult") {
        if (filterVal == "Past30" && filterOption == "P") //display 'Pending Resolution' (Pending + Active) created in last 30 days
        {
            $('tr[Age="30"],tr[ConsultType="P"]').attr("style", "");
            $('tr[ConsultType="NP"]').attr("style", "display:none");
            $('tr[Age="90"]').attr("style", "display:none");
            $('tr[Age="91"]').attr("style", "display:none");
        }
        else if (filterVal == "Past30" && filterOption == "All") //display all created in last 30 days
        {
            $('tr[Age="30"]').attr("style", "");
            $('tr[Age="90"]').attr("style", "display:none");
            $('tr[Age="91"]').attr("style", "display:none");
        }
        else if (filterVal == "Past90" && filterOption == "P") //display 'Pending Resolution' (Pending + Active) created in last 90 days
        {
            $('tr[Age="30"],tr[ConsultType="P"]').attr("style", "");
            $('tr[Age="90"],tr[ConsultType="P"]').attr("style", "");
            $('tr[ConsultType="NP"]').attr("style", "display:none");
            $('tr[Age="91"]').attr("style", "display:none");
        }
        else if (filterVal == "Past90" && filterOption == "All") //display all created in last 90 days
        {
            $('tr[Age="30"]').attr("style", "");
            $('tr[Age="90"]').attr("style", "");
            $('tr[Age="91"]').attr("style", "display:none");
        }
        else if (filterOption == "P") //display all 'Pending Resolution' (Pending + Active)
        {
            $('tr[ConsultType="P"]').attr("style", "");
            $('tr[ConsultType="NP"]').attr("style", "display:none");
        }
        else //display all consults
        {
            $('tr[Age="30"]').attr("style", "");
            $('tr[Age="90"]').attr("style", "");
            $('tr[Age="91"]').attr("style", "");
        }
    }
    else {
        if (filterVal == "Past30") {
            $('tr[Age="30"]').attr("style", "");
            $('tr[Age="90"]').attr("style", "display:none");
            $('tr[Age="91"]').attr("style", "display:none");
        }
        else if (filterVal == "Past90") {
            $('tr[Age="30"]').attr("style", "");
            $('tr[Age="90"]').attr("style", "");
            $('tr[Age="91"]').attr("style", "display:none");
        }
        else {
            $('tr[Age="30"]').attr("style", "");
            $('tr[Age="90"]').attr("style", "");
            $('tr[Age="91"]').attr("style", "");
        }
    }
};

GetConsultsForPatient.RetrieveConsults = function () {
    var group = Xrm.Page.getAttribute("mcs_groupappointment");
    if (group != null && group.getValue() != null && group.getValue()) return;

    var checkVistaSwitchesDeferred = GetConsultsForPatient.CheckVistaSwitches();

    $.when(checkVistaSwitchesDeferred).done(function (returnData) {

        if (returnData === false) return;

        GetConsultsForPatient.HideAll();

        Xrm.Page.ui.tabs.get('tab_9').setVisible(true);
        //formContext.ui.tabs.get('tab_9').setVisible(true);
        var isHomeMobile = Xrm.Page.getAttribute("cvt_type").getValue();
        var isSft = false;
        var sftObj = Xrm.Page.getAttribute("cvt_telehealthmodality");
        isSft = sftObj != null ? sftObj.getValue() : false;

        //  Pro and Pat fields will be always visible to avoid confusion
        //Xrm.Page.getControl("cvt_proconsultien").setVisible(!isSft);
        //Xrm.Page.getControl("cvt_prortcid").setVisible(!isSft);
        //Xrm.Page.getControl("cvt_rtcparentprovider").setVisible(!isSft);
        //Xrm.Page.getControl("cvt_patconsultien").setVisible(!isHomeMobile);
        //Xrm.Page.getControl("cvt_patrtcid").setVisible(!isHomeMobile);
        //Xrm.Page.getControl("cvt_rtcparentpatient").setVisible(!isHomeMobile);
        var patient = null;
        var patients = Xrm.Page.getAttribute("customers").getValue();
        if (patients != null && patients.length > 0) patient = patients[0].id;
        var patSiteObj = Xrm.Page.getAttribute("mcs_relatedsite");
        var patSite = patSiteObj != null && patSiteObj.getValue() != null ? patSiteObj.getValue()[0].id : null;
        var proSiteObj = Xrm.Page.getAttribute("mcs_relatedprovidersite");
        var proSite = proSiteObj != null && proSiteObj.getValue() != null ? proSiteObj.getValue()[0].id : null;
        //var proFacilityObj = Xrm.Page.getAttribute("cvt_providerfacility");
        //var proFacility = proFacilityObj != null && proFacilityObj.getValue() != null ? proFacilityObj.getValue()[0].id : null;
        var proFacilityDeferred = GetConsultsForPatient.GetFacilityId();

        $.when(proFacilityDeferred).done(function (returnData) {
            var proFacility = returnData;

            //Fallbacks making use of TSA OData Query from Switch Statement in case form fields aren't populated
            if (proSite == null) proSite = TsaProviderSiteId;
            if (patSite == null) patSite = TsaPatientSiteId;
            if (patient != null && Xrm.Page.getAttribute("cvt_relatedschedulingpackage") != null && Xrm.Page.getAttribute("cvt_relatedschedulingpackage").getValue() != null) {
                $("#msgBoxWorking").show();

                var callIntegrationDeferred = GetConsultsForPatient.CallIntegration(patient, proSite, proFacility, patSite, isHomeMobile, isSft);

                $.when(callIntegrationDeferred).done(function (returnData) {
                    if (returnData != null) GetConsultsForPatient.DisplayResults(returnData);
                });
            }
            else {
                var message = "";
                if (patient == null) message = "There is no patient selected, please select a SP before selecting the patient consult";

                if (Xrm.Page.getAttribute("cvt_relatedschedulingpackage") == null || Xrm.Page.getAttribute("cvt_relatedschedulingpackage").getValue() == null) message = "There is no SP selected, please select a SP before selecting the patient's consult";

                GetConsultsForPatient.ShowWarningMessage(message);
            }
        });
    });
};

GetConsultsForPatient.CallIntegration = function (patientId, providerSiteId, providerFacilityId, patientSiteId, isHomeMobile, isSft) {
    var deferred = $.Deferred();

    //Determine whether to use Action or OData call to CRME_RetrieveMultiplePostStageRunner (like person search)
    var results;
    var patDuz = Xrm.Page.getAttribute("cvt_patuserduz").getValue();
    var proDuz = Xrm.Page.getAttribute("cvt_prouserduz").getValue();
    var filter = "cvt_integrationtype eq 917290001";
    var proStationCode = Xrm.Page.getAttribute("cvt_providerstationcode").getValue();

    if (patientId != null) filter += " and crme_contactid eq '" + patientId.replace("{", "").replace("}", "") + "'";
    if (patientSiteId != null) filter += " and cvt_patientsiteid eq '" + patientSiteId.replace("{", "").replace("}", "") + "'";
    if (providerSiteId != null) filter += " and cvt_providersiteid eq '" + providerSiteId.replace("{", "").replace("}", "") + "'";
    if (providerFacilityId != null) filter += " and cvt_providerfacilityid eq '" + providerFacilityId.replace("{", "").replace("}", "") + "'";
    if (proStationCode != null) filter += " and cvt_providerstationcode eq '" + proStationCode + "'";

    filter += " and cvt_issft eq '" + isSft.toString() + "'";
    filter += " and cvt_ishomemobile eq '" + isHomeMobile.toString() + "'";

    if (patDuz != null) filter += " and cvt_patuserduz eq '" + patDuz + "'";
    if (proDuz != null) filter += " and cvt_prouserduz eq '" + proDuz + "'";

    var integrationResults = null;

    console.log("filter: " + filter);
    var select = 'cvt_consulttitle, cvt_consultien, cvt_consultstatus, cvt_consulttimestamp, cvt_clinicallyindicateddate, cvt_issft, cvt_ishomemobile, cvt_providerstationcode, crme_url, cvt_consulttext, cvt_consulttype, cvt_rtcid, cvt_rtcrequestdatetime, cvt_clinicien, cvt_clinicname, cvt_stopcodes, cvt_provider, cvt_comments, cvt_numberofappointments, cvt_interval, cvt_rtcparent, cvt_receivingsite';

    Xrm.WebApi.retrieveMultipleRecords("crme_person", "?$select=" + select + "&$filter=" + filter).then(
        function success(results) {
        if (results && results.entities != null) {
            if (results.entities.length == 0 || results.entities.length == 1) {
                $("#msgBoxWorking").hide();
                GetConsultsForPatient.ShowWarningMessage("No Consults/Return to Clinics were found");
            }
            else {
                integrationResults = results;
                GetConsultsForPatient.DisplayResults(results);
            }

            deferred.resolve(integrationResults);
            //return integrationResults;
        }
        else {
            deferred.resolve(null);
        }
        //return null;
    },
    function (error) {
        // var friendlyError = "Failure in Get Consults/Return to Clinics For Patient: " + parent.MCS.cvt_Common.RestError(err);
        var friendlyError = "Failure in Get Consults/Return to Clinics For Patient: " + error.message;
        $("#msgBoxWorking").hide();
        GetConsultsForPatient.ShowErrorMessage(friendlyError);
        deferred.resolve(null);
        //return null;
    });
    //return integrationResults;
    return deferred.promise();
};

GetConsultsForPatient.DisplayResults = function (data) {
    var consultResultsTable = $("#ConsultResultsTable");
    consultResultsTable.find("thead, tr, th").remove();
    consultResultsTable.find("tr:gt(0)").remove();

    var rtcResultsTable = $("#RtcResultsTable");
    rtcResultsTable.find("thead, tr, th").remove();
    rtcResultsTable.find("tr:gt(0)").remove();

    if (data != null && data.entities != null && data.entities.length > 0) {
        var error = GetConsultsForPatient.Failure(data.entities);
        if (error != "") {
            GetConsultsForPatient.ShowErrorMessage(error);
            return;
        }
        //Todo:Show/Hide Tables based on the optionSet Selection
        GetConsultsForPatient.BindConsultsResults(consultResultsTable, data.entities);
        GetConsultsForPatient.BindReturnToClinicsResults(rtcResultsTable, data.entities);
    }
    else {
        $("#warningDetails").text("");
        $("#warningDetails").append("No Consults/Return To Clinics were found for this station");
        $("#msgBoxNoDataReturned").show();
    }

    $("#msgBoxWorking").hide();
};

GetConsultsForPatient.SetConsultValues = function (selectedRow) {
    document.getElementById("FilterType").disabled = true;
    document.getElementById("RefreshButton").disabled = true;

    var ien = selectedRow.getAttribute("cvt_ConsultIEN");
    var cvt_loc = selectedRow.getAttribute("location").toLowerCase();
    var isHomeMobile = Xrm.Page.getAttribute("cvt_type").getValue();
    var ReceivingSite = selectedRow.getAttribute("ReceivingSiteId");
    var ReceivingSiteConsult = selectedRow.getAttribute("ReceivingSiteConsultId");
    var cid = selectedRow.getAttribute("cvt_ClinicallyIndicatedDate").toLowerCase();
    if (cid != undefined) Xrm.Page.getAttribute("cvt_clinicallyindicateddate").setValue(cid);

    if (cvt_loc == "patient") {
        Xrm.Page.getAttribute("cvt_patconsultien").setValue(ien);
        var proConsultIen = Xrm.Page.getAttribute("cvt_proconsultien").getValue();

        proConsultIen = ((proConsultIen != null) && (proConsultIen != "undefined")) ? proConsultIen : "";
        Xrm.Page.getControl("cvt_proconsultien").setValue(proConsultIen);

        if (!isHomeMobile && ReceivingSiteConsult != undefined) { //&& (proConsultIen == null || proConsultIen == "")
            //Need to set this to the Provider Site IEN value
            //Where is this value, does it need to be split out, is it in the table? Is it Intra/Inter dependent?//
            Xrm.Page.getAttribute("cvt_proconsultien").setValue(ReceivingSiteConsult);
        }
    }
    else {
        Xrm.Page.getAttribute("cvt_proconsultien").setValue(ien);
        var patConsultIen = Xrm.Page.getAttribute("cvt_patconsultien").getValue();

        patConsultIen = ((patConsultIen != null) && (patConsultIen != "undefined")) ? patConsultIen : "";
        Xrm.Page.getControl("cvt_patconsultien").setValue(patConsultIen);

        if (!isHomeMobile && ReceivingSiteConsult != undefined) { //&& (patConsultIen == null || patConsultIen == "")
            //Need to set this to the Patient Site IEN value....
            Xrm.Page.getAttribute("cvt_patconsultien").setValue(ReceivingSiteConsult);
        }
    }

    $("#selectedConsult").append("<br/>Added Consult with IEN: " + ien + " to appointment booking on " + cvt_loc + " side.");
    $("#informUserConsultAdded").show();

    GetConsultsForPatient.RemoveOtherConsultsFromSameLocation(selectedRow.parentElement, selectedRow);
};

GetConsultsForPatient.ShowErrorMessage = function (errorMessage) {
    $("#errorDetails").text("");
    $("#errorDetails").append(errorMessage);

    $("#msgBoxSearchResultError").show();
    $("#msfBoxSearchResultError").focus();
};

GetConsultsForPatient.ShowWarningMessage = function (message) {
    $("#warningDetails").text("");
    $("#warningDetails").append(message);

    $("#msgBoxNoDataReturned").show();
};

GetConsultsForPatient.HideAll = function () {
    $("#msgBoxNoDataReturned").hide();
    $("#ConsultResultsGrid").hide();
    $("#RtcResultsGrid").hide();
    $("#msgBoxSearchResultError").hide();
    $("#msgBoxFailedValidation").hide();
    $("#msgBoxWorking").hide();
};

GetConsultsForPatient.GetRtcValues = function (data, row, counter) {
    var rtcId = data.cvt_rtcid;
    row.setAttribute("cvt_RtcId", rtcId);

    var rtcParent = data.cvt_rtcparent;
    row.setAttribute("cvt_rtcparent", rtcParent);

    var ien = data.cvt_clinicien;
    row.setAttribute("cvt_ClinicIen", ien);

    var rtcRequestDateTime = data.cvt_rtcrequestdatetime;
    row.setAttribute("cvt_RtcRequestDateTime", rtcRequestDateTime);

    var clinicallyIndicatedDate = data.cvt_clinicallyindicateddate.substr(0, 10);
    row.setAttribute("cvt_ClinicallyIndicatedDate", clinicallyIndicatedDate);

    var clinicName = data.cvt_clinicname;
    row.setAttribute("cvt_ClinicName", clinicName);

    var stopCodes = data.cvt_stopcodes;
    row.setAttribute("cvt_StopCodes", stopCodes);

    var provider = data.cvt_provider;
    row.setAttribute("cvt_Provider", provider);

    var comments = data.cvt_comments;
    row.setAttribute("cvt_Comments", comments);

    var side = data.crme_url;
    row.setAttribute("location", side);

    row.setAttribute("status", "less");

    var formattedCid = 'Not Available';
    var consultCid = new Date(clinicallyIndicatedDate);
    if (consultCid != null && consultCid != undefined) {
        consultCid.setDate(consultCid.getDate() + 1);
        formattedCid = (consultCid.getMonth() + 1) + '/' + consultCid.getDate() + '/' + consultCid.getFullYear();
    }

    var rtcRequestedFormattedDate = 'Not Available';
    var rtcRequestDate = new Date(rtcRequestDateTime);
    if (rtcRequestDate != null && rtcRequestDate != undefined) {
        rtcRequestDate.setDate(rtcRequestDate.getDate() + 1);
        rtcRequestedFormattedDate = (rtcRequestDate.getMonth() + 1) + '/' + rtcRequestDate.getDate() + '/' + rtcRequestDate.getFullYear();

        var filterDate = $('#FilterDate').val();
        if (filterDate != null) {
            var todayDate = new Date();
            var ageInDays = (todayDate - rtcRequestDate) / 86400000;
            if (ageInDays > 90) ageInDays = "91";
            else if (ageInDays <= 30) ageInDays = "30";
            else ageInDays = "90";

            if (filterDate == "Past30" && ageInDays != "30") row.setAttribute("style", "display:none");
            else if (filterDate == "Past90" && (ageInDays == "91")) // (ageInDays == "90" || ageInDays == "30"))
            row.setAttribute("style", "display:none");
            else row.setAttribute("style", "");

            row.setAttribute("Age", ageInDays);
        }
    }

    return [rtcId, rtcRequestedFormattedDate, formattedCid, ien, clinicName, stopCodes, provider, comments, side];
};

GetConsultsForPatient.GetValues = function (data, row, counter) {
    var title = data.cvt_consulttitle;
    row.setAttribute("cvt_ConsultTitle", title);

    var ien = data.cvt_consultien;
    row.setAttribute("cvt_ConsultIEN", ien);

    var consultCreatedOn = data.cvt_consulttimestamp;
    row.setAttribute("cvt_ConsultTimestamp", consultCreatedOn);

    var clinicallyIndicatedDate = data.cvt_clinicallyindicateddate.substr(0, 10);
    row.setAttribute("cvt_ClinicallyIndicatedDate", clinicallyIndicatedDate);

    var status = data.cvt_consultstatus;
    row.setAttribute("cvt_ConsultStatus", status);

    var filterVal = $('#FilterOption').val();
    //debugger;
    if ((status != "Pending") && (status != "Active")) {
        if (filterVal == "P") {
            row.setAttribute("style", "display:none");
        }
        row.setAttribute("ConsultType", "NP");
    } else {
        row.setAttribute("ConsultType", "P");
    }

    var side = data.crme_url;
    row.setAttribute("location", side);
    row.setAttribute("status", "less");

    var formattedCid = 'Not Available';
    var consultCid = new Date(clinicallyIndicatedDate);

    if (consultCid != null && consultCid != undefined) {
        consultCid.setDate(consultCid.getDate() + 1);
        formattedCid = (consultCid.getMonth() + 1) + '/' + consultCid.getDate() + '/' + consultCid.getFullYear();
    }

    var formattedTimeStamp = 'Not Available';
    var consultCreatedDate = new Date(consultCreatedOn);

    if (consultCreatedDate != null && consultCreatedDate != undefined) formattedTimeStamp = (consultCreatedDate.getMonth() + 1) + '/' + consultCreatedDate.getDate() + '/' + consultCreatedDate.getFullYear();

    var filterDate = $('#FilterDate').val();
    if (filterDate != null) {
        var todayDate = new Date();
        var ageInDays = (todayDate - consultCreatedDate) / 86400000;
        if (ageInDays > 90) ageInDays = "91";
        else if (ageInDays <= 30) ageInDays = "30";
        else ageInDays = "90";

        if (filterDate == "Past30" && ageInDays == "30") {
            if ((status != "Pending" && status != "Active") && filterVal == "P") row.setAttribute("style", "display:none");
            else row.setAttribute("style", "");
        }
        else if (filterDate == "Past90" && (ageInDays == "90" || ageInDays == "30")) {
            if ((status != "Pending" && status != "Active") && filterVal == "P") row.setAttribute("style", "display:none");
            else row.setAttribute("style", "");
        }
        else {
            if ((status != "Pending" && status != "Active") && filterVal == "P") row.setAttribute("style", "display:none");
            else row.setAttribute("style", "");
        }
        row.setAttribute("Age", ageInDays);
    }

    var receivingSite = data.cvt_receivingsite;
    var receivingSiteId = null;
    var receivingSiteConsultId = null;

    if (receivingSite != null || receivingSite != "") {
        receivingSite = receivingSite.toString().split(",");
        receivingSiteId = receivingSite.length > 0 ? receivingSite[0] : "";
        receivingSiteConsultId = receivingSite.length > 0 ? receivingSite[1] : "";

        row.setAttribute("receivingSiteId", receivingSiteId);
        row.setAttribute("receivingSiteConsultId", receivingSiteConsultId);
    }
    return [title.toLowerCase(), ien, formattedTimeStamp, formattedCid, status, side, receivingSiteId, receivingSiteConsultId];
};

GetConsultsForPatient.SetUserDuzs = function (data) {
    var patDuz = data[0].cvt_consultien; //arbitrarily re-purposing consult IEN for pat side user duz in plugin
    var proDuz = data[0].cvt_consulttext; //arbitrarily re-purposing consult text for pro side user duz in plugin
    var patFieldObj = parent.Xrm.Page.getAttribute("cvt_patuserduz");
    var proFieldObj = parent.Xrm.Page.getAttribute("cvt_prouserduz");

    if (patFieldObj.getValue() != null) patFieldObj.setValue(patDuz);
    if (proFieldObj.getValue() != null) proFieldObj.setValue(proDuz);

    patFieldObj.setSubmitMode("always");
    proFieldObj.setSubmitMode("always");
};

GetConsultsForPatient.ToggleText = function (cell, row) {
    if (row.getAttribute("status") == "less") {
        cell.innerHTML = row.getAttribute("cvt_ConsultText");
        cell.innerHTML += GetConsultsForPatient.SeeLess;
        row.setAttribute("status", "more");
    } else if (row.getAttribute("status") == "more") {
        cell.innerHTML = row.getAttribute("displayText");
        cell.innerHTML += GetConsultsForPatient.SeeMore;
        row.setAttribute("status", "less");
    }
};

GetConsultsForPatient.Failure = function (data) {
    var errorMessage = "";
    for (var i = 0; i < data.length; i++) {
        if (data[i].crme_url == "fail") errorMessage = data[i].cvt_consulttitle;
    }
    return errorMessage;
};

GetConsultsForPatient.RemoveOtherConsultsFromSameLocation = function (table, row) {
    var isProvider = row.getAttribute("location").toLowerCase() == "provider";
    var tableRows = table.children;

    //Skip header row, start with row index 1; use decrementer if a row has been removed to cancel out the table size change
    for (var i = 1; i < tableRows.length; i++) {
        var rowLocation = tableRows[i].getAttribute("location");
        if (isProvider) {
            if (rowLocation.toLowerCase() == "provider") table.deleteRow(i--);
        }
        else {
            if (rowLocation.toLowerCase() == "patient") table.deleteRow(i--);
        }
    }
};

GetConsultsForPatient.CheckVistaSwitches = function () {
    var deferred = $.Deferred();
    var returnData = {
        success: true,
        data: {}
    };

    var VistaSwitchesConfig = true;
    var baseSwitchConfig = true;
    var hmConfig = true;
    var ifcConfig = true;
    var singleNonHmConfig = true;
    var filter = "mcs_name eq 'Active Settings'";
    var select = 'cvt_usevistaintegration, cvt_usevvshomemobile, cvt_usevvsinterfacility, cvt_usevvssingleencounternonhomemobile';

    Xrm.WebApi.retrieveMultipleRecords("mcs_setting", "?$select=" + select + "&$filter=" + filter).then(
    function success(results) {
        if (results && results.entities != null) {
            if (results.entities.length != 0) {
                var record = results.entities[0];
                baseSwitchConfig = record.cvt_usevistaintegration != null ? record.cvt_usevistaintegration : true;
                hmConfig = record.cvt_usevvshomemobile != null ? record.cvt_usevvshomemobile : true;
                ifcConfig = record.cvt_usevvsinterfacility != null ? record.cvt_usevvsinterfacility : true;
                singleNonHmConfig = record.cvt_usevvssingleencounternonhomemobile != null ? record.cvt_usevvssingleencounternonhomemobile : true;
            }

            //Note that this is a triple equal, not a double, so a null value is considered acceptable to continue, only a false will mean "don't show Get Consults"
            if (baseSwitchConfig === false) deferred.resolve(false);

            var appointmentTypeSwitchCheckDeferred;

            if (typeof Xrm != "undefined") appointmentTypeSwitchCheckDeferred = MCS.VIALogin.AppointmentTypeSwitchCheck(hmConfig, ifcConfig, singleNonHmConfig, Xrm);
            else appointmentTypeSwitchCheckDeferred = MCS.VIALogin.AppointmentTypeSwitchCheck(hmConfig, ifcConfig, singleNonHmConfig, parent.Xrm);

            $.when(appointmentTypeSwitchCheckDeferred).done(function (returnData) {
                if (returnData === false) {

                    deferred.resolve(false);
                }
                else {

                    // patFacId and proFacId are null currently, if we need to set them, set them in MCS.VIALogin.AppointmentTypeSwitchCheck
                    var facilitySwitch = true;
                    var patFacId = null;
                    var proFacId = null;

                    if (patFacId != null && proFacId != null && patFacId == proFacId) {
                        Xrm.WebApi.retrieveRecord("mcs_facility", patFacId, "?$select=cvt_usevistaintrafacility").then(
                        function success(result) {
                            if (result !== null) facilitySwitch = result.cvt_usevistaintrafacility != null ? result.cvt_usevistaintrafacility : true;
                            deferred.resolve(facilitySwitch === false ? false : true);
                        },
                        function (error) {
                            // alert("failed specialty type check, defaulting to display Consults" + MCS.cvt_Common.RestError(error));
                            alert("failed specialty type check, defaulting to display Consults" + error);
                        });
                    }
                    else {
                        deferred.resolve(facilitySwitch === false ? false : true);
                    }
                }
            });
        }
        else {
            deferred.resolve(false);
        }
    },
    function (error) {
        deferred.resolve(VistaSwitchesConfig);
    });

    return deferred.promise();
};

// GetConsultsForPatient.AppointmentTypeSwitchCheck = function (hmConfig, ifcConfig, singleNonHmConfig, Xrm) {
GetConsultsForPatient.GetFacilityId = function () {
    var facilityIdDeferred = $.Deferred();

    var tsaObj = Xrm.Page.getAttribute("cvt_relatedschedulingpackage").getValue();
    if (tsaObj == null) return false;
    var tsaId = tsaObj[0].id.replace("{", "").replace("}", "");

    Xrm.WebApi.retrieveRecord('cvt_resourcepackage', tsaId, '?$select=_cvt_providerfacility_value').then(
    function success(result) {
        var providerFacility = result['_cvt_providerfacility_value'];
        facilityIdDeferred.resolve(providerFacility);
    },
    function (error) {
        facilityIdDeferred.resolve(null);
    });

    return facilityIdDeferred.promise();
};

GetConsultsForPatient.AppointmentTypeSwitchCheck = function (hmConfig, ifcConfig, singleNonHmConfig, Xrm) {
    var deferred = $.Deferred();

    var tsaObj = Xrm.Page.getAttribute("cvt_relatedschedulingpackage").getValue();
    if (tsaObj == null) return false;
    var tsaId = tsaObj[0].id;

    var subSpecialty = null;
    var specialty = null;
    var isInterFacility = false;

    var isHomeMobile = Xrm.Page.getAttribute("cvt_type").getValue();

    if (isHomeMobile) deferred.resolve(hmConfig);

    var isSingleNonHm = Xrm.Page.getAttribute("cvt_telehealthmodality").getValue();
    if (isSingleNonHm) deferred.resolve(singleNonHmConfig);

    if (isInterFacility) deferred.resolve(ifcConfig);

    Xrm.WebApi.retrieveRecord('mcs_resourcepackage', tsaId, '?&select=cvt_specialty,cvt_specialtysubtype,cvt_intraorinterfacility').then(
    function success(result) {
        if (result != null) {

            //if (data.d.cvt_PatientFacility != null)
            //    patFacId = data.d.cvt_PatientFacility.Id;
            //if (data.d.cvt_ProviderFacility != null)
            //    proFacId = data.d.cvt_ProviderFacility.Id;
            //if (data.d.cvt_ServiceScope != null)
            //    isInterFacility = data.d.cvt_ServiceScope.Value == 917290000;
            if (result.cvt_servicetype != null) specialty = resultcvt_specialty.Id;

            if (result.cvt_servicesubtype != null) subSpecialty = result.cvt_specialtysubtype.Id;

            if (result.cvt_intraorinterfacility != null) isIntrafacility = result.cvt_intraorinterfacility.Value == 917290000;

            //if (data.d.cvt_relatedprovidersiteid != null)
            //    TsaProviderSiteId = data.d.cvt_relatedprovidersiteid.Id; //This is key to run each time TSA is set because the Provider Site does not come through until after TSA js has run, but this function runs before TSA js is complete, so pulling in TSAProviderSiteId will set providerSiteId when its not otherwise populated
            //if (data.d.cvt_relatedprovidersiteid != null)
            //    TsaPatientSiteId = data.d.cvt_relatedpatientsiteid.Id;
            if (subSpecialty != null) {
                //Check SubSpecialty Switch
                Xrm.WebApi.retrieveRecord("mcs_servicesubtype", subSpecialty, "?$select=cvt_usevvs").then(
                function success(result) {
                    subSpecialtySwitch = result.cvt_usevvs;

                    //various "===" is so that if the value is null, then we evaluate the next condition instead of considering it false
                    if (subSpecialtySwitch === false) deferred.resolve(false);

                    var specialtySwitch = true;

                    if (specialty != null) {

                        Xrm.WebApi.retrieveRecord("mcs_servicetype", specialty, "?$select=cvt_usevvs").then(
                        function success(result) {
                            specialtySwitch = result.cvt_usevvs;
                            if (specialtySwitch === false) deferred.resolve(false);
                            else deferred.resolve(true);
                        },
                        function (error) {
                            // alert("failed specialty type check, defaulting to display Consults" + MCS.cvt_Common.RestError(error));
                            alert("failed specialty type check, defaulting to display Consults" + error);
                            deferred.resolve(true);
                        });
                    }
                    else {
                        deferred.resolve(true);
                    }
                },
                function (error) {
                    // alert("failed sub-specialty type check, looking for specialty switch" + MCS.cvt_Common.RestError(error));
                    alert("failed sub-specialty type check, looking for specialty switch" + error);
                    deferred.resolve(true);
                });
            }
            else {
                deferred.resolve(true);
            }
        }
        else {
            deferred.resolve(true);
        }
    },
    function (error) {
        alert("failed TSA check, defaulting to display Consults" + error);
        deferred.resolve(true);
    });

    //return facilitySwitch === false ? false : true;
    return deferred.promise();
};

GetConsultsForPatient.BindConsultsResults = function (consultResultsTable, data) {
    var filterVal = $('#FilterType').val();
    if (filterVal == "Consult") {
        $("#ConsultResultsGrid").show();
        $("#RtcResultsGrid").hide();
    }

    //var consultTableHeaders = ['Title', 'IEN', 'Consult Created Date', 'Clinically Indicated Date', 'Status', 'Location', 'ReceivingSiteId', 'ReceivingSiteConsultId']; //'Text',
    var consultTableHeaders = ['Title', 'IEN', 'Consult Created Date', 'Clinically Indicated Date', 'Status', 'Location']; //'Text',
    var thead = document.createElement('tr');
    for (var i = 0; i < consultTableHeaders.length; i++) {
        var th = document.createElement('th');
        th.className = "grid_title_left";
        th.appendChild(document.createTextNode(consultTableHeaders[i]));
        th.id = "th_" + consultTableHeaders[i].replace(' ', '');
        th.title = consultTableHeaders[i];
        if (consultTableHeaders[i] == "Text") { //|| consultTableHeaders[i] == "ReceivingSiteId" || consultTableHeaders[i] == "ReceivingSiteConsultId"
            th.style = "display:none";
        }
        th.scope = "col";
        thead.appendChild(th);
    }
    consultResultsTable.append(thead);
    GetConsultsForPatient.SetUserDuzs(data);

    //Skip first row because it has been re-purposed to pass across the Pat and Pro User Duz so user only logs in 1x during GetConsults, then login is re-used during make appointment
    for (var i = 1; i < data.length; i++) {
        if (data[i].cvt_consulttype == true) {
            var row = document.createElement('tr');
            row.id = i;
            var crmValues = GetConsultsForPatient.GetValues(data[i], row, i);
            for (var j = 0; j < consultTableHeaders.length; j++) {
                var td;
                if (j == 0) {
                    td = document.createElement('th');
                    td.scope = "row";
                    td.className = "th_row_left";
                }
                else {
                    td = document.createElement('td');
                }
                td.id = "th_" + i + "_" + j + "_" + consultTableHeaders[j].replace(' ', '');
                td.title = crmValues[j];

                td.appendChild(document.createTextNode(crmValues[j]));

                row.appendChild(td);
            }
            row.className = "grid_row_left";
            row.ondblclick = function () {
                GetConsultsForPatient.SetConsultValues(this);
            };
            //If intrafacility, only append rows that have no receiving consult data
            //If interfacility, only append rows that have receiving consult data
            //if ((isIntrafacility && row.getAttribute('ReceivingSiteConsultId') == null) || (!isIntrafacility && row.getAttribute('ReceivingSiteConsultId') != null))
            consultResultsTable.append(row);
        }
    }
    //force focus to message
    GetConsultsForPatient.FilterCombined();
    consultResultsTable.focus();
};

//Todo: Review and refactor this method
GetConsultsForPatient.BindReturnToClinicsResults = function (rtcResultsTable, data) {
    var filterVal = $('#FilterType').val();
    if (filterVal != "Consult") {
        $("#RtcResultsGrid").show();
        $("#ConsultResultsGrid").hide();
    }

    var rtcTableHeaders = ['RTC ID', 'RTC Request Date', 'Clinically Indicated Date', 'Clinic IEN', 'Clinic Name', 'Stop Codes', 'Provider', 'Comments', 'Location']; //'Text',
    var thead = document.createElement('tr');
    for (var i = 0; i < rtcTableHeaders.length; i++) {
        var th = document.createElement('th');
        th.className = "grid_title_left";
        th.appendChild(document.createTextNode(rtcTableHeaders[i]));
        th.id = "th_" + rtcTableHeaders[i].replace(' ', '');
        th.title = rtcTableHeaders[i];
        th.scope = "col";
        thead.appendChild(th);
    }
    rtcResultsTable.append(thead);

    //Skip first row because it has been re-purposed to pass across the Pat and Pro User Duz so user only logs in 1x during GetConsults, then login is re-used during make appointment
    for (var i = 1; i < data.length; i++) {
        if (data[i].cvt_consulttype != true) {
            var row = document.createElement('tr');
            row.id = i;
            var crmValues = GetConsultsForPatient.GetRtcValues(data[i], row, i);
            for (var j = 0; j < rtcTableHeaders.length; j++) {
                var td;
                if (j == 0) {
                    td = document.createElement('th');
                    td.scope = "row";
                }
                else {
                    td = document.createElement('td');
                }
                td.id = "th_" + i + "_" + j + "_" + rtcTableHeaders[j].replace(' ', '');
                td.title = crmValues[j];
                td.appendChild(document.createTextNode(crmValues[j]));

                row.appendChild(td);
            }
            row.className = "grid_row_left";
            row.ondblclick = function () {
                GetConsultsForPatient.SetRtcValues(this);
            };
            rtcResultsTable.append(row);
        }
    }
    //force focus to message
    rtcResultsTable.focus();
};

GetConsultsForPatient.SetRtcValues = function (selectedRow) {
    document.getElementById("FilterType").disabled = true;
    document.getElementById("RefreshButton").disabled = true;

    var rtcId = selectedRow.getAttribute("cvt_RtcId");
    var rtcParent = selectedRow.getAttribute("cvt_rtcparent");
    var patientprovider = selectedRow.getAttribute("location").toLowerCase();
    var cid = selectedRow.getAttribute("cvt_ClinicallyIndicatedDate").toLowerCase();

    if (cid != undefined) Xrm.Page.getAttribute("cvt_clinicallyindicateddate").setValue(cid);

    if (patientprovider == "patient") {
        Xrm.Page.getAttribute("cvt_patrtcid").setValue(rtcId);
        Xrm.Page.getAttribute("cvt_rtcparentpatient").setValue(rtcParent);
    } else {
        Xrm.Page.getAttribute("cvt_prortcid").setValue(rtcId);
        Xrm.Page.getAttribute("cvt_rtcparentprovider").setValue(rtcParent);

        //Populating the patient RTC and RTC Parent with the same value as Provider as requested by Phil (HealthShare)
        var patrtcId = formContext.getAttribute("cvt_patrtcid").getValue();
        if (patrtcId == null || patrtcId == "") Xrm.Page.getAttribute("cvt_patrtcid").setValue(rtcId);

        var patrtcParentId = formContext.getAttribute("cvt_rtcparentpatient").getValue();
        if (patrtcParentId == null || patrtcParentId == "") Xrm.Page.getAttribute("cvt_rtcparentpatient").setValue(rtcParent);
    }

    $("#selectedConsult").append("<br/>Added Return to Clinic with RTC Id: " + rtcId + " to appointment booking on " + patientprovider + " side.");
    $("#informUserConsultAdded").show();

    GetConsultsForPatient.RemoveOtherConsultsFromSameLocation(selectedRow.parentElement, selectedRow);
};