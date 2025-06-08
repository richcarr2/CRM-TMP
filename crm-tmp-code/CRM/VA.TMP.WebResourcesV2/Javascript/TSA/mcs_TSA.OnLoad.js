//Library Name: mcs_TSA.OnLoad.js
if (typeof MCS == "undefined")
    MCS = {};
// Create Namespace container for functions in this library;
MCS.mcs_TSA_OnLoad = {};

//Global Variables
MCS.mcs_TSA_OnLoad.Form_Type;
MCS.mcs_TSA_OnLoad.EntityName;
MCS.mcs_TSA_OnLoad.GroupAppt;
MCS.mcs_TSA_OnLoad.Type;
MCS.mcs_TSA_OnLoad.EntityId;

MCS.mcs_TSA_OnLoad.TSAName;
MCS.mcs_TSA_OnLoad.MTSAName;

MCS.mcs_TSA_OnLoad.relatedProviderSiteId;
MCS.mcs_TSA_OnLoad.relatedProviderSiteName;

MCS.mcs_TSA_OnLoad.relatedPatientSiteId;
MCS.mcs_TSA_OnLoad.relatedPatientSiteName;

MCS.mcs_TSA_OnLoad.relatedPatientFacilityId;
MCS.mcs_TSA_OnLoad.relatedPatientFacilityName;

MCS.mcs_TSA_OnLoad.ProvResourcesCreated = new Boolean;

MCS.SiteEmergencyPhone;
MCS.SiteMainPhone;
MCS.TCTPhone = "";
MCS.SiteLocal911Phone;

//Compare for Audit cleanup
MCS.mcs_TSA_OnLoad.Capacity;
MCS.mcs_TSA_OnLoad.InitalStatus;

MCS.mcs_TSA_OnLoad.OnLoad = function (executionContext) {
    var formContext = executionContext.getFormContext();
    MCS.mcs_TSA_OnLoad.CheckForMTSA(executionContext);
    if (formContext.ui.getFormType() == MCS.cvt_Common.FORM_TYPE_CREATE) {
        alert('The Archived Agreement functionality is obselete and the new record creation is not available.');
        formContext.ui.close();
        return;
    }
    else {
        if (formContext.getAttribute("cvt_relatedmasterid").getValue() == null)
            return;
        //Actually set all the values used in the library
        MCS.mcs_TSA_OnLoad.Form_Type = formContext.ui.getFormType();
        MCS.mcs_TSA_OnLoad.EntityName = formContext.data.entity.getEntityName();
        MCS.mcs_TSA_OnLoad.GroupAppt = (formContext.getAttribute("cvt_groupappointment") != null) ? formContext.getAttribute("cvt_groupappointment").getValue() : false;
        MCS.mcs_TSA_OnLoad.Type = (formContext.getAttribute("cvt_type") != null) ? formContext.getAttribute("cvt_type").getValue() : false;
        MCS.mcs_TSA_OnLoad.EntityId = formContext.data.entity.getId();

        if (MCS.mcs_TSA_OnLoad.Form_Type == MCS.cvt_Common.FORM_TYPE_CREATE)
            MCS.cvt_Common.DateTime('cvt_beginngtime', 8, 00);

        MCS.cvt_Common.EnableDependentLookup(executionContext, "cvt_servicetype", "cvt_servicesubtype");

        if (formContext.getAttribute("cvt_relatedmasterid").getValue() != null) {
            formContext.getControl("cvt_relatedmasterid").setDisabled(true);
        }
        //Conditional because of potentially missing data.
        if (formContext.getAttribute("cvt_providerlocationtype").getValue() != null) {
            formContext.getControl("cvt_providerlocationtype").setDisabled(true);
        }
        else {
            formContext.getControl("cvt_providerlocationtype").setDisabled(false);
        }

        formContext.getControl("mcs_name").setFocus(); //To bring to the top

        formContext.getAttribute("cvt_relatedpatientsiteid").setSubmitMode("always");
    }
    };

