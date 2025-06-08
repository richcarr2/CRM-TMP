//If the SDK namespace object is not defined, create it.
if (typeof MCS == "undefined")
    MCS = {};
// Create Namespace container for functions in this library;
if (typeof MCS.mcs_Service_Activity_OnLoad == "undefined")
    MCS.mcs_Service_Activity_OnLoad = {};

MCS.mcs_Service_Activity_OnLoad.FORM_TYPE_CREATE = 1;
MCS.mcs_Service_Activity_OnLoad.FORM_TYPE_UPDATE = 2;
MCS.mcs_Service_Activity_OnLoad.FORM_TYPE_READ_ONLY = 3;
MCS.mcs_Service_Activity_OnLoad.FORM_TYPE_DISABLED = 4;

//Global Variables
var totalPatients = new Array();
var totalGroupPatients = new Array();
MCS.Patients = [];

//This onLoad function is the master function that coordinates the actual scripts that run
MCS.mcs_Service_Activity_OnLoad.OnLoad = function (executionContext) {
    var formContext = executionContext.getFormContext();
    //Add Scripting events to fields for when fields change
    formContext.getAttribute("cvt_type").addOnChange(MCS.mcs_Service_Activity.CVTtoHome);
    formContext.getAttribute("mcs_groupappointment").addOnChange(MCS.mcs_Service_Activity.GroupAppt);
    formContext.getAttribute("mcs_groupappointment").addOnChange(MCS.mcs_Service_Activity.EnableSchedulingPackage);
    formContext.getAttribute("mcs_groupappointment").addOnChange(MCS.mcs_Service_Activity.HandleOnChangeSchedulingPackageLookup);
    formContext.getAttribute("mcs_groupappointment").addOnChange(MCS.mcs_Service_Activity.CreateName);
    formContext.getAttribute("cvt_telehealthmodality").addOnChange(MCS.mcs_Service_Activity.HandleOnChangeSchedulingPackageLookup);
    formContext.getAttribute("mcs_relatedsite").addOnChange(MCS.mcs_Service_Activity.EnableSchedulingPackage);
    formContext.getAttribute("mcs_relatedsite").addOnChange(MCS.mcs_Service_Activity.HandleOnChangeSchedulingPackageLookup);
    formContext.getAttribute("mcs_relatedprovidersite").addOnChange(MCS.mcs_Service_Activity.EnableSchedulingPackage);
    formContext.getAttribute("mcs_relatedprovidersite").addOnChange(MCS.mcs_Service_Activity.HandleOnChangeSchedulingPackageLookup);
    formContext.getAttribute("mcs_servicetype").addOnChange(MCS.mcs_Service_Activity.EnableServiceSubType);
    formContext.getAttribute("mcs_servicetype").addOnChange(MCS.mcs_Service_Activity.HandleOnChangeSchedulingPackageLookup);
    formContext.getAttribute("mcs_servicetype").addOnChange(MCS.mcs_Service_Activity.CreateName);
    formContext.getAttribute("mcs_servicesubtype").addOnChange(MCS.mcs_Service_Activity.EnableSchedulingPackage);
    formContext.getAttribute("mcs_servicesubtype").addOnChange(MCS.mcs_Service_Activity.HandleOnChangeSchedulingPackageLookup);
    formContext.getAttribute("mcs_servicesubtype").addOnChange(MCS.mcs_Service_Activity.CreateName);
    formContext.getAttribute("cvt_relatedproviderid").addOnChange(MCS.mcs_Service_Activity.HandleOnChangeSchedulingPackageLookup);
    formContext.getAttribute("cvt_relatedschedulingpackage").addOnChange(MCS.mcs_Service_Activity.GetSchedulingPackageData);
    formContext.getAttribute("cvt_relatedschedulingpackage").addOnChange(MCS.mcs_Service_Activity.HandleOnChangeSchedulingPackageLookup);
    formContext.getAttribute("cvt_relatedschedulingpackage").addOnChange(MCS.mcs_Service_Activity.ClearResources);
    formContext.getAttribute("cvt_relatedschedulingpackage").addOnChange(MCS.mcs_Service_Activity.filterSubGrid);
    formContext.getAttribute("resources").addOnChange(MCS.mcs_Service_Activity.GetProviderSite);
    formContext.getAttribute("customers").addOnChange(MCS.mcs_Service_Activity.BlockAddPatient);
    formContext.getAttribute("cvt_patientsiteresourcesrequired").addOnChange(MCS.mcs_Service_Activity.HandlePatientSiteResourcesRequiredChange);
    formContext.getAttribute("cvt_telephonecall").addOnChange(MCS.mcs_Service_Activity_OnLoad.PhoneModalityDisplayCheck);
    formContext.getAttribute("mcs_groupappointment").addOnChange(MCS.mcs_Service_Activity_OnLoad.PhoneModalityDisplayCheck);
    formContext.getAttribute("cvt_type").addOnChange(MCS.mcs_Service_Activity_OnLoad.PhoneCallCheck);
    formContext.getAttribute("cvt_type").addOnChange(MCS.mcs_Service_Activity_OnLoad.PhoneModalityDisplayCheck);
    formContext.getAttribute("cvt_telephonecall").addOnChange(MCS.mcs_Service_Activity_OnLoad.ToggleGroupTelephone);
    formContext.getAttribute("mcs_groupappointment").addOnChange(MCS.mcs_Service_Activity_OnLoad.ToggleGroupTelephone);

    //Run the following functions when the form loads
    MCS.mcs_Service_Activity.EnableServiceSubType(executionContext);
    MCS.mcs_Service_Activity.EnableSchedulingPackage(executionContext);
    MCS.mcs_Service_Activity.GroupAppt(executionContext);
    MCS.mcs_Service_Activity_OnLoad.SetDefaultDateTime(executionContext);
    MCS.mcs_Service_Activity.GetSchedulingPackageData(executionContext);
    MCS.mcs_Service_Activity.HandleOnChangeSchedulingPackageLookup(executionContext);
    MCS.mcs_Service_Activity.CVTtoHome(executionContext);
    MCS.mcs_Service_Activity_OnLoad.ShowHideCancelRemarks(executionContext);
    MCS.mcs_Service_Activity_OnLoad.RemoveNotification(executionContext);
    MCS.mcs_Service_Activity_OnLoad.ShowMVI(executionContext);
    MCS.mcs_Service_Activity_OnLoad.LoadPatients(executionContext);
    MCS.mcs_Service_Activity_OnLoad.ResetPatients(executionContext);
    MCS.mcs_Service_Activity_OnLoad.ValidatePreselectedPatient(executionContext);
    MCS.mcs_Service_Activity.HandlePatientSiteResourcesRequiredChange(executionContext);
    MCS.mcs_Service_Activity_OnLoad.PhoneModality(executionContext);


    formContext.getAttribute("cvt_patuserduz").setValue(null);
    formContext.getAttribute("cvt_prouserduz").setValue(null);
    formContext.getAttribute("cvt_samltoken").setValue(null);

    formContext.getAttribute("cvt_patuserduz").setSubmitMode("never");
    formContext.getAttribute("cvt_prouserduz").setSubmitMode("never");
    formContext.getAttribute("cvt_samltoken").setSubmitMode("never");
};


