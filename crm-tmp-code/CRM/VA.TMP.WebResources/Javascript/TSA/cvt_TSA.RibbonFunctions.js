
//SD: web-use-strict-equality-operators
if (typeof MCS === "undefined")
    MCS = {};
//SD: web-use-strict-equality-operators
if (typeof MCS.TSA_Ribbon === "undefined")
    MCS.TSA_Ribbon = {};

var process = [[], [], [], []];
var facilities;
var PatFacility;
var ProFacility;

//Ribbon button calls this function to create a new service activity and pass in the fields for: Specialty and sub-type, service, capacity, name, group, modality, and type
MCS.TSA_Ribbon.CreateNewServiceActivity = function (executionContext) {
    //SD - Start
    //web-use-strict-mode
    "use strict";
    //SD - End
    var formContext = executionContext.getFormContext();
    var relatedPatientSiteValue = MCS.cvt_Common.checkNull("cvt_relatedpatientsiteid");
    if (relatedPatientSiteValue != null)
        relatedPatientSiteValue = relatedPatientSiteValue[0];

    var relatedProviderSiteValue = MCS.cvt_Common.checkNull("cvt_relatedprovidersiteid");
    if (relatedProviderSiteValue != null)
        relatedProviderSiteValue = relatedProviderSiteValue[0];

    var serviceTypeValue = MCS.cvt_Common.checkNull("cvt_servicetype");
    var serviceSubTypeValue = MCS.cvt_Common.checkNull("cvt_servicesubtype");
    var serviceValue = MCS.cvt_Common.checkNull("mcs_relatedserviceid");
    var TSAName = MCS.cvt_Common.checkNull("mcs_name");
    var groupApptOptionValue = MCS.cvt_Common.checkNull("cvt_groupappointment");
    var modalityValue = MCS.cvt_Common.checkNull("cvt_availabletelehealthmodalities");
    var TypeOptionValue = MCS.cvt_Common.checkNull("cvt_type");
    var instructionsValue = MCS.cvt_Common.checkNull("cvt_schedulinginstructions");

    var tsa = { id: formContext.data.entity.getId(), name: TSAName };

    MCS.TSA_Ribbon.openNewServiceAppointment(groupApptOptionValue, relatedProviderSiteValue, tsa, relatedPatientSiteValue, TypeOptionValue, modalityValue, instructionsValue);
};

//Refactored such that it is all happening in 1 function
MCS.TSA_Ribbon.openNewServiceAppointment = function (group, provSite, tsa, patSite, type, modality, instructions) {
    //SD - Start
    //web-use-strict-mode
    "use strict";
    //SD - End
    //SD: web-use-strict-equality-operators
    group = group == null ? false : group;
    type = type == null ? false : type;
    SFT = modality === 917290001;
    //SD: web-use-strict-equality-operators
    if (typeof Xrm !== "undefined" && typeof Xrm.Utility !== "undefined") {
        var p = {
            formid: "75ec0e60-421b-4e71-8f5d-d8de7e0aa04e",
            mcs_groupappointment: group,
            mcs_relatedtsa: tsa.id,
            mcs_relatedtsaname: tsa.name,
            cvt_type: type,
            cvt_telehealthmodality: SFT,
            cvt_schedulinginstructions: instructions
        };
        if (provSite != null) {
            p.mcs_relatedprovidersite = provSite.id;
            p.mcs_relatedprovidersitename = provSite.name;
        }
        if (patSite != null) {
            p.mcs_relatedsite = patSite.id;
            p.mcs_relatedsitename = patSite.name;
        }
        Xrm.Navigation.openForm("serviceappointment", null, p);
    }
    else {
        var features = "location=no,menubar=no,status=no,toolbar=no,resizable=yes";
        var extraqs = ["formid=75ec0e60-421b-4e71-8f5d-d8de7e0aa04e",
            "mcs_groupappointment=" + group,
            "mcs_relatedtsa=" + tsa.id,
            "mcs_relatedtsaname=" + tsa.name,
            "cvt_type=" + type,
            "cvt_telehealthmodality=" + SFT,
            "cvt_schedulinginstructions=" + instructions
        ];
        if (provSite != null)
            extraqs.concat["mcs_relatedprovidersite=" + provSite.id, "mcs_relatedprovidersitename=" + provSite.name];
        if (patSite != null)
            extraqs.concat["mcs_relatedsite=" + patSite.id, "mcs_relatedsitename=" + patSite.name];

        var globalContext = Xrm.Utility.getGlobalContext();
        var url = globalContext.getClientUrl();
        //var url = Xrm.Page.context.getClientUrl();
        if (url.match(/\/$/)) {
            url = url.substring(0, url.length - 1);
        }

        //SD: web-use-strict-equality-operators
        if (typeof globalContext.getClientUrl !== "undefined") {
            url = globalContext.getClientUrl();
            // if (typeof Xrm.Page.context.getClientUrl != "undefined") {
            //    url = Xrm.Page.context.getClientUrl();
        }
        window.open(url + "/main.aspx?etn=serviceappointment&pagetype=entityrecord&extraqs=" + encodeURIComponent(extraqs.join("&")), "_blank", features, false);
    }
};

