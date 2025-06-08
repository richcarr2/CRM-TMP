//Library Name: mcs_TSA.OnSave.js
if (typeof MCS == "undefined")
    MCS = {};
// Create Namespace container for functions in this library;
MCS.mcs_TSA = {};

MCS.mcs_TSA.CreateName = function (executionContext) {
    var formContext = executionContext.getFormContext();
    var cvt_servicetype = formContext.getAttribute("cvt_servicetype").getValue();
    var cvt_servicesubtype = formContext.getAttribute("cvt_servicesubtype").getValue();
    var cvt_relatedprovidersiteid = formContext.getAttribute("cvt_relatedprovidersiteid").getValue();
    var cvt_type = formContext.getAttribute("cvt_type").getValue();
    var cvt_groupappointment = formContext.getAttribute("cvt_groupappointment").getValue();

    var derivedResultField = "";

    if (cvt_servicetype != null) {
        derivedResultField += cvt_servicetype[0].name;
    }

    if (cvt_servicesubtype != null) {
        derivedResultField += " : ";
        derivedResultField += cvt_servicesubtype[0].name;
    }

    derivedResultField += " From ";

    if (cvt_relatedprovidersiteid != null) {
        derivedResultField += cvt_relatedprovidersiteid[0].name;
    }

    //Group
    if (cvt_groupappointment == true) {
        //derivedResultField += " (G)";
    } else {
        //CVT to home
        if (cvt_type == true) {
            derivedResultField += " to VA Video Connect";
        }
        //else {
        //    derivedResultField += " (I)";
        //}
    }
    if (formContext.getAttribute("mcs_name").getValue() != derivedResultField) {
        formContext.getAttribute("mcs_name").setSubmitMode("always");
        formContext.getAttribute("mcs_name").setValue(derivedResultField);
    }
};

//To move to Common
MCS.mcs_TSA.runRibbonWorkflow = function (executionContext,workflowId) {
    var formContext = executionContext.getFormContext();
    //To move to Common
    MCS.GlobalFunctions.runWorkflow(formContext.data.entity.getId(), workflowId, MCS.GlobalFunctions.runWorkflowResponse);
};

MCS.mcs_TSA.EnforceChanges = function (executionContext) {
    var formContext = executionContext.getFormContext();
    if (MCS.mcs_TSA_OnLoad.GroupAppt != formContext.getAttribute("cvt_groupappointment").getValue())
        formContext.getAttribute("cvt_groupappointment").setSubmitMode("always");

    formContext.getAttribute("cvt_relatedpatientsiteid").setSubmitMode("always");

    if (MCS.mcs_TSA_OnLoad.Form_Type == MCS.cvt_Common.FORM_TYPE_CREATE) {
        formContext.getAttribute("cvt_type").setSubmitMode("always");
    }
    else {
        if (MCS.mcs_TSA_OnLoad.Type != formContext.getAttribute("cvt_type").getValue())
            formContext.getAttribute("cvt_type").setSubmitMode("always");
        else
            formContext.getAttribute("cvt_type").setSubmitMode("never");
    }
};

//Description: Service Activities Warning
MCS.mcs_TSA.RelatedServiceActivitiesWarning = function (executionContext,executionObj) {
    var formContext = executionContext.getFormContext();
    var status = formContext.getAttribute("statuscode").getValue();
    var service = formContext.getAttribute("mcs_relatedserviceid").getValue();
    var EntityId;

    if ((status == 251920000) && (service != null)) {
        if (typeof (MCS.mcs_TSA_OnLoad) != "undefined") {
            EntityId = MCS.mcs_TSA_OnLoad.EntityId;
        }
        else {
            EntityId = window.parent.MCS.mcs_TSA_OnLoad.EntityId;
        }

        var fetchXml = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false' aggregate='true'>";
        fetchXml += "<entity name='serviceappointment'>";
        fetchXml += "<attribute name='subject' alias='recordcount' aggregate='count' />";
        fetchXml += "<filter type='and'>";
        fetchXml += "<condition attribute='mcs_relatedtsa' operator='eq' uitype='mcs_services' value='" + EntityId + "' />";
        fetchXml += "<condition attribute= 'statecode' operator='in'><value>0</value><value>3</value></condition>";
        fetchXml += "</filter>";
        fetchXml += "</entity>";
        fetchXml += "</fetch>";

        XrmSvcToolkit.fetch({
            fetchXml: fetchXml,
            async: false,
            successCallback: function (result) {
                countValue = result.entities[0].recordcount;
                if (countValue > 0) {
                   // alert("You are about to update the Service Components of this TSA. There are open or scheduled Service Activities which are using the previous set of Service components for this TSA. Only future Service Activities related to this TSA will use the newly defined Service Components.");
                    var r = confirm("Service Activities exist that are using previously defined service components of this TSA. Only future Service Activities will inherit the newly defined service components. If you would like to continue and update this TSA's service components, press OK. Otherwise press Cancel to abort and review the Service Activities using the previously defined service components of this TSA.");
                    if (r == true) {
                        x = "Save Confirmed";
                    }
                    else {
                        x = executionObj.getEventArgs().preventDefault();
                        formContext.getAttribute("statuscode").setValue(1);
                    }
                }
            },
            errorCallback: function (error) {
                throw error;
            }
        });
    }    
};