//If the Service is not loaded, default the start and end time to 8:30-9:30
MCS.mcs_Service_Activity_OnLoad.SetDefaultDateTime = function (executionContext) {
    var formContext = executionContext.getFormContext();
    //Set Default Datetime if the Scheduling Package Service has not been loaded yet. 
    if (formContext.getAttribute("serviceid").getValue() == null) {
        MCS.cvt_Common.DateTime(executionContext,'scheduledstart', 8, 30);
        MCS.cvt_Common.DateTime(executionContext,'scheduledend', 9, 30);
        formContext.getAttribute("scheduleddurationminutes").setValue(60);
    }
};

//unsupported modification to clear out the incorrect notification that the resources do not match the service rules listed (they are in the appointments per design)
MCS.mcs_Service_Activity_OnLoad.RemoveNotification = function (executionContext) {
    var formContext = executionContext.getFormContext();
    //Group and Update Form
    if (formContext.getAttribute("mcs_groupappointment").getValue() == true && formContext.ui.getFormType() != 1) {
        var notificationsList = Sys.Application.findComponent('crmNotifications');
        if (notificationsList) {
            //Hide message
            notificationsList.SetVisible(false);
        }
    }
};
MCS.mcs_Service_Activity_OnLoad.PhoneModality = function (executionContext) {
    var formContext = executionContext.getFormContext();
    if (formContext.ui.getFormType() == 1) {
        //initially hide all the Telephone Modality Fields
        //.setRequiredLevel("required")
        formContext.getControl("cvt_telephonecall").setVisible(false);
        formContext.getControl("cvt_patientverifiedphone").setVisible(false);
        formContext.getAttribute("cvt_patientverifiedphone").setRequiredLevel("none");
        formContext.getControl("cvt_patientverifiedemail").setVisible(false);
        formContext.getControl("cvt_mobilephone").setVisible(false);
        formContext.getControl("cvt_businessphone").setVisible(false);
        formContext.getControl("cvt_homephone").setVisible(false);
        formContext.getControl("cvt_technologytype").setVisible(false);
        formContext.getControl("cvt_donotallowemails").setVisible(false);
        formContext.getControl("cvt_email").setVisible(false);
    }
};