MCS.mcs_TSA_OnLoad.BulkEdit = function (executionContext) {
    var formContext = executionContext.getFormContext();
    //Not Bulk Edit
    if (MCS.mcs_TSA_OnLoad.Form_Type != 6) {
        //Hide the Bulk Edit section on all forms
        formContext.ui.tabs.get('tab_Pat').sections.get('tab_Pat_section_Bulk').setVisible(false);

        //Set all sections to hidden on form.  Show all sections
        //TSA tab
        formContext.ui.tabs.get('tab_Name').setVisible(true);

        //General section
        formContext.ui.tabs.get('tab_Info').sections.get('tab_Info_section_General').setVisible(true);
        formContext.ui.tabs.get('tab_Info').sections.get('tab_Info_section_General2').setVisible(true);

        //Provider Grid section
        formContext.ui.tabs.get('tab_Prov').sections.get('tab_provresources').setVisible(true);

        //Patient Grid section
        formContext.ui.tabs.get('tab_Pat').sections.get('tab_patientresources').setVisible(true);
        //Set Emergency IFRAME true       
        formContext.ui.tabs.get('tab_Pat').sections.get('tab_Info_section_Emergency').setVisible(true);
        formContext.getControl('IFRAME_EmergencyContactViewer').setVisible(true);

        //Admin tab
        formContext.ui.tabs.get('tab_Admin').setVisible(true);

        MCS.mcs_TSA_OnLoad.SOS(executionContext);

        //Load Operations Guide
        var filter = "mcs_name eq 'Active Settings'";

        Xrm.WebApi.retrieveMultipleRecords("mcs_setting", "?$select=cvt_telehealthoperationsmanual&$filter=" + filter).then(
            function success(result) {
                if (result != null && result.entities.length != 0) {
                    var url = result.entities[0].cvt_telehealthoperationsmanual != null ? result.entities[0].cvt_telehealthoperationsmanual : null;
                    if (url != null)
                        formContext.getAttribute("cvt_telehealthoperationsmanual").setValue(url);
                }
            },
            function (error) {
            }
        );
        //calls = CrmRestKit.ByQuery("mcs_setting", ['cvt_telehealthoperationsmanual'], filter, false);
        //calls.fail(function (err) {
        //}).done(function (data) {
        //    if (data && data.d && data.d.results != null && data.d.results.length != 0) {
        //        var url = data.d.results[0].cvt_telehealthoperationsmanual != null ? data.d.results[0].cvt_telehealthoperationsmanual : null;
        //        if (url != null)
        //            formContext.getAttribute("cvt_telehealthoperationsmanual").setValue(url);
        //    }
        //});
    }
};

//Getting Attributes needed for Quick Create Buttons
MCS.mcs_TSA_OnLoad.GetAttributes = function (executionContext) {
    var formContext = executionContext.getFormContext();
    //TSA
    //if (MCS.mcs_TSA_OnLoad.EntityName == "mcs_services") {
    MCS.mcs_TSA_OnLoad.TSAName = formContext.getAttribute("mcs_name").getValue();

    var provSiteField = formContext.getAttribute("cvt_relatedprovidersiteid");
    var patSiteField = formContext.getAttribute("cvt_relatedpatientsiteid");
    switch (MCS.mcs_TSA_OnLoad.GroupAppt) {
        case false:
            if (patSiteField.getValue() != null) {
                var relatedPatientSite = patSiteField.getValue();
                MCS.mcs_TSA_OnLoad.relatedPatientSiteId = relatedPatientSite[0].id;
                MCS.mcs_TSA_OnLoad.relatedPatientSiteName = relatedPatientSite[0].name;
            }
            if (provSiteField.getValue() != null) {
                var relatedProviderSite = provSiteField.getValue();
                MCS.mcs_TSA_OnLoad.relatedProviderSiteId = relatedProviderSite[0].id;
                MCS.mcs_TSA_OnLoad.relatedProviderSiteName = relatedProviderSite[0].name;
            }
            break;

        case true:
            if (provSiteField.getValue() != null) {
                var relatedProviderSite = provSiteField.getValue();
                MCS.mcs_TSA_OnLoad.relatedProviderSiteId = relatedProviderSite[0].id;
                MCS.mcs_TSA_OnLoad.relatedProviderSiteName = relatedProviderSite[0].name;
            }
            var patFacilityField = formContext.getAttribute("cvt_patientfacility");
            if (patFacilityField.getValue() != null) {
                MCS.mcs_TSA_OnLoad.relatedPatientFacilityId = patFacilityField.getValue()[0].id;
                MCS.mcs_TSA_OnLoad.relatedPatientFacilityName = patFacilityField.getValue()[0].name;
            }
            break;
    }
};