//************************BEGIN APPROVAL METHODS HERE****************************//
//
//This function is used to dynamically build out the Approval flow process into an array which correlates the status, the role name, and the teamId sequentially so that one can get the step and all of the corresponding values
MCS.TSA_Ribbon.buildProcess = function (executionContext) {
    var formContext = executionContext.getFormContext();
    var serviceType = formContext.getAttribute("cvt_servicetype").getValue(); //Get Service Line
    if (serviceType == null) {
        alert("Specialty should always be populated");
        return;
    }
    var counter = 0, patFacilityData = null, proFacilityData = null;
    var provCoSTeam = null, provSCTeam = null, provCPTeam = null, provFTCTeam = null, provNotificationTeam = null, patCoSTeam = null, patSCTeam = null, patFTCTeam = null, patNotificationTeam = null, patCPTeam = null;
    //SD: web-use-strict-equality-operators
    var skipPatient = formContext.getAttribute("cvt_type").getValue() === true || formContext.getAttribute("cvt_servicescope").getValue() === 917290001;
    //SD: web-use-strict-equality-operators
    var skipProvider = formContext.getAttribute("cvt_type").getValue() === true;

    //Draft
    process[0][counter] = 1; //Current Status
    process[1][counter] = "Draft"; //Current Role /Approver
    process[2][counter] = null; //Approval team to reach this stage
    process[3][counter] = null; //workflow to run at this stage
    counter++;

    //Updated Query to Retrieve Team based on Attributes instead of lookups on Facility
    var filter = "(cvt_ServiceType/Id eq (Guid'" + serviceType[0].id + "') or cvt_ServiceType/Id eq null) and cvt_Type ne null and (cvt_Facility/Id eq (Guid'" + ProFacility.Id + "')";
    filter += skipPatient ? ")" : " or cvt_Facility/Id eq (Guid'" + PatFacility.Id + "'))";

    Xrm.WebApi.retrieveMultipleRecords("Team", "?$select=TeamId,cvt_Type,cvt_ServiceType,cvt_Facility&$filter=" + filter).then(
        function success(result) {
            for (var counter in result) {
                var item = result[counter];
                //SD: web-use-strict-equality-operators
                if (ProFacility.Id === item.cvt_Facility.Id) { //ProvFacility
                    switch (item.cvt_Type.Value) {
                        case 917290000: //FTC
                            provFTCTeam = item.TeamId;
                            break;
                        case 917290001: //Service Chief - extra check here for Specialty
                            if (item.cvt_ServiceType != null)
                                provSCTeam = item.TeamId;
                            break;
                        case 917290002: //Chief of Staff
                            provCoSTeam = item.TeamId;
                            break;
                        case 917290004: //Notification Team
                            provNotificationTeam = item.TeamId;
                            break;
                        default:
                            break;
                    }
                }
                else { //PatFacility
                    switch (item.cvt_Type.Value) {
                        case 917290000: //FTC
                            patFTCTeam = item.TeamId;
                            break;
                        case 917290001: //Service Chief - extra check here for Specialty
                            if (item.cvt_ServiceType != null) {
                                patSCTeam = item.TeamId;
                            }
                            break;
                        case 917290002: //Chief of Staff
                            patCoSTeam = item.TeamId;
                            break;
                        case 917290004: //Notification Team
                            patNotificationTeam = item.TeamId;
                            break;
                        default:
                            break;
                    }
                }
            }
        },
        function (error) {
            alert(MCS.cvt_Common.RestError(error));
        }
    );

    //CrmRestKit.ByQueryAll("Team", ['TeamId', 'cvt_Type', 'cvt_ServiceType', 'cvt_Facility'], filter, false)
    //.fail(function(err) {
    //    alert(MCS.cvt_Common.RestError(err));
    //}).done(function(data){
    //    for(var counter in data)
    //    {
    //        var item = data[counter];
    //        if (ProFacility.Id == item.cvt_Facility.Id) { //ProvFacility
    //            switch (item.cvt_Type.Value) {
    //                case 917290000: //FTC
    //                    provFTCTeam = item.TeamId;
    //                    break;
    //                case 917290001: //Service Chief - extra check here for Specialty
    //                    if (item.cvt_ServiceType != null)
    //                        provSCTeam = item.TeamId;
    //                    break;
    //                case 917290002: //Chief of Staff
    //                    provCoSTeam = item.TeamId;
    //                    break;
    //                case 917290004: //Notification Team
    //                    provNotificationTeam = item.TeamId;
    //                    break;
    //                default:
    //                    break;
    //            }
    //        }
    //        else { //PatFacility
    //            switch (item.cvt_Type.Value) {
    //                case 917290000: //FTC
    //                    patFTCTeam = item.TeamId;
    //                    break;
    //                case 917290001: //Service Chief - extra check here for Specialty
    //                    if (item.cvt_ServiceType != null) {
    //                        patSCTeam = item.TeamId;
    //                    }
    //                    break;
    //                case 917290002: //Chief of Staff
    //                    patCoSTeam = item.TeamId;
    //                    break;
    //                case 917290004: //Notification Team
    //                    patNotificationTeam = item.TeamId;
    //                    break;
    //                default:
    //                    break;
    //            }
    //        }
    //    }
    //});

    var missingTeamStart = "The following Teams are missing:";
    var missingTeams = missingTeamStart;
    if (provFTCTeam == null)
        missingTeams += "\n-Provider FTC Team for " + ProFacility.Name;
    if (!skipProvider) {
        if (provSCTeam == null)
            missingTeams += "\n-Provider Service Chief Team for " + ProFacility.Name + " " + serviceType[0].name;
        if (provCoSTeam == null)
            missingTeams += "\n-Provider Chief of Staff Team for " + ProFacility.Name;
    }
    if (!skipPatient) {
        if (patFTCTeam == null)
            missingTeams += "\n-Patient FTC Team for " + PatFacility.Name;
        if (patSCTeam == null)
            missingTeams += "\n-Patient Service Chief Team for " + PatFacility.Name + " " + serviceType[0].name;
        if (patCoSTeam == null)
            missingTeams += "\n-Patient Chief of Staff Team for " + PatFacility.Name;
    }

    //SD: web-use-strict-equality-operators
    if (missingTeams !== missingTeamStart) {
        alert(missingTeams + "\n\nPlease contact your Facility Telehealth Coordinator to get this team set up");
        return false;
    }

    //PatFTC
    if (!skipPatient) {

        process[0][counter] = 917290002;
        process[1][counter] = "Patient Site FTC";
        process[2][counter] = patFTCTeam;
        process[3][counter] = "195750DF-B5DF-4A5B-B3FF-A29C280BC02A";
        counter++;
    }

    //ProFTC
    process[0][counter] = 917290000;
    process[1][counter] = "Provider Site FTC";
    process[2][counter] = provFTCTeam;
    process[3][counter] = "AC75C046-5CE4-4981-BD20-FDD95EEEF641";
    counter++;

    if (!skipProvider) {
        //ProSC
        process[0][counter] = 917290001;
        process[1][counter] = "Provider Site Service Chief";
        process[2][counter] = provSCTeam;
        process[3][counter] = "EB62D141-FA4A-4461-A6CA-E5DA9BE17339";
        counter++;

        //Pro CoS
        process[0][counter] = 917290004;
        process[1][counter] = "Provider Site Chief of Staff";
        process[2][counter] = provCoSTeam;
        process[3][counter] = "BB6BF3C9-41DB-4184-B18F-6A78B8BD2CF9";
        counter++;
    }

    if (!skipPatient) {
        //PatSC
        process[0][counter] = 917290005;
        process[1][counter] = "Patient Site Service Chief";
        process[2][counter] = patSCTeam;
        process[3][counter] = "8423B567-1FAB-461D-ACB6-59CEDDA7012A";
        counter++;

        //Pat CoS
        process[0][counter] = 251920000;
        process[1][counter] = "Patient Site Chief of Staff";
        process[2][counter] = patCoSTeam;
        process[3][counter] = null;
        counter++;
    }
    else {
        process[0][counter - 1] = 251920000;
        process[3][counter - 1] = null;
    }
};

