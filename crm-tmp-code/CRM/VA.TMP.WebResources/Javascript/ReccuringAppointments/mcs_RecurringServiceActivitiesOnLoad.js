//If the SDK namespace object is not defined, create it.
if (typeof (MCS) == "undefined")
{ MCS = {}; }
// Create Namespace container for functions in this library;
MCS.mcs_RecurringAppts_OnLoad = {};

MCS.mcs_RecurringAppts_OnLoad.FORM_TYPE_CREATE = 1;
MCS.mcs_RecurringAppts_OnLoad.FORM_TYPE_UPDATE = 2;
MCS.mcs_RecurringAppts_OnLoad.FORM_TYPE_READ_ONLY = 3;
MCS.mcs_RecurringAppts_OnLoad.FORM_TYPE_DISABLED = 4;

MCS.mcs_RecurringAppts_OnLoad.LoadScheduledResources = function (executionContext) {
    /***********************************************************************
    /** 
    /** Description: Grabs the scheduled Resources from the related Service Activities form. 
    /** 
    ***********************************************************************/
    var formContext = executionContext.getFormContext();
    if ((formContext.getAttribute("cvt_serviceactivityid").getValue() != null) && (formContext.getAttribute("requiredattendees").getValue() == null)) {

        var relatedServiceActivity = formContext.getAttribute("cvt_serviceactivityid").getValue();
        var relatedServiceActivityId = relatedServiceActivity[0].id;

        var retrievemcs_relatedSAResources = new XMLHttpRequest();
        var request = MCS.GlobalFunctions._getClientUrl("ODATA") + "/ServiceAppointmentSet(guid'" + relatedServiceActivityId + "')?$select=serviceappointment_activity_parties/ParticipationTypeMask,serviceappointment_activity_parties/PartyId&$expand=serviceappointment_activity_parties";


        retrievemcs_relatedSAResources.open("GET", request, true);
        retrievemcs_relatedSAResources.setRequestHeader("Accept", "application/json");
        retrievemcs_relatedSAResources.setRequestHeader("Content-Type", "application/json; charset=utf-8");
        retrievemcs_relatedSAResources.onreadystatechange = function () { MCS.mcs_RecurringAppts_OnLoad.retrievemcs_relatedSAResourcesCallBack(this); };
        retrievemcs_relatedSAResources.send();
    }
};

MCS.mcs_RecurringAppts_OnLoad.retrievemcs_relatedSAResourcesCallBack = function (executionContext,retrievemcs_relatedSAResources) {
    var formContext = executionContext.getFormContext();
        if (retrievemcs_relatedSAResources.readyState == 4 /* complete */) {
            if (retrievemcs_relatedSAResources.status == 200) {
                //Success
                var retrievemcs_relatedResources = window.JSON.parse(retrievemcs_relatedSAResources.responseText).d;
                if (retrievemcs_relatedResources.serviceappointment_activity_parties != null) {
                    //check to see if the control is on the form before you try to setvalue on it
                    if (formContext.getAttribute("requiredattendees") != null) {
                        if (retrievemcs_relatedResources.serviceappointment_activity_parties.results != null)
                        {
                            var resources = retrievemcs_relatedResources.serviceappointment_activity_parties.results;

                            var resourcesList = new Array();

                            for (var i = 0; i < resources.length; i++) {

                                if (resources[i].ParticipationTypeMask.Value == 10) {
                                    resourcesList[i] = new Object();
                                    resourcesList[i].id = resources[i].PartyId.Id; //Guid (i.e., Guid of Resource)
                                    resourcesList[i].name = resources[i].PartyId.Name; //Name (i.e., Name Resource)
                                    resourcesList[i].entityType = resources[i].PartyId.LogicalName; //entity schema name
                                }
                            }

                            formContext.getAttribute("requiredattendees").setValue(resourcesList);
                        }
                        else
                        {
                            formContext.getAttribute("requiredattendees").setValue(null);
                        }
                    }       
                } else {
                    alert('No records were returned from the copy function, please contact your administrator');
                }
            }
            else
            {
                //Failure
               // MCS.GlobalFunctions.errorHandler(retrievemcs_relatedSAResources);
            }
        }
    };