MCS.mcs_TSA_OnLoad.LoadProvResources = function (executionObj) {
    var formContext = executionContext.getFormContext();
    if (formContext.getAttribute("cvt_relatedmasterid").getValue() == null)
        return;
    var mtsaId = formContext.getAttribute("cvt_relatedmasterid").getValue()[0].id;

    if ((mtsaId != null) && (MCS.mcs_TSA_OnLoad.Form_Type == 1) && (MCS.mcs_TSA_OnLoad.ProvResourcesCreated == false)) {

        //Looking up the Prov Site Resources related to the MTSA we came from.     
        //var columns = "<attribute name='cvt_providerresourcegroupid'/><attribute name='cvt_tsaresourcetype'/>";
        var columns = [
            'cvt_providerresourcegroupid',
            'cvt_tsaresourcetype'
        ];
        var conditions = [
            "<condition attribute='cvt_relatedmastertsaid' operator='eq' uitype='cvt_mastertsa' value='" + mtsaId + "' />",
            "<condition attribute= 'statecode' operator='in'><value>0</value><value>3</value></condition>"
        ];
        var fetchXml = MCS.cvt_Common.CreateFetch('cvt_providerresourcegroup', columns, conditions, null);

        XrmSvcToolkit.fetch({
            fetchXml: fetchXml,
            async: true,
            successCallback: function (result) {
                countValue = result.entities.length;
                if (countValue > 0) {
                    for (var i = 0; i < countValue; i++) {
                        var provResourceId = result.entities[i].cvt_providerresourcegroupid;
                        var tsaResourceType = result.entities[i].cvt_tsaresourcetype.Value;

                        //Single Resource
                        if (tsaResourceType == 1) {
                            Xrm.WebApi.retrieveRecord("cvt_providerresourcegroup", provResourceId, "?$select=cvt_name,cvt_TSAResourceType,cvt_Type,cvt_RelatedResourceId,cvt_relatedsiteid").then(
                                function success(result) {
                                    Name = result.cvt_name;
                                    Type = result.cvt_Type.Value;
                                    TSAType = result.cvt_TSAResourceType.Value;
                                    RelatedSite = result.cvt_relatedsiteid;
                                    Resource = result.cvt_RelatedResourceId;
                                },
                                function () {
                                    alert("Retrieve Failed")
                                }
                            );

                            //CrmRestKit.Retrieve('cvt_providerresourcegroup', provResourceId, ['cvt_name', 'cvt_TSAResourceType', 'cvt_Type', 'cvt_RelatedResourceId', 'cvt_relatedsiteid'], false)
                            //         .fail(function () { alert("Retrieve Failed") })
                            //         .done(function (data) {
                            //             Name = data.d.cvt_name;
                            //             Type = data.d.cvt_Type.Value;
                            //             TSAType = data.d.cvt_TSAResourceType.Value;
                            //             RelatedSite = data.d.cvt_relatedsiteid;
                            //             Resource = data.d.cvt_RelatedResourceId;                                      
                            //         })
                            var provResource = {
                                'cvt_name': Name,
                                'cvt_TSAResourceType': { __metadata: { type: "Microsoft.Crm.Sdk.Data.Services.OptionSetValue" }, Value: TSAType },
                                'cvt_Type': { __metadata: { type: "Microsoft.Crm.Sdk.Data.Services.OptionSetValue" }, Value: Type },
                                // 'cvt_RelatedTSAid': { __metadata: { type: "Microsoft.Crm.Sdk.Data.Services.EntityReference" }, Id: EntityId, LogicalName: EntityName },                             
                                'cvt_RelatedResourceId': { __metadata: { type: "Microsoft.Crm.Sdk.Data.Services.EntityReference" }, Id: Resource.Id, LogicalName: Resource.LogicalName },
                                'cvt_relatedsiteid': { __metadata: { type: "Microsoft.Crm.Sdk.Data.Services.EntityReference" }, Id: RelatedSite.Id, LogicalName: RelatedSite.LogicalName },
                                'cvt_mtsaguid': mtsaId
                            };
                            Xrm.WebApi.createRecord("cvt_providerresourcegroup", provResource).then(
                                function success(result) {
                                },
                                function (error) {
                                    alert("Create failed")
                                }
                            );
                            //CrmRestKit.Create('cvt_providerresourcegroup', provResource, true)
                            //.fail(function () { alert("Create failed") })
                            //.done()
                        }

                        //Resource Group
                        if (tsaResourceType == 0) {
                            Xrm.WebApi.retrieveRecord("cvt_providerresourcegroup", provResourceId, "?$select=cvt_name,cvt_TSAResourceType,cvt_Type,cvt_RelatedResourceGroupid").then(
                                function success(result) {
                                    Name = result.cvt_name;
                                    Type = result.cvt_Type.Value;
                                    TSAType = result.cvt_TSAResourceType.Value;
                                    RelatedSite = result.cvt_relatedsiteid;
                                    ResourceGroup = result.cvt_RelatedResourceGroupid;
                                },
                                function (error) {
                                    alert("Retrieve Failed")
                                }
                            );
                            //CrmRestKit.Retrieve('cvt_providerresourcegroup', provResourceId, ['cvt_name', 'cvt_TSAResourceType', 'cvt_Type', 'cvt_RelatedResourceGroupid', 'cvt_relatedsiteid'], false)
                            //         .fail(function () { alert("Retrieve Failed") })
                            //         .done(function (data) {
                            //             Name = data.d.cvt_name;
                            //             Type = data.d.cvt_Type.Value;
                            //             TSAType = data.d.cvt_TSAResourceType.Value;
                            //             RelatedSite = data.d.cvt_relatedsiteid;
                            //             ResourceGroup = data.d.cvt_RelatedResourceGroupid;                                        
                            //         })
                            var provResource = {
                                'cvt_name': Name,
                                'cvt_TSAResourceType': { __metadata: { type: "Microsoft.Crm.Sdk.Data.Services.OptionSetValue" }, Value: TSAType },
                                'cvt_Type': { __metadata: { type: "Microsoft.Crm.Sdk.Data.Services.OptionSetValue" }, Value: Type },
                                //  'cvt_RelatedTSAid': { __metadata: { type: "Microsoft.Crm.Sdk.Data.Services.EntityReference" }, Id: EntityId, LogicalName: EntityName },
                                'cvt_RelatedResourceGroupid': { __metadata: { type: "Microsoft.Crm.Sdk.Data.Services.EntityReference" }, Id: ResourceGroup.Id, LogicalName: ResourceGroup.LogicalName },
                                'cvt_relatedsiteid': { __metadata: { type: "Microsoft.Crm.Sdk.Data.Services.EntityReference" }, Id: RelatedSite.Id, LogicalName: RelatedSite.LogicalName },
                                'cvt_mtsaguid': mtsaId
                            };
                            Xrm.WebApi.createRecord("cvt_providerresourcegroup", provResource).then(
                                function success(result) {
                                },
                                function (error) {
                                    alert("Create failed")
                                }
                            );

                            //CrmRestKit.Create('cvt_providerresourcegroup', provResource, true)
                            //    .fail(function () { alert("Create failed") })
                            //    .done()
                        }

                        //Provider
                        if (tsaResourceType == 2) {
                            Xrm.WebApi.retrieveRecord("cvt_providerresourcegroup", provResourceId, "?$select=cvt_name,cvt_TSAResourceType,cvt_RelatedUserId,cvt_relatedsiteid").then(
                                function success(result) {
                                    Name = result.cvt_name;
                                    TSAType = result.cvt_TSAResourceType.Value;
                                    RelatedSite = result.cvt_relatedsiteid;
                                    User = result.cvt_RelatedUserId;
                                },
                                function () {
                                    alert("Retrieve Failed")
                                }
                            );
                            //CrmRestKit.Retrieve('cvt_providerresourcegroup', provResourceId, ['cvt_name', 'cvt_TSAResourceType', 'cvt_RelatedUserId', 'cvt_relatedsiteid'], false)
                            //         .fail(function () { alert("Retrieve Failed") })
                            //         .done(function (data) {
                            //             Name = data.d.cvt_name;
                            //             TSAType = data.d.cvt_TSAResourceType.Value;
                            //             RelatedSite = data.d.cvt_relatedsiteid;
                            //             User = data.d.cvt_RelatedUserId;
                            //         })
                            var provResource = {
                                'cvt_name': Name,
                                'cvt_TSAResourceType': { __metadata: { type: "Microsoft.Crm.Sdk.Data.Services.OptionSetValue" }, Value: TSAType },
                                //  'cvt_RelatedTSAid': { __metadata: { type: "Microsoft.Crm.Sdk.Data.Services.EntityReference" }, Id: EntityId, LogicalName: EntityName },                                
                                'cvt_RelatedUserId': { __metadata: { type: "Microsoft.Crm.Sdk.Data.Services.EntityReference" }, Id: User.Id, LogicalName: User.LogicalName },
                                'cvt_relatedsiteid': { __metadata: { type: "Microsoft.Crm.Sdk.Data.Services.EntityReference" }, Id: RelatedSite.Id, LogicalName: RelatedSite.LogicalName },
                                'cvt_mtsaguid': mtsaId
                            };
                            Xrm.WebApi.createRecord("cvt_providerresourcegroup", provResource).then(
                                function success(result) {
                                },
                                function () {
                                    alert("Create failed")
                                }
                            );
                            //CrmRestKit.Create('cvt_providerresourcegroup', provResource, true)
                            //.fail(function () { alert("Create failed") })
                            //.done()
                        }
                    }
                }
            },
            errorCallback: function (error) {
                throw error;
            }
        });
    }
};