//recursively calls itself until either of 2 conditions are met: 1) the person approving is found on a prior team, which returns true and alerts user, 2) user is not found on any prior teams, so return false
MCS.TSA_Ribbon.checkPriorTeams = function (statuscode) {
    var userSettings = Xrm.Utility.getGlobalContext().userSettings;
    var userId = userSettings.userId;
    var approvalLevel = process[0].indexOf(statuscode);
    //SD: web-use-strict-equality-operators
    if (approvalLevel === 0)
        return false;

    var previousTeam = MCS.TSA_Ribbon.checkTeamMembership(process[2][approvalLevel], userId);
    if (previousTeam) {
        alert("The " + process[1][approvalLevel] + " Team (your team) has already approved this TSA.  Please see the notes to view the previous approvals.");
        return true;
    }
    else {
        MCS.TSA_Ribbon.checkPriorTeams(process[0][approvalLevel - 1]);
    }
};

//This method accepts parameter for status and it checks that the user who is hitting approve is authorized to do so (based on team membership of specified team, or if that is not populated, then if the user is specified as the correct person - the CoS, SC, FTC, or C&P)
MCS.TSA_Ribbon.checkApprover = function (status) {
    var userSettings = Xrm.Utility.getGlobalContext().userSettings;
    var userId = userSettings.userId;
    var IsApprover = false;
    var step = process[0].indexOf(status);
    IsApprover = MCS.TSA_Ribbon.checkTeamMembership(process[2][step + 1], userId);
    if (!IsApprover) {
        if (!MCS.TSA_Ribbon.checkPriorTeams(status)) {
            var approvalMessage = "Only members of the " + process[1][step + 1] + " Approval Group can perform this approval";
            alert(approvalMessage);
        }
    }
    return IsApprover;
};

