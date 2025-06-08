//If the SDK namespace object is not defined, create it.
if (typeof (MCS) === "undefined") { MCS = {}; }
// Create Namespace container for functions in this library;
MCS.PPEFeedback = {};

MCS.PPEFeedback.OnLoad = function (executionContext) {
    var formContext = executionContext.getFormContext();
    if (formContext.getAttribute("cvt_anythingtoreport").getValue() !== null) {
        formContext.getControl("cvt_anythingtoreport").setDisabled(true);
        //Ribbon Notification, has been submitted
        formContext.ui.setFormNotification('PPE record has already been submitted', 'INFORMATION');
    }
    //Check if Requested Grid is populated
    if (formContext.getAttribute("cvt_responserequested").getValue() !== null) {
        //Populate Request Grid from Team Name
        MCS.PPEFeedback.GetRelatedTeamMembers(executionContext, "cvt_responserequested", "requested_view");
    }

    //Check if Escalation Team is populated
    if (formContext.getAttribute("cvt_responseescalated").getValue() !== null) {
        //Show Section
        formContext.ui.tabs.get('generalTab').sections.get('escalationSection').setVisible(true);

        //Edit subgrid
        MCS.PPEFeedback.GetRelatedTeamMembers(executionContext, "cvt_responseescalated", "escalated_view");

        //Display Escalation message
        if (formContext.getAttribute("cvt_anythingtoreport").getValue() !== null)
            formContext.ui.setFormNotification('PPE record had been escalated', 'WARNING');
        else
            formContext.ui.setFormNotification('PPE record is overdue and has been escalated', 'WARNING');
    }
};

//Set grid which will retrieve all Team Members Related to this Team.  
MCS.PPEFeedback.GetRelatedTeamMembers = function (executionContext, teamField, subgrid) {
    var formContext = executionContext.getFormContext();
    if (formContext.ui.getFormType() !== MCS.cvt_Common.FORM_TYPE_CREATE) {
        var Grid = document.getElementById(subgrid);

        if (Grid === null) { //make sure the grids have loaded 
            setTimeout(function () { MCS.PPEFeedback.GetRelatedTeamMembers(teamField, subgrid); }, 500); //if the grid hasn�t loaded run this again when it has 
            return;
        }

        var ThisTeam = formContext.getAttribute(teamField).getValue();
        var ThisTeamName = (ThisTeam[0] != null) ? ThisTeam[0].name : "";
        var ThisTeamId = (ThisTeam[0] != null) ? ThisTeam[0].id : MCS.cvt_Common.BlankGUID;

        //set the fetch xml to the sub grid   
        Grid.control.SetParameter("fetchXml", MCS.PPEFeedback.GetFetchXML(ThisTeamName, ThisTeamId));
        Grid.control.Refresh();
    }
};

//Get correct FetchXML - for grids
MCS.PPEFeedback.GetFetchXML = function (ThisTeamName, ThisTeamId) {
    var FetchXML =
        "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>" +
        "<entity name='systemuser'>" +
        "<attribute name='fullname' />" +
        "<attribute name='cvt_type' />" +
        "<attribute name='domainname' />" +
        "<attribute name='internalemailaddress' />" +
        "<attribute name='jobtitle' />" +
        "<attribute name='cvt_site' />" +
        "<attribute name='cvt_facility' />" +
        "<attribute name='mcs_visn' />" +
        "<attribute name='systemuserid' />" +

        "<order attribute='fullname' descending='false' />" +
        "<link-entity name='teammembership' from='systemuserid' to='systemuserid' visible='false' intersect='true'>" +
        "<link-entity name='team' from='teamid' to='teamid' alias='ab'>" +

        "<filter type='and'>" +
        "<condition attribute='teamid' operator='eq' uiname='" + MCS.cvt_Common.formatXML(ThisTeamName) + "' uitype='team' value='" + ThisTeamId + "' />" +
        "</filter>" +
        "</link-entity>" +
        "</link-entity>" +
        "</entity>" +
        "</fetch>";

    return FetchXML;
};

MCS.PPEFeedback.SaveAndClose = function (primaryControl) {
    var formContext = primaryControl.getFormContext();
    formContext.data.entity.save("saveandclose");
};