MCS.mcs_TSA_OnLoad.AssignProvResources = function (executionContext, executionObj) {
    var formContext = executionContext.getFormContext();
    if (formContext.getAttribute("cvt_relatedmasterid").getValue() == null)
        return;
    var mtsaId = formContext.getAttribute("cvt_relatedmasterid").getValue()[0].id;
    var EntityId;
    var EntityName;

    if ((mtsaId != null) && (MCS.mcs_TSA_OnLoad.Form_Type == 2)) {
        if (typeof (MCS.mcs_TSA_OnLoad) != "undefined") {
            EntityId = MCS.mcs_TSA_OnLoad.EntityId;
            EntityName = MCS.mcs_TSA_OnLoad.EntityName;
        }
        else {
            EntityId = window.parent.MCS.mcs_TSA_OnLoad.EntityId;
            EntityName = window.parent.MCS.mcs_TSA_OnLoad.EntityName;
        }

        //Looking up the Prov Site Resources related to the MTSA we came from. 
        var columns = [
            'cvt_providerresourcegroupid',
            'cvt_tsaresourcetype'
        ];
        var conditions = [
            "<condition attribute='cvt_mtsaguid' value='" + mtsaId + "' operator='eq'/>",
            "<condition attribute= 'statecode' operator='in'><value>0</value><value>3</value></condition>"
        ];
        var fetchXml = MCS.cvt_Common.CreateFetch('cvt_providerresourcegroup', columns, conditions, null);

        XrmSvcToolkit.fetch({
            fetchXml: fetchXml,
            async: false,
            successCallback: function (result) {
                countValue = result.entities.length;
                if (countValue > 0) {
                    for (var i = 0; i < countValue; i++) {
                        var provResourceId = result.entities[i].cvt_providerresourcegroupid;

                        var provResource = {
                            'cvt_RelatedTSAid': { __metadata: { type: "Microsoft.Crm.Sdk.Data.Services.EntityReference" }, Id: EntityId, LogicalName: EntityName },
                            'cvt_mtsaguid': ''
                        };
                        Xrm.WebApi.updateRecord("cvt_providerresourcegroup", provResourceId, provResource).then(
                            function success(result) {
                            },
                            function (error) {
                                alert("Update Failed")
                            }
                        );

                        //CrmRestKit.Update('cvt_providerresourcegroup', provResourceId, provResource, false)
                        //                .fail(function () { alert("Update Failed") })
                        //                .done(function () {
                        //                })
                    }
                }
            },
            errorCallback: function (error) {
                throw error;
            }
        });
    }
};