//This method accepts parameter for status and it checks that the user who is hitting approve is authorized to do so (based on team membership of specified team, or if that is not populated, then if the user is specified as the correct person - the CoS, SC, FTC, or C&P)
MCS.TSA_Ribbon.checkDeny = function (status) {
    var userSettings = Xrm.Utility.getGlobalContext().userSettings;
    var userId = userSettings.userId;
    var CanDeny = false;
    var step = process[0].indexOf(status);
    CanDeny = MCS.TSA_Ribbon.checkTeamMembership(process[2][step + 1], userId);
    if (!CanDeny) {
        if (!MCS.TSA_Ribbon.checkPriorTeams(status)) {
            var denyMessage = "Only members of the " + process[1][step + 1] + " Approval Group can perform this denial";
            alert(denyMessage);
        }
    }
    return CanDeny;
};

//Populates the patient and provider facilities for use across a number of other functions
//TO-DO skip populating the patient facility or site for VA Video Connect
MCS.TSA_Ribbon.getFacilities = function (executionContext) {
    var formContext = executionContext.getFormContext();
    var provSiteId = formContext.getAttribute("cvt_relatedprovidersiteid").getValue()[0].id;
    var provFacility = MCS.mcs_TSA_OnChange.QueryFacility(provSiteId);
    //SD: web-use-strict-equality-operators
    var group = formContext.getAttribute("cvt_groupappointment").getValue() === 1;

    //Set Global Variable as result of this function
    ProFacility = provFacility;

    //This is the check for if patient facility (or site) is null (depending on group or not), then dont run this function
    if (group && formContext.getAttribute("cvt_patientfacility").getValue() == null
        || !group && formContext.getAttribute("cvt_relatedpatientsiteid").getValue() == null)
        return;
    var patFacilityObj = group ? formContext.getAttribute("cvt_patientfacility").getValue()[0] :
        MCS.mcs_TSA_OnChange.QueryFacility(formContext.getAttribute("cvt_relatedpatientsiteid").getValue()[0].id);

    //Populate facility object whether it is via OData query (requiring Caps) or directly from form (all lowercase) and pass that object through as if it is OData
    var patFacility =
    {
        Id: patFacilityObj.Id == null ? patFacilityObj.id : patFacilityObj.Id,
        Name: patFacilityObj.Name == null ? patFacilityObj.name : patFacilityObj.Name
    };
    //Populate the patFacility global variable appropriately
    PatFacility = patFacility;
};