MCS.mcs_Service_Activity_OnLoad.ToggleGroupTelephone = function (executionContext) {
    var formContext = executionContext.getFormContext();
    var phonecall = formContext.getAttribute("cvt_telephonecall").getValue();
    var group = formContext.getAttribute("mcs_groupappointment").getValue();
    var type = formContext.getAttribute("cvt_type").getValue();

    //clear and hide the 'group' control if 'Telephone Appointment' is selected
    //unhide if 'Telephone Appointment' is deselected
    if (phonecall == true) {
        formContext.getAttribute("mcs_groupappointment").setValue(false);
        formContext.getControl("mcs_groupappointment").setVisible(false);
    }
    else {
        formContext.getControl("mcs_groupappointment").setVisible(true);
    }

    //clear and hide the 'Telephone Appointment' control if 'Group'  or 'CVT' is selected
    //unhide if 'Group' is deselected
    if ((group == true) || (type == false)) {
        formContext.getAttribute("cvt_telephonecall").setValue(false);
        formContext.getAttribute("cvt_patientverifiedphone").setRequiredLevel("none");
        formContext.getControl("cvt_telephonecall").setVisible(false);
    }
    else {
        formContext.getControl("cvt_telephonecall").setVisible(true);
    }
};

MCS.mcs_Service_Activity_OnLoad.PhoneCallCheck = function (executionContext) {
    var formContext = executionContext.getFormContext();
    var phonecall = formContext.getAttribute("cvt_telephonecall").getValue();
    var type = formContext.getAttribute("cvt_type").getValue();
    if (type == false) {
        formContext.getAttribute("cvt_telephonecall").setValue(false);
        formContext.getControl("cvt_telephonecall").setVisible(false);
    }
    else {
        formContext.getControl("cvt_telephonecall").setVisible(true);
    }
};

MCS.mcs_Service_Activity_OnLoad.PhoneModalityDisplayCheck = function (executionContext) {
    var formContext = executionContext.getFormContext();
    var phonecall = formContext.getAttribute("cvt_telephonecall").getValue();
    var group = formContext.getAttribute("mcs_groupappointment").getValue();
    var type = formContext.getAttribute("cvt_type").getValue();


    if ((phonecall == 1) && (group == 0) && (type == 1)) //  must be a non-group VVC *phone call*
    {

        formContext.getControl("cvt_patientverifiedphone").setVisible(true);
        formContext.getAttribute("cvt_patientverifiedphone").setRequiredLevel("required");
        formContext.getControl("cvt_patientverifiedemail").setVisible(true);
        formContext.getControl("cvt_mobilephone").setVisible(true);
        formContext.getControl("cvt_businessphone").setVisible(true);
        formContext.getControl("cvt_homephone").setVisible(true);
        formContext.getControl("cvt_technologytype").setVisible(true);
        formContext.getControl("cvt_donotallowemails").setVisible(true);
        formContext.getControl("cvt_email").setVisible(true);
        MCS.mcs_Service_Activity_OnLoad.PopulatePhoneModalityFields(executionContext);

        //now disable the fields
        formContext.getControl("cvt_mobilephone").setDisabled(true);
        formContext.getControl("cvt_businessphone").setDisabled(true);
        formContext.getControl("cvt_homephone").setDisabled(true);
        formContext.getControl("cvt_technologytype").setDisabled(true);
        formContext.getControl("cvt_donotallowemails").setDisabled(true);
        formContext.getControl("cvt_email").setDisabled(true);
    }
    else {
        formContext.getAttribute("cvt_mobilephone").setValue(null);
        formContext.getAttribute("cvt_businessphone").setValue(null);
        formContext.getAttribute("cvt_homephone").setValue(null);
        formContext.getAttribute("cvt_technologytype").setValue(null);
        formContext.getAttribute("cvt_donotallowemails").setValue(false);
        formContext.getAttribute("cvt_email").setValue(null);
        //hide all the Telephone Modality Fields
        formContext.getAttribute("cvt_patientverifiedphone").setRequiredLevel("none");
        formContext.getControl("cvt_patientverifiedphone").setVisible(false);
        formContext.getControl("cvt_patientverifiedemail").setVisible(false);
        formContext.getControl("cvt_mobilephone").setVisible(false);
        formContext.getControl("cvt_businessphone").setVisible(false);
        formContext.getControl("cvt_homephone").setVisible(false);
        formContext.getControl("cvt_technologytype").setVisible(false);
        formContext.getControl("cvt_donotallowemails").setVisible(false);
        formContext.getControl("cvt_email").setVisible(false);
    }
};

MCS.mcs_Service_Activity_OnLoad.PopulatePhoneModalityFields = function (executionContext) {
    var formContext = executionContext.getFormContext();
    var _patient = formContext.getAttribute("customers").getValue();
    if (_patient != null) {
        var patient = _patient[0].id;

        //if we have a patient we can populate the corresponding contact fields
        Xrm.WebApi.retrieveRecord('contact', patient, "?$select=contactid,mobilephone,telephone1,telephone2,cvt_tablettype,donotemail,emailaddress1").then(
            function success(result) {
                console.log("successfully retrieved Patient");
                if (result.mobilephone != null) {
                    formContext.getAttribute("cvt_mobilephone").setValue(result.mobilephone);
                }
                if (result.telephone1 != null) {
                    formContext.getAttribute("cvt_businessphone").setValue(result.telephone1);
                }
                if (result.telephone2 != null) {
                    formContext.getAttribute("cvt_homephone").setValue(result.telephone2);
                }
                if (result.cvt_tablettype != null) {
                    var tablettype = "";
                    switch (result.cvt_tablettype) {
                        case 917290002:
                            tablettype = "VA Issued Device";
                            break;
                        case 917290003:
                            tablettype = "Personal Device";
                            break;
                        default:
                            tablettype = "";
                            break;
                    }
                    formContext.getAttribute("cvt_technologytype").setValue(tablettype);
                }
                if (result.donotemail != null) {
                    var donotsend = false;

                    if (result.donotemail == "false") {
                        donotsend = false;
                    } else {
                        donotsend = true;
                    }

                    formContext.getAttribute("cvt_donotallowemails").setValue(donotsend);
                }
                if (result.emailaddress1 != null) {
                    formContext.getAttribute("cvt_email").setValue(result.emailaddress1);
                }

            },
            function (error) {
                console.log("no Patient retrieved");
            }
        )
    }
};

//Read the configuration value from Active Settings to determine whether or not to display the Patient Search (MVI) iFrame as well as the patients field
MCS.mcs_Service_Activity_OnLoad.ShowMVI = function (executionContext) {
    //debugger;
    var formContext = executionContext.getFormContext();
    var patObj = formContext.getAttribute("customers");
    var patientExists = patObj.getValue() != null;
    var mviTab = formContext.ui.tabs.get("tab_7");
    //var showMVI = MCS.cvt_Common.MVIConfig();
    var showMVIretrieveTokenDeferred = MCS.cvt_Common.MVIConfig();
    $.when(showMVIretrieveTokenDeferred).done(function (returnData) {
        //debugger;
        if (returnData.success == true) {
            var showMVI = returnData.data.result;
            showMVI = showMVI && !MCS.cvt_Common.AppointmentOccursInPast(executionContext);
            mviTab.setVisible(showMVI);
            formContext.getControl("customers").setDisabled(!showMVI);
            if (showMVI && !patientExists)
                mviTab.setFocus();
            else
                formContext.getControl("cvt_type").setFocus();
        }
    });
    if (patientExists) {
        mviTab.setDisplayState("collapsed");
    }
};

MCS.mcs_Service_Activity_OnLoad.LoadPatients = function (executionContext) {
    var formContext = executionContext.getFormContext();
    var patientObj = formContext.getAttribute("customers");
    var patients = patientObj != null ? patientObj.getValue() : [];
    MCS.Patients = patients;
};

//This function is required in order to ensure the "Patients" Activity Party List does not show empty/duplicate APs - existing CRM bug
MCS.mcs_Service_Activity_OnLoad.ResetPatients = function (executionContext) {
    var formContext = executionContext.getFormContext();
    var cust = formContext.getAttribute("customers").getValue();
    formContext.getAttribute("customers").setValue(cust);
};

MCS.mcs_Service_Activity_OnLoad.ValidatePreselectedPatient = function (executionContext) {
    //Perform the Patient Validation when the appointment is launched from the patient record/view
    //When the schedule appointment button is clicked from patient record/view, the patient field is populated in the new appointment screen/form, 
    //Hence the selected patient need to be validated on load if the patient is pre - selected[Form type Create only]
    var formContext = executionContext.getFormContext();
    if (formContext.ui.getFormType() === MCS.mcs_Service_Activity_OnLoad.FORM_TYPE_CREATE) {
        var veteranAttribute = formContext.getAttribute("customers");

        if (veteranAttribute !== null) {
            var veterans = veteranAttribute.getValue();
            if (veterans !== null && veterans.length > 0) {
                //Note: The user do not have the option to select more than one patient from the patient view to initiate schedule appointment. 
                //Hence we should have only one patient in this situation or none in case the appointment is created outside of patient view
                if (veterans[0].entityType === "contact")
                    Xrm.WebApi.retrieveRecord('contact', veterans[0].id, "?$select=*").then(
                        function success(data) {
                            var isValid = ValidateRequiredPatientDetails(data);

                            if (!isValid) { //If not valid, clear the Patient field value
                                veteranAttribute.setValue();
                            }
                        },
                        function (error) {
                            alert(error.message);
                        }
                    );
            }
        }
    }
};

MCS.mcs_Service_Activity_OnLoad.ShowHideCancelRemarks = function (executionContext) {
    var formContext = executionContext.getFormContext();
    var _state = formContext.getAttribute("statecode").getValue();
    var _cancelComments = formContext.getAttribute("cvt_cancelremarks").getValue();
    var _apptType = formContext.getAttribute("cvt_type").getValue();
    formContext.getControl("cvt_providerstationcode").setVisible(false);

    if (formContext.ui.getFormType() == MCS.mcs_Service_Activity_OnLoad.FORM_TYPE_CREATE) {
        formContext.getControl("cvt_cancelremarks").setVisible(false);
    }
    else {
        if ((_state == 2) && (_cancelComments != null)) {  //the appointment is canceled *AND* there are remarks, so show the field
            formContext.getControl("cvt_cancelremarks").setVisible(true);
        } else {  //either the state is not caneled or there are no comments;
            if (_apptType == true) {
                if (_cancelComments != null) {
                    formContext.getControl("cvt_cancelremarks").setVisible(true);
                }
            } else {
                formContext.getControl("cvt_cancelremarks").setVisible(false);
            }
        }
    }
};