MCS.mcs_TSA_OnLoad.RefreshGridAgain = function (executionContext) {
    var formContext = executionContext.getFormContext();
    if (formContext.getAttribute("cvt_relatedmasterid").getValue() == null)
        return;
    var mtsaId = formContext.getAttribute("cvt_relatedmasterid").getValue()[0].id;
    if ((mtsaId != null) && (MCS.mcs_TSA_OnLoad.Form_Type == 2)) {

        var ProvSiteResourcesGrid = document.getElementById("provgroups");
        if (ProvSiteResourcesGrid == null) {
            setTimeout(function () { MCS.mcs_TSA_OnLoad.RefreshGridAgain(); }, 500);
            return;
        }
        setTimeout(function () {

            if (ProvSiteResourcesGrid.control.Refresh != undefined) {
                ProvSiteResourcesGrid.control.Refresh()
            }
            else {
                ProvSiteResourcesGrid.control.refresh()
            }
        }, 3000);

        setTimeout(function () {

            if (ProvSiteResourcesGrid.control.Refresh != undefined) {
                ProvSiteResourcesGrid.control.Refresh()
            }
            else {
                ProvSiteResourcesGrid.control.refresh()
            }
        }, 3000);

        setTimeout(function () {

            if (ProvSiteResourcesGrid.control.Refresh != undefined) {
                ProvSiteResourcesGrid.control.Refresh()
            }
            else {
                ProvSiteResourcesGrid.control.refresh()
            }
        }, 3000);

        setTimeout(function () {

            if (ProvSiteResourcesGrid.control.Refresh != undefined) {
                ProvSiteResourcesGrid.control.Refresh()
            }
            else {
                ProvSiteResourcesGrid.control.refresh()
            }
        }, 3000);
    }
};