//This method Queries via OData to check if the user specified is a member of the team specified.  It returns true if so
MCS.TSA_Ribbon.checkTeamMembership = function (teamid, userId) {
    var teamFound = false;
    if (teamid == null)
        return false;
    var filter = "SystemUserId eq (Guid' " + userId + "') and TeamId eq (Guid' " + teamid + "')";
    //Query for any team membership records where the team ID equals the team listed and the userId of logged in user matches the TeamMemberShip UserId
    Xrm.WebApi.retrieveMultipleRecords("TeamMembership", "?$select=TeamId&$filter=" + filter).then(
        function success(result) {
            for (var i = 0; i < result.entities.length; i++) {
                teamFound = result.entities.length > 0;
            }
            console.log("Next page link: " + result.nextLink);
            // perform additional operations on retrieved records
        },
        function (error) {
            //alert("ERROR: " + error);
        }
    );
    //var query = CrmRestKit.ByQuery("TeamMembership", ['TeamId'], filter, false);
    //query.fail(function (err) {
    //    //alert("ERROR: " + err);
    //}).done(function (data) {
    //    if (data && data.d.results)
    //        teamFound = data.d.results.length > 0;
    //});
    return teamFound;
};

//This method is used to begin the TSA approval process (when you hit begin collecting signatures)
MCS.TSA_Ribbon.runPatApprovalWorkflow = function (executionContext) {
    var formContext = executionContext.getFormContext();
    var status = formContext.getAttribute("statuscode");
    //SD: web-use-strict-equality-operators
    if (status.getValue() !== 1) //If the TSA is not in draft, you can't run this "Begin Collecting Signatures"
        return;

    MCS.TSA_Ribbon.runApproveTSA();
};

//This function is called every time the "Approve" Button is hit: it checks for the current status, decides what the next status should be, and calls a check that the user approving has permission to do so
MCS.TSA_Ribbon.runApproveTSA = function (executionContext) {
    var formContext = executionContext.getFormContext();
    MCS.TSA_Ribbon.getFacilities();
    //SD: web-use-strict-equality-operators
    if (MCS.TSA_Ribbon.buildProcess() === false)
        return;
    var status = formContext.getAttribute("statuscode");

    //Status Listing: 917290002==Approved by Pat FTC, 917290000==Prov FTC, 917290001==Prov SC, 917290004==Prov CoS, 917290005==Pat SC, 917290006==Pending Privileging
    var step = process[0].indexOf(status.getValue());
    //SD: web-use-strict-equality-operators
    if (step === -1)
        return;

    var action = process[1][step + 1];
    var passedCheck = MCS.TSA_Ribbon.checkApprover(status.getValue());
    var confirmResult;
    if (!passedCheck)
        return;
    //SD: web-use-strict-equality-operators
    if (typeof process[1][step + 2] === "undefined")
        confirmResult = confirm("Click OK to Approve this TSA and put it into Production");
    else
        confirmResult = confirm("Click OK to Approve this TSA and automatically route it to the " + process[1][step + 2] + " Approval Group");
    if (confirmResult) {
        var tsa = new Object();
        //If they confirm they are approving, move the TSA to the next status in the process via OData, also set the field for user feedback
        MCS.TSA_Ribbon.runRibbonWorkflow(process[3][step + 1]);
        tsa.statuscode = { Value: process[0][step + 1] };
        status.setValue(process[0][step + 1]);
        MCS.TSA_Ribbon.CreateNote("Approved", null, "Approved by " + action);
        Xrm.WebApi.updateRecord("mcs_services", formContext.data.entity.getId(), tsa).then(
            function success() {
                formContext.ui.controls.get("statuscode").setDisabled(true);
            },
            function (error) {
                alert(MCS.cvt_Common.RestError(error));
            }
        );
        //CrmRestKit.Update('mcs_services', Xrm.Page.data.entity.getId(), tsa, true).
        //    fail(function (err) {
        //        alert(MCS.cvt_Common.RestError(err));
        //    }).
        //    done();
        //formContext.ui.controls.get("statuscode").setDisabled(true);
    }
    else
        alert("This TSA will remain in your queue until you approve or deny it.");
};

MCS.TSA_Ribbon.CreateNote = function (executionContext, action, reason, newStatus) {
    var name = "";
    var formContext = executionContext.getFormContext();
    var userSettings = Xrm.Utility.getGlobalContext().userSettings;
    var userId = userSettings.userId;
    Xrm.WebApi.retrieveRecord("SystemUser", userId, "?$select=FullName").then(
        function success(result) {
            name = result.FullName;
        },
        function (error) {
        }
    );
    //CrmRestKit.Retrieve("SystemUser", Xrm.Page.context.getUserId(), ['FullName'], false).fail().done(function (data) { name = data.d.FullName; });
    var refTSA = new Object();
    refTSA.LogicalName = "mcs_services";
    refTSA.Id = formContext.data.entity.getId();
    var noteText = "This TSA has been " + action;
    if (reason != null)
        noteText += " because: " + reason;
    noteText += ". New Status = " + newStatus;
    var note = {
        'Subject': action + " by " + name, 'ObjectId': refTSA, 'NoteText': noteText
    };
    Xrm.WebApi.createRecord("Annotation", note).then(
        function success(result) {
        },
        function (error) {
            alert(error.responseText);
        }
    );
    //CrmRestKit.Create("Annotation", note, true).fail(function (err) { alert(err.responseText); }).done();
};

//TO-DO switch to a dialog instead of "prompt"
//This method is used to Deny the TSA and is called whenever the "Deny" button is hit regardless of current status of TSA
MCS.TSA_Ribbon.DenyTSA = function (executionContext) {
    var formContext = executionContext.getFormContext();
    MCS.TSA_Ribbon.getFacilities();
    var status = formContext.getAttribute("statuscode");
    //Status Listing: 917290002==Approved by Pat FTC, 917290000==Prov FTC, 917290001==Prov SC, 917290004==Prov CoS, 917290005==Pat SC
    //SD: web-use-strict-equality-operators
    if (MCS.TSA_Ribbon.buildProcess() === false)
        return;

    var CanDeny = MCS.TSA_Ribbon.checkDeny(status.getValue());
    if (CanDeny) {
        var confirmResult = prompt("Enter Denial Reason and Click OK to Deny this TSA");
        if (confirmResult != null) {
            //set status to Denied and create a note
            MCS.TSA_Ribbon.runRibbonWorkflow("cf2bc300-a19f-440d-9055-dc8a9a55e102");
            status.setValue(917290003);
            MCS.TSA_Ribbon.CreateNote("Denied", confirmResult, "Denied");
            formContext.ui.controls.get("statuscode").setDisabled(true);
        }
        else {
            alert("This TSA will remain in your queue until you approve or deny it.");
        }
    }
};

//To move to Common
MCS.TSA_Ribbon.runRibbonWorkflow = function (workflowId) {
    var userSettings = Xrm.Utility.getGlobalContext().userSettings;
    var userId = userSettings.userId;
    //To move to Common
    MCS.GlobalFunctions.runWorkflow(userId, workflowId, MCS.GlobalFunctions.runWorkflowResponse);
};