MCS.mcs_TSA.CheckPatientProviderSiteResources = function (executionContext, executionObj) {
    var formContext = executionContext.getFormContext();
    var status = formContext.getAttribute("statuscode").getValue();
    var EntityId;

    if (status == 251920000) {

        if (typeof (MCS.mcs_TSA_OnLoad) != "undefined") {
            EntityId = MCS.mcs_TSA_OnLoad.EntityId;
        }
        else {
            EntityId = window.parent.MCS.mcs_TSA_OnLoad.EntityId;
        }

        //First do a check to make sure there are any Pat / Pro Site Resources added to the TSA. We dont want to move to production if there are none. 
        //if CVT to home then bypass the patient resources
        if (formContext.getAttribute("cvt_type").getValue() != true) {
            var fetchXml = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false' aggregate='true'>";
            fetchXml += "<entity name='cvt_patientresourcegroup'>";
            fetchXml += "<attribute name='cvt_name' alias='recordcount' aggregate='count' />";
            fetchXml += "<filter type='and'>";
            fetchXml += "<condition attribute='cvt_relatedtsaid' operator='eq' uitype='mcs_services' value='" + EntityId + "' />";
            fetchXml += "<condition attribute= 'statecode' operator='in'><value>0</value><value>3</value></condition>";
            fetchXml += "</filter>";
            fetchXml += "</entity>";
            fetchXml += "</fetch>";

            XrmSvcToolkit.fetch({
                fetchXml: fetchXml,
                async: false,
                successCallback: function (result) {
                    countValue = result.entities[0].recordcount;
                    if (countValue < 1) {
                        alert("No Patient Site Resources have been added to this TSA. Patient Site Resources must be added before moving this TSA to Production status.");
                        x = executionObj.getEventArgs().preventDefault();
                        formContext.getAttribute("statuscode").setValue(1);
                    }

                },
                errorCallback: function (error) {
                    throw error;
                }
            });
        }
        var fetchXml = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false' aggregate='true'>";
        fetchXml += "<entity name='cvt_providerresourcegroup'>";
        fetchXml += "<attribute name='cvt_name' alias='recordcount' aggregate='count' />";
        fetchXml += "<filter type='and'>";
        fetchXml += "<condition attribute='cvt_relatedtsaid' operator='eq' uitype='mcs_services' value='" + EntityId + "' />";
        fetchXml += "<condition attribute= 'statecode' operator='in'><value>0</value><value>3</value></condition>";
        fetchXml += "</filter>";
        fetchXml += "</entity>";
        fetchXml += "</fetch>";

        XrmSvcToolkit.fetch({
            fetchXml: fetchXml,
            async: false,
            successCallback: function (result) {
                countValue = result.entities[0].recordcount;
                if (countValue < 1) {
                    alert("No Provider Site Resources have been added to this TSA. Provider Site Resources must be added before moving this TSA to Production status.");
                    x = executionObj.getEventArgs().preventDefault();
                    formContext.getAttribute("statuscode").setValue(1);
                }

            },
            errorCallback: function (error) {
                throw error;
            }
        });

        //Now we are going to check for the resourcespecguid on the Pat / Pro Site Resources which is need to build the TSA Service. 
        //if CVT to home then bypass the patient resources
        if (formContext.getAttribute("cvt_type").getValue() != true) {
            var fetchXml = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false' aggregate='true'>";
            fetchXml += "<entity name='cvt_patientresourcegroup'>";
            fetchXml += "<attribute name='cvt_name' alias='recordcount' aggregate='count' />";
            fetchXml += "<filter type='and'>";
            fetchXml += "<condition attribute='cvt_relatedtsaid' operator='eq' uitype='mcs_services' value='" + EntityId + "' />";
            fetchXml += "<condition attribute= 'statecode' operator='in'><value>0</value><value>3</value></condition>";
            fetchXml += "<condition attribute='cvt_resourcespecguid' operator='null'/>";
            fetchXml += "</filter>";
            fetchXml += "</entity>";
            fetchXml += "</fetch>";

            XrmSvcToolkit.fetch({
                fetchXml: fetchXml,
                async: false,
                successCallback: function (result) {
                    countValue = result.entities[0].recordcount;
                    if (countValue > 0) {
                        alert("Some of the Patient Site Resources added to this TSA are still generating their Service Components. Please wait a few moments and try to Save to Production again");
                        x = executionObj.getEventArgs().preventDefault();
                        formContext.getAttribute("statuscode").setValue(1);
                    }

                },
                errorCallback: function (error) {
                    throw error;
                }
            });
        }
        var fetchXml = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false' aggregate='true'>";
        fetchXml += "<entity name='cvt_providerresourcegroup'>";
        fetchXml += "<attribute name='cvt_name' alias='recordcount' aggregate='count' />";
        fetchXml += "<filter type='and'>";
        fetchXml += "<condition attribute='cvt_relatedtsaid' operator='eq' uitype='mcs_services' value='" + EntityId + "' />";
        fetchXml += "<condition attribute= 'statecode' operator='in'><value>0</value><value>3</value></condition>";
        fetchXml += "<condition attribute='cvt_resourcespecguid' operator='null'/>";
        fetchXml += "</filter>";
        fetchXml += "</entity>";
        fetchXml += "</fetch>";

        XrmSvcToolkit.fetch({
            fetchXml: fetchXml,
            async: false,
            successCallback: function (result) {
                countValue = result.entities[0].recordcount;
                if (countValue > 0) {
                    alert("Some of the Provider Site Resources added to this TSA are still generating their Service Components. Please wait a few moments and try to Save to Production again");
                    x = executionObj.getEventArgs().preventDefault();
                    formContext.getAttribute("statuscode").setValue(1);
                }
            },
            errorCallback: function (error) {
                throw error;
            }
        });
    }
};