MCS.mcs_TSA_OnLoad.showEmergencyContactViewer = function (executionContext) {
    var formContext = executionContext.getFormContext();
    if (formContext.ui.getFormType() == MCS.cvt_Common.FORM_TYPE_CREATE) {
        formContext.ui.close();
    }
    else {
        if (formContext.getAttribute("cvt_relatedmasterid").getValue() == null)
            return;
        if (MCS.mcs_TSA_OnLoad.Form_Type == MCS.cvt_Common.FORM_TYPE_CREATE || MCS.mcs_TSA_OnLoad.GroupAppt == true || MCS.mcs_TSA_OnLoad.Type == true) {
            formContext.getControl("IFRAME_EmergencyContactViewer").setVisible(false);
            return;
        }
        if (formContext.getAttribute("cvt_relatedpatientsiteid").getValue() == null) {
            alert("Please select a Patient Site.");
            formContext.getControl("IFRAME_EmergencyContactViewer").setVisible(false);
            return;
        }
        var siteId = formContext.getAttribute("cvt_relatedpatientsiteid").getValue()[0];
        Xrm.WebApi.retrieveRecord("mcs_site", siteId.id, "?$select=cvt_Local911,cvt_UrgentEmergencyPhone,cvt_phone").then(
            function success(result) {
                var resultFields = data.d;
                var patTCTteam = (formContext.getAttribute("cvt_patientsitetctteam") != null) ? formContext.getAttribute("cvt_patientsitetctteam").getValue() : null;
                MCS.SiteLocal911Phone = resultFields["cvt_Local911"];
                MCS.SiteEmergencyPhone = resultFields["cvt_UrgentEmergencyPhone"];
                MCS.SiteMainPhone = resultFields["cvt_phone"];

                if (patTCTteam != null) {
                    //var filter = "SystemUserId eq (Guid' " + userId + "') and TeamId eq (Guid' " + teamid + "')";
                    var filter = "TeamId eq (Guid'" + patTCTteam[0].id + "')";;   //Teammembers where teamid = PatTCTTeam
                    Xrm.WebApi.retrieveMultipleRecords("TeamMembership", "?$select=TeamId', 'SystemUserId&$filter=" + filter).then(
                        function success(result1) {
                            for (var i = 0; i < result1.entities.length; i++) {
                                //alert("Results found: " + result1.entities.length);
                                var teamMembers = result1.entities[i];
                                for (var i = 0; i < teamMembers.length; i++) {
                                    var tctName = "";
                                    var tctPhone = "";
                                    Xrm.WebApi.retrieveRecord("SystemUser", teamMembers[i].SystemUserId, "?$select=FirstName,LastName,cvt_officephone,MobilePhone").then(
                                        function success(result2) {
                                            var office = result2["cvt_officephone"];
                                            var mobile = result2["MobilePhone"];
                                            tctName = result2["FirstName"] + " " + result2["LastName"];
                                            tctPhone = (mobile == null) ? office : mobile;
                                        },
                                        function (error) {
                                            alert("user retrieve failed");
                                        }
                                    );
                                    if (tctPhone != null) {
                                        if (MCS.TCTPhone != "")
                                            MCS.TCTPhone += "; ";
                                        MCS.TCTPhone += tctName + ': ' + tctPhone;
                                    }
                                }
                            }
                        },
                        function (error) {
                            alert("team member retrieve failed");
                        }
                    );
                }
                else {
                    MCS.TCTPhone = "No TCT Team Members";
                }
            },
            function (error) {
            }
        );
        var urlBuilder = MCS.cvt_Common.BuildRelationshipServerUrl() + "/WebResources/cvt_EmergencyContactViewer.html";
        var IFrame = formContext.ui.controls.get("IFRAME_EmergencyContactViewer");

        // Use the setSrc method so that the IFRAME uses the new page with the existing parameters
        IFrame.setSrc(urlBuilder);
    }
    };

MCS.mcs_TSA_OnLoad.CheckForMTSA = function (executionContext) {
    var formContext = executionContext.getFormContext();
    //Check if formType = create
    if (formContext.ui.getFormType() == MCS.cvt_Common.FORM_TYPE_CREATE) {
        //Check for MTSA - //No MTSA - must be created from one.
        if (formContext.getAttribute("cvt_relatedmasterid").getValue() == null)
            MCS.cvt_Common.closeWindow(executionContext,"A TSA must be created from a MTSA.\n\nPlease close this TSA form and start from a MTSA.");
    }
};

MCS.mcs_TSA_OnLoad.SOS = function (executionContext) {
    var formContext = executionContext.getFormContext();
    //Make the SOS field visible and get the url
    if (formContext.getAttribute("cvt_servicetype").getValue() != null) {
        Xrm.WebApi.retrieveRecord("mcs_servicetype", formContext.getAttribute("cvt_servicetype").getValue()[0].id, "?$select=cvt_specialtyoperationssupplement").then(
            function success(result) {
                var url = data.d["cvt_specialtyoperationssupplement"];

                if (url) {
                    formContext.getControl("cvt_specialtyoperationssupplement").setVisible(true);
                    formContext.getAttribute("cvt_specialtyoperationssupplement").setValue(url);
                }
            },
            function (error) {
            }
        );

        //CrmRestKit.Retrieve('mcs_servicetype', Xrm.Page.getAttribute("cvt_servicetype").getValue()[0].id, ["cvt_specialtyoperationssupplement"], false).fail(function (err) {
        //    //alert("fail");
        //}).done(function (data) {
        //    //alert("success");
        //    var url = data.d["cvt_specialtyoperationssupplement"];

        //    if (url) {
        //        Xrm.Page.getControl("cvt_specialtyoperationssupplement").setVisible(true);
        //        Xrm.Page.getAttribute("cvt_specialtyoperationssupplement").setValue(url);
        //    }
        //});
    }
};