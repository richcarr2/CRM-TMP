//If the SDK namespace object is not defined, create it.
if (typeof MCS == "undefined")
    MCS = {};

// Create Namespace container for functions in this library;
if (typeof MCS.mcs_Service_Activity == "undefined")
    MCS.mcs_Service_Activity = {};

MCS.mcs_Service_Activity.FORM_TYPE_CREATE = 1;
MCS.mcs_Service_Activity.FORM_TYPE_UPDATE = 2;
MCS.mcs_Service_Activity.FORM_TYPE_READ_ONLY = 3;
MCS.mcs_Service_Activity.FORM_TYPE_DISABLED = 4;

//Gets the Scheduling Package Data - populates fields on Service Activity
MCS.mcs_Service_Activity.GetSchedulingPackageData = function (executionContext) {
    var formContext = executionContext.getFormContext();
    var relatedSchedulingPackage = formContext.getAttribute("cvt_relatedschedulingpackage").getValue();

    if (relatedSchedulingPackage != null) {
        MCS.mcs_Service_Activity.getcvt_relatedschedulingpackageLookupData(executionContext, relatedSchedulingPackage[0].id);
    }
    else {
        formContext.getAttribute("serviceid").setValue(null);
        formContext.getAttribute("mcs_servicetype").setValue(null);
        formContext.getAttribute("mcs_servicesubtype").setValue(null);
    }
};

//re-written piece leveraging the Crm Rest Kit Library - pulls in values from the Scheduling Package and sets appropriate fields on Service Activity
MCS.mcs_Service_Activity.getcvt_relatedschedulingpackageLookupData = function (executionContext, cvt_relatedschedulingpackage) {
    var formContext = executionContext.getFormContext();

    Xrm.WebApi.retrieveRecord("cvt_resourcepackage", cvt_relatedschedulingpackage, "?$select=_cvt_specialtysubtype_value,_cvt_specialty_value,_cvt_relatedservice_value,cvt_patientlocationtype,cvt_availabletelehealthmodality,cvt_groupappointment").then(
        function success(result) {
            //MCS.mcs_Service_Activity.SetLookup(result.cvt_relatedservice, formContext.getAttribute("serviceid"));
            //debugger;
            var value = result["_cvt_specialty_value"];
            var Name = result["_cvt_specialty_value@OData.Community.Display.V1.FormattedValue"];
            var LogicalName = result["_cvt_specialty_value@Microsoft.Dynamics.CRM.lookuplogicalname"];
            MCS.mcs_Service_Activity.SetNewLookup(value, Name, LogicalName, formContext.getAttribute("mcs_servicetype"));

            if (result._cvt_specialtysubtype_value != null) {
                //MCS.mcs_Service_Activity.SetLookup(result._cvt_specialtysubtype_value, formContext.getAttribute("mcs_servicesubtype"));
                MCS.mcs_Service_Activity.SetNewLookup(result["_cvt_specialtysubtype_value"], result["_cvt_specialtysubtype_value@OData.Community.Display.V1.FormattedValue"], result["_cvt_specialtysubtype_value@Microsoft.Dynamics.CRM.lookuplogicalname"], formContext.getAttribute("mcs_servicesubtype"));
            }


            //MCS.mcs_Service_Activity.SetLookup(result.cvt_relatedpatientsiteid, formContext.getAttribute("mcs_relatedsite"));
            //MCS.mcs_Service_Activity.SetLookup(result.cvt_relatedprovidersiteid, formContext.getAttribute("mcs_relatedprovidersite"));
            //Fire OnChange so that SubType shows
            formContext.getAttribute("mcs_servicetype").fireOnChange();
            if (result.cvt_patientlocationtype != null)
                formContext.getAttribute("cvt_type").setValue(result.cvt_patientlocationtype == 917290001);
            if (result.cvt_groupappointment != null)
                formContext.getAttribute("mcs_groupappointment").setValue(result.cvt_groupappointment);
            formContext.getAttribute("cvt_telehealthmodality").setValue((result.cvt_availabletelehealthmodality == 917290001))
            //if (result.cvt_SchedulingInstructions != null)
            //    formContext.getAttribute("cvt_schedulinginstructions").setValue(result.cvt_SchedulingInstructions);

            var isVVC = formContext.getAttribute("cvt_type").getValue() == true;
            var isPatientSiteResourcesRequired = formContext.getAttribute("cvt_patientsiteresourcesrequired").getValue() == true;


            //If it is VVC, then use the SP service
            if (isVVC && !isPatientSiteResourcesRequired) {
                //MCS.mcs_Service_Activity.SetLookup(result.cvt_relatedservice, formContext.getAttribute("serviceid"));
                MCS.mcs_Service_Activity.SetNewLookup(result["_cvt_relatedservice_value"], result["_cvt_relatedservice_value@OData.Community.Display.V1.FormattedValue"], result["_cvt_relatedservice_value@Microsoft.Dynamics.CRM.lookuplogicalname"], formContext.getAttribute("serviceid"));
            }
            else {

                cvt_relatedschedulingpackage = cvt_relatedschedulingpackage.substr(1, 36);
                var filter = "_cvt_resourcepackage_value eq " + cvt_relatedschedulingpackage;
                var siteId = "";

                // If patient site resources are required, search related site

                if (isPatientSiteResourcesRequired) {
                    if (formContext.getAttribute("mcs_relatedsite").getValue() != null) {
                        siteId = formContext.getAttribute("mcs_relatedsite").getValue()[0].id;
                        filter += " and cvt_locationtype eq 917290001";
                    }
                }
                //if it is group, then search the Provider PS
                else if (result.cvt_groupappointment == true) {
                    if (formContext.getAttribute("mcs_relatedprovidersite").getValue() != null) {
                        siteId = formContext.getAttribute("mcs_relatedprovidersite").getValue()[0].id;
                        filter += " and cvt_locationtype eq 917290000";
                    }
                }
                else { //search Patient PS
                    if (formContext.getAttribute("mcs_relatedsite").getValue() != null) {
                        siteId = formContext.getAttribute("mcs_relatedsite").getValue()[0].id;
                        filter += " and cvt_locationtype eq 917290001";
                    }
                }
                if (siteId != "" || siteId != null) {

                    siteId = siteId.substr(1, 36);

                    if (!isPatientSiteResourcesRequired)
                        filter += " and _cvt_site_value eq " + siteId;

                    Xrm.WebApi.retrieveMultipleRecords("cvt_participatingsite", "?$select=_cvt_relatedservice_value,cvt_grouppatientbranch&$filter=" + filter).then(
                        function success(result) {

                            if (result != null && result.entities.length != 0) {

                                MCS.mcs_Service_Activity.SetNewLookup(result.entities[0]._cvt_relatedservice_value, result.entities[0]["_cvt_relatedservice_value@OData.Community.Display.V1.FormattedValue"], "service", formContext.getAttribute("serviceid"));
                                //MCS.mcs_Service_Activity.SetLookup(result.entities[0]._cvt_relatedservice_value, formContext.getAttribute("serviceid"));
                                //Set the id to this field, then make sure it goes into the Resources field.
                                MCS.mcs_Service_Activity.GroupPat(result.entities[0].cvt_grouppatientbranch, executionContext);

                            }
                            else {
                                alert("No Participating Site was retrieved, and therefore no service was retrieved.");
                            }
                        },
                        function (error) {
                            alert("Error: " + error.message);
                        }
                    );
                }
                else {
                    alert("Appropriate Site was not filled in prior to selecting the Scheduling Package.");
                }
            }
        },
        function (error) {
            alert("Please verify that this Scheduling Package is in Production, and if so, contact your system administrator");
            return;
        }
    );

    //CrmRestKit.Retrieve('cvt_resourcepackage', cvt_relatedschedulingpackage, ['cvt_specialtysubtype', 'cvt_specialty', 'cvt_relatedservice', 'cvt_patientlocationtype', 'cvt_availabletelehealthmodality',
    //    //'cvt_relatedpatientsiteid', 'cvt_relatedprovidersiteid',
    //    'cvt_groupappointment'], true).fail(
    //        function (err) {
    //            alert("Please verify that this Scheduling Package is in Production, and if so, contact your system administrator");
    //            return;
    //        }).done(
    //            function (data) {
    //                //MCS.mcs_Service_Activity.SetLookup(data.d.cvt_relatedservice, Xrm.Page.getAttribute("serviceid"));
    //                MCS.mcs_Service_Activity.SetLookup(data.d.cvt_specialty, Xrm.Page.getAttribute("mcs_servicetype"));
    //                MCS.mcs_Service_Activity.SetLookup(data.d.cvt_specialtysubtype, Xrm.Page.getAttribute("mcs_servicesubtype"));
    //                //MCS.mcs_Service_Activity.SetLookup(data.d.cvt_relatedpatientsiteid, Xrm.Page.getAttribute("mcs_relatedsite"));
    //                //MCS.mcs_Service_Activity.SetLookup(data.d.cvt_relatedprovidersiteid, Xrm.Page.getAttribute("mcs_relatedprovidersite"));

    //                //Fire OnChange so that SubType shows
    //                Xrm.Page.getAttribute("mcs_servicetype").fireOnChange();

    //                if (data.d.cvt_patientlocationtype != null)
    //                    Xrm.Page.getAttribute("cvt_type").setValue(data.d.cvt_patientlocationtype.Value == 917290001);
    //                if (data.d.cvt_groupappointment != null)
    //                    Xrm.Page.getAttribute("mcs_groupappointment").setValue(data.d.cvt_groupappointment);
    //                Xrm.Page.getAttribute("cvt_telehealthmodality").setValue((data.d.cvt_availabletelehealthmodality.Value == 917290001))
    //                //if (data.d.cvt_SchedulingInstructions != null)
    //                //    Xrm.Page.getAttribute("cvt_schedulinginstructions").setValue(data.d.cvt_SchedulingInstructions);

    //                //If it is VVC, then use the SP service
    //                if (Xrm.Page.getAttribute("cvt_type").getValue() == true) {
    //                    MCS.mcs_Service_Activity.SetLookup(data.d.cvt_relatedservice, Xrm.Page.getAttribute("serviceid"));
    //                }
    //                else {
    //                    var filter = "cvt_resourcepackage/Id eq (Guid'" + cvt_relatedschedulingpackage + "') and statuscode/Value eq 1";
    //                    var siteId = "";
    //                    //if it is group, then search the Provider PS
    //                    if (data.d.cvt_groupappointment == true) {
    //                        if (Xrm.Page.getAttribute("mcs_relatedprovidersite").getValue() != null) {
    //                            siteId = Xrm.Page.getAttribute("mcs_relatedprovidersite").getValue()[0].id;
    //                            filter += " and cvt_locationtype/Value eq 917290000";
    //                        }
    //                    }
    //                    else { //search Patient PS
    //                        if (Xrm.Page.getAttribute("mcs_relatedsite").getValue() != null) {
    //                            siteId = Xrm.Page.getAttribute("mcs_relatedsite").getValue()[0].id;
    //                            filter += " and cvt_locationtype/Value eq 917290001";
    //                        }
    //                    }
    //                    if (siteId != "" || siteId != null) {
    //                        filter += " and cvt_site/Id eq (Guid'" + siteId + "')";
    //                        calls = CrmRestKit.ByQuery("cvt_participatingsite", ['cvt_relatedservice', 'cvt_grouppatientbranch'], filter, false);
    //                        calls.fail(function (err) {
    //                            return;
    //                        }).done(function (data) {
    //                            if (data && data.d && data.d.results != null && data.d.results.length != 0) {
    //                                MCS.mcs_Service_Activity.SetLookup(data.d.results[0].cvt_relatedservice, Xrm.Page.getAttribute("serviceid"));
    //                                //Set the id to this field, then make sure it goes into the Resources field.
    //                                MCS.mcs_Service_Activity.GroupPat(data.d.results[0].cvt_grouppatientbranch);
    //                            }
    //                        });
    //                    }
    //                    else {
    //                        alert("Appropriate Site was not filled in prior to selecting the Scheduling Package.");
    //                    }
    //                }
    //            });
};

//Pass in OData EntityReferences and set a lookup with the EntityReference Value
MCS.mcs_Service_Activity.SetLookup = function (column, targetField) {
    if (targetField != null) {
        var obj = { id: column.Id, entityType: column.LogicalName, name: column.Name }
        if (obj.name == null)
            targetField.setValue(null);
        else
            targetField.setValue([obj]);
    }
}

MCS.mcs_Service_Activity.SetNewLookup = function (value, Name, LogicalName, targetField) {
    //debugger;
    if (targetField != null) {
        var obj = { id: value, entityType: LogicalName, name: Name }
        if (obj.name == null)
            targetField.setValue(null);
        else
            targetField.setValue([obj]);
    }
}

//Show Scheduling Package lookup field once site has been selected
MCS.mcs_Service_Activity.EnableSchedulingPackage = function (executionContext) {
    var formContext = executionContext.getFormContext();
    var relatedSite = null;
    var relatedSiteSLU = null;
    var relatedProviderSite = null;
    var relatedProviderSiteSLU = null;

    var relatedSiteAttribute = formContext.getAttribute("mcs_relatedsite");
    if (relatedSiteAttribute != null)
        relatedSite = relatedSiteAttribute.getValue();
    if (relatedSite != null)
        relatedSiteSLU = relatedSite[0].name;

    var relatedProviderSiteAttribute = formContext.getAttribute("mcs_relatedprovidersite");
    if (relatedProviderSiteAttribute != null)
        relatedProviderSite = relatedProviderSiteAttribute.getValue();
    if (relatedProviderSite != null)
        relatedProviderSiteSLU = relatedProviderSite[0].name;

    if (relatedSiteSLU != null || relatedProviderSiteSLU != null) {
        formContext.getControl("cvt_relatedschedulingpackage").setVisible(true);
        formContext.getControl("cvt_relatedschedulingpackage").setFocus();
    }
    else {
        formContext.getControl("cvt_relatedschedulingpackage").setVisible(false);
        formContext.getAttribute("cvt_relatedschedulingpackage").setValue(null);
    }
};

//Show Specialty Subtype field once Specialty has been selected
MCS.mcs_Service_Activity.EnableServiceSubType = function (executionContext) {
    var formContext = executionContext.getFormContext();
    var serviceType = null;
    var serviceTypeSLU = null;

    if (formContext.getAttribute("mcs_servicetype") != null)
        serviceType = formContext.getAttribute("mcs_servicetype").getValue();
    if (serviceType != null)
        serviceTypeSLU = serviceType[0].name;

    if (serviceTypeSLU != null)
        formContext.getControl("mcs_servicesubtype").setVisible(true);
    else {
        formContext.getControl("mcs_servicesubtype").setVisible(false);
        formContext.getAttribute("mcs_servicesubtype").setValue(null);
    }
};

/* Adds Custom Filtered lookup view for Scheduling Package based on selections
/* for Specialty, Service Sub-Type, and Site(Patient Site). */
MCS.mcs_Service_Activity.HandleOnChangeSchedulingPackageLookup = function (executionContext) {
    var formContext = executionContext.getFormContext();
    var schedulingPackageControl = formContext.getControl("cvt_relatedschedulingpackage");
    if (schedulingPackageControl.getVisible()) {
        var siteObj, site, sitesearch;
        var serviceType = new Array();
        var serviceSubType = new Array();
        var selectedSite = new Array();
        var schedulingPackagetype = formContext.getAttribute("cvt_type").getValue();
        var groupAppt = formContext.getAttribute("mcs_groupappointment") == null ? 0 : formContext.getAttribute("mcs_groupappointment").getValue();
        var providers = formContext.getAttribute("cvt_relatedproviderid").getValue();
        var requirePatResources = formContext.getAttribute("cvt_patientsiteresourcesrequired").getValue();

        //which site are you using to filter the SA lookup?
        if (groupAppt || schedulingPackagetype) { //Group or VVC?
            if (requirePatResources) {
                selectedSite = formContext.getAttribute("mcs_relatedsite").getValue();
            }
            else {
                selectedSite = formContext.getAttribute("mcs_relatedprovidersite").getValue();
            }
        }
        else {
            selectedSite = formContext.getAttribute("mcs_relatedsite").getValue();
        }

        serviceType = formContext.getAttribute("mcs_servicetype").getValue();
        serviceSubType = formContext.getAttribute("mcs_servicesubtype").getValue();

        if (groupAppt == 0) {
            if (schedulingPackagetype == true) { //CVT to Home - Provider
                if (requirePatResources) {
                    siteObj = formContext.getAttribute("mcs_relatedsite");
                    sitesearch = "cvt_patientsites";
                }
                else {
                    siteObj = formContext.getAttribute("mcs_relatedprovidersite");
                    sitesearch = "cvt_providersites";
                }
            }
            else { //(I) - Patient
                siteObj = formContext.getAttribute("mcs_relatedsite");
                sitesearch = "cvt_patientsites";
            }
        }
        else { //(G) - Provider
            siteObj = formContext.getAttribute("mcs_relatedprovidersite");
            sitesearch = "cvt_providersites";
        }

        if (siteObj != null)
            site = siteObj.getValue();
        if (site == null)
            return;

        var viewDisplayName = "Filtered by Site";
        var siteID = site[0].id;

        var siteName = MCS.cvt_Common.formatXML(site[0].name).trim();

        var lastInstanceOfOpenParenthesis = siteName.lastIndexOf('(');

        var sitenameWithoutStationNumber = lastInstanceOfOpenParenthesis === -1 ? siteName : siteName.substr(0, lastInstanceOfOpenParenthesis - 1);


        var fetchBase = '<fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="true"><entity name="cvt_resourcepackage"><attribute name="cvt_resourcepackageid"/><attribute name="cvt_name"/><attribute name="createdon"/>' +
            '<order attribute="cvt_name" descending="false"/><filter type="and"><condition attribute="statecode" operator="eq" value="0"/><condition attribute="' + sitesearch + '" operator="like" value="%' + sitenameWithoutStationNumber + '%"/>';

        if (serviceType != null) {
            viewDisplayName += " & Specialty";
            var serviceTypeID = serviceType[0].id;
            fetchBase += '<condition attribute="cvt_specialty" operator="eq" uiname="' + MCS.cvt_Common.formatXML(serviceType[0].name) + '" uitype="mcs_servicetype" value="' + serviceTypeID + '"/>';
            if (serviceSubType != null) {
                viewDisplayName += " & Specialty Sub-Type";
                var serviceSubTypeID = serviceSubType[0].id;
                fetchBase += '<condition attribute="cvt_specialtysubtype" operator="eq" uiname="' + MCS.cvt_Common.formatXML(serviceSubType[0].name) + '" uitype="mcs_servicesubtype" value="' + serviceSubTypeID + '"/>';
            }
        }
        if (schedulingPackagetype == true) {
            viewDisplayName += " & VA Video Connect";
            fetchBase += '<condition attribute="cvt_patientlocationtype" value="917290001" operator="eq"/>';
        }
        else {
            viewDisplayName += " & Clinic Based";
            fetchBase += '<filter type="or"><condition attribute="cvt_patientlocationtype" value="917290000" operator="eq"/><condition attribute="cvt_patientlocationtype" operator="null"/></filter>';
        }

        if (formContext.getAttribute("cvt_telehealthmodality").getValue() == true) {
            viewDisplayName += " & SFT";
            fetchBase += '<condition attribute="cvt_availabletelehealthmodality" value="917290001" operator="eq"/>';
        }
        else {
            viewDisplayName += " & CVT";
            fetchBase += '<filter type="or"><condition attribute="cvt_availabletelehealthmodality" value="917290000" operator="eq"/><condition attribute="cvt_availabletelehealthmodality" operator="null"/></filter>';
        }
        fetchBase += '<condition attribute="statuscode" value="1" operator="eq"/><condition attribute="cvt_groupappointment" value="' + groupAppt + '" operator="eq"/></filter>"';

        //Add associated PS
        fetchBase += '<link-entity name="cvt_participatingsite" alias="aa" to="cvt_resourcepackageid" from="cvt_resourcepackage"><filter type="and"><condition attribute="cvt_site" value="' + siteID + '" operator="eq" uitype="mcs_site" uiname="' + MCS.cvt_Common.formatXML(site[0].name) + '" /><condition attribute="statecode" value="0" operator="eq" />';

        //This is only commented out for testing.  Readd this in later
        //fetchBase += '<condition attribute="cvt_scheduleable" value="1" operator="eq" /> <condition attribute="cvt_relatedservice" operator="not-null" />';

        if (providers != null) {
            viewDisplayName += " & Provider";
            fetchBase += '<condition attribute="cvt_providers" value="' + '%' + MCS.cvt_Common.formatXML(providers[0].name) + '%' + '" operator="like"/>';
            //'<condition attribute="cvt_providers" value="%Smith%" operator="like" />
        }
        fetchBase += '</filter></link-entity></entity ></fetch>';

        var schedulingPackageLayoutXml = '<grid name="resultset" object="10010" jump="cvt_name" select="1" icon="0" preview="0"><row name="result" id="cvt_resourcepackageid">'
            + '<cell name="cvt_otherspecialtydetails" width="125"/><cell name="cvt_providersites" width="300"/><cell name="cvt_providersitevistaclinics" width="300"/><cell name="cvt_patientsites" width="300"/><cell name="cvt_patientsitevistaclinics" width="300"/>'
            + '<cell name="cvt_providers" width="300"/><cell name="cvt_name" width="300"/><cell name="cvt_groupappointment" width="125"/><cell name="cvt_patientsites" width="300"/><cell name="cvt_specialty" width="100"/><cell name="cvt_specialtysubtype" width="100"/><cell name="createdon" width="125"/></row></grid>';

        //Dynamically retrieve layout
        //  SELECT LayoutXml, * FROM[CVT_DEV2015_MSCRM].[dbo].[FilteredSavedQuery]         where Name = 'Active Scheduling Packages'
        var filter = " name eq 'Scheduling Package Lookup View'";
        //alert("About to retrieve the view layout.");

        Xrm.WebApi.retrieveMultipleRecords("SavedQuery", "?$select=layoutxml&$filter=" + filter).then(
            function success(result) {
                if (result && result != null && result.entities.length != 0 && result.entities[0].layoutxml != null) {
                    //"<grid name=\"resultset\" object=\"10010\" jump=\"cvt_name\" select=\"1\" icon=\"0\" preview=\"0\"><row name=\"result\" id=\"cvt_resourcepackageid\"><cell name=\"cvt_otherspecialtydetails\" width=\"125\"/><cell name=\"cvt_providersites\" width=\"300\"/><cell name=\"cvt_providersitevistaclinics\" width=\"300\"/><cell name=\"cvt_patientsites\" width=\"300\"/><cell name=\"cvt_patientsitevistaclinics\" width=\"300\"/><cell name=\"cvt_providers\" width=\"300\"/><cell name=\"cvt_name\" width=\"300\"/><cell name=\"cvt_groupappointment\" width=\"125\"/><cell name=\"cvt_patientsites\" width=\"300\"/><cell name=\"cvt_specialty\" width=\"100\"/><cell name=\"cvt_specialtysubtype\" width=\"100\"/><cell name=\"createdon\" width=\"125\"/></row></grid>"
                    //replace all of the \" with "
                    schedulingPackageLayoutXml = (result.entities[0].layoutxml).replace('\\', '');
                    //alert(schedulingPackageLayoutXml);
                }
            },
            function (error) {
                return;
            }
        );
        //var calls = CrmRestKit.ByQuery("SavedQuery", ['LayoutXml'], filter, false);
        //calls.fail(function (err) {
        //    return;
        //}).done(function (data) {
        //    if (data && data.d && data.d.results != null && data.d.results.length != 0 && data.d.results[0].LayoutXml != null) {
        //        //"<grid name=\"resultset\" object=\"10010\" jump=\"cvt_name\" select=\"1\" icon=\"0\" preview=\"0\"><row name=\"result\" id=\"cvt_resourcepackageid\"><cell name=\"cvt_otherspecialtydetails\" width=\"125\"/><cell name=\"cvt_providersites\" width=\"300\"/><cell name=\"cvt_providersitevistaclinics\" width=\"300\"/><cell name=\"cvt_patientsites\" width=\"300\"/><cell name=\"cvt_patientsitevistaclinics\" width=\"300\"/><cell name=\"cvt_providers\" width=\"300\"/><cell name=\"cvt_name\" width=\"300\"/><cell name=\"cvt_groupappointment\" width=\"125\"/><cell name=\"cvt_patientsites\" width=\"300\"/><cell name=\"cvt_specialty\" width=\"100\"/><cell name=\"cvt_specialtysubtype\" width=\"100\"/><cell name=\"createdon\" width=\"125\"/></row></grid>"
        //        //replace all of the \" with "
        //        schedulingPackageLayoutXml = (data.d.results[0].LayoutXml).replace('\\', '');

        //        //alert(schedulingPackageLayoutXml);
        //    }
        //});
        schedulingPackageControl.addCustomView(siteID, "cvt_resourcepackage", viewDisplayName, fetchBase, schedulingPackageLayoutXml, true);
    }
};

MCS.mcs_Service_Activity.GetProviderSite = function (executionContext) {
    var formContext = executionContext.getFormContext();
    var providerSite = formContext.getAttribute("mcs_relatedprovidersite");
    var schedulingPackage = formContext.getAttribute("cvt_relatedschedulingpackage").getValue();
    if (schedulingPackage == null) {
        alert('Please select SP');
        return;
    }

    if (providerSite != null) {
        var sft = formContext.getAttribute("cvt_telehealthmodality").getValue(); //Get the Resources values
        var resources = formContext.getAttribute("resources").getValue(); //Get the Resources values

        if (sft) {

            var fetchXml = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>";
            fetchXml += "<entity name='cvt_participatingsite'>";
            fetchXml += "    <attribute name='cvt_participatingsiteid' />";
            fetchXml += "    <attribute name='cvt_name' />";
            fetchXml += "    <attribute name='cvt_site' />";
            fetchXml += "    <order attribute='cvt_name' descending='false' />";
            fetchXml += "   <filter type='and'>";
            fetchXml += "      <condition attribute='cvt_resourcepackage' operator='eq' value='" + schedulingPackage[0].id + "' />";
            fetchXml += "      <condition attribute='cvt_locationtype' operator='eq' value='917290000' />";
            fetchXml += "      <condition attribute='cvt_scheduleable' operator='eq' value='1' />";
            fetchXml += "      <condition attribute='statecode' operator='eq' value='0' />";
            fetchXml += "    </filter>";
            fetchXml += "  </entity>";
            fetchXml += "</fetch>";

            fetchXml = "?fetchXml=" + encodeURIComponent(fetchXml);
            Xrm.WebApi.retrieveMultipleRecords("cvt_participatingsite", fetchXml).then(
                function success(resultSFTPS) {
                    var countValue = resultSFTPS.entities.length;
                    if (countValue > 0) {
                        var relatedSiteId = resultSFTPS.entities[0]._cvt_site_value;
                        var relatedSiteName = resultSFTPS.entities[0]["_cvt_site_value@OData.Community.Display.V1.FormattedValue"];
                        if (providerSite.getValue() == null)
                            MCS.mcs_Service_Activity.SetNewLookup(relatedSiteId, relatedSiteName, "mcs_site", providerSite);
                        return;
                    }
                    //No results
                    else {
                        alert('No Provider Site found.');
                    }
                },
                function (error) {
                    alert('Error occured while searching for valid participating site: ' + error.message);
                }
            );
        }
        else if (resources != null) {
            for (var i = 0; i < resources.length; i++) { //
                //alert(resources[i].name);
                if (resources[i].type == 4000) { //Equipment

                    //Retrieve the info from the Scheduling Package
                    Xrm.WebApi.retrieveRecord("Equipment", resources[i].id, "?$select=_mcs_relatedresource_value,name").then(
                        function success(resultEquipment) {
                            if (resultEquipment._mcs_relatedresource_value == null) {
                                //if (formContext.getAttribute("cvt_telehealthmodality").getValue()) {
                                //    if (result.Name.indexOf("SFT Technology") != -1) {
                                //        var splitString = result.Name.split("@");
                                //        if (splitString != null && splitString.length == 2) {
                                //            var siteName = splitString[1].trim();

                                //            var siteFetchXml = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>";
                                //            siteFetchXml += "   <entity name='mcs_site'>";
                                //            siteFetchXml += "   <attribute name='mcs_name'/>";
                                //            siteFetchXml += "       <filter type='and'>";
                                //            siteFetchXml += "            <condition attribute='mcs_name' operator='eq' value='" + siteName + "' />";
                                //            siteFetchXml += "       </filter>";
                                //            siteFetchXml += "   </entity>";
                                //            siteFetchXml += "</fetch>";

                                //            XrmSvcToolkit.fetch({
                                //                fetchXml: siteFetchXml,
                                //                async: false,
                                //                successCallback: function (result4) {
                                //                    if (result4.entities.length > 0) {
                                //                        var siteId = result4.entities[0].mcs_siteid;
                                //                        if (siteId != null && siteId != '') {
                                //                            var site = { Id: siteId, Name: result4.entities[0].mcs_name, LogicalName: 'mcs_site' };
                                //                            MCS.mcs_Service_Activity.SetLookup(site, providerSite);
                                //                            MCS.VIALogin.Login();
                                //                            return;
                                //                        }
                                //                    }
                                //                    else
                                //                        alert("Site could not be found.");
                                //                },
                                //                errorCallback: function (error) {
                                //                    throw error;
                                //                }
                                //            });
                                //        }
                                //    }
                                //}
                                //else
                                alert("Orphaned Resource has been scheduled: " + result.name + " with Id: " + result.id + ". Please fix this resource and rebuild the Scheduling Package (or just re-link the equipment with the TMP Resource).");
                            }
                            else {
                                Xrm.WebApi.retrieveRecord("mcs_resource", resultEquipment._mcs_relatedresource_value, "?$select=mcs_type,_cvt_defaultprovider_value").then(
                                    function success(resultResource) {
                                        if (resultResource.mcs_type != null && resultResource.mcs_type == 251920000) { //VistaClinic
                                            //We need to find the Provider Participating Site based on the VC being standalone or part of a PRG -> Scheduling Resource
                                            //Query Scheduling Resources where sr.cvt_resource.Id == resource.Id &&
                                            var fetchXml = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>";
                                            fetchXml += "<entity name='cvt_participatingsite'>";
                                            fetchXml += "    <attribute name='cvt_participatingsiteid' />";
                                            fetchXml += "    <attribute name='cvt_name' />";
                                            fetchXml += "    <attribute name='cvt_site' />";
                                            fetchXml += "    <order attribute='cvt_name' descending='false' />";
                                            fetchXml += "   <filter type='and'>";
                                            fetchXml += "      <condition attribute='cvt_resourcepackage' operator='eq' value='" + schedulingPackage[0].id + "' />";
                                            fetchXml += "      <condition attribute='cvt_locationtype' operator='eq' value='917290000' />";
                                            fetchXml += "      <condition attribute='cvt_scheduleable' operator='eq' value='1' />";
                                            fetchXml += "      <condition attribute='statecode' operator='eq' value='0' />";
                                            fetchXml += "    </filter>";
                                            fetchXml += "    <link-entity name='cvt_schedulingresource' from='cvt_participatingsite' to='cvt_participatingsiteid' alias='ab'>";
                                            fetchXml += "      <filter type='and'>";
                                            fetchXml += "        <condition attribute='cvt_tmpresource' operator='eq' value='{" + resultEquipment._mcs_relatedresource_value + "}' />";
                                            fetchXml += "      </filter>";
                                            fetchXml += "    </link-entity>";
                                            fetchXml += "  </entity>";
                                            fetchXml += "</fetch>";

                                            fetchXml = "?fetchXml=" + encodeURIComponent(fetchXml);

                                            Xrm.WebApi.retrieveMultipleRecords("cvt_participatingsite", fetchXml).then(
                                                function success(resultPS1) {
                                                    var countValue = resultPS1.entities.length;
                                                    if (countValue > 0) {
                                                        var relatedSiteId = resultPS1.entities[0]._cvt_site_value;
                                                        var relatedSiteName = resultPS1.entities[0]["_cvt_site_value@OData.Community.Display.V1.FormattedValue"];
                                                        if (providerSite.getValue() == null)
                                                            MCS.mcs_Service_Activity.SetNewLookup(relatedSiteId, relatedSiteName, "mcs_site", providerSite);

                                                        //Set the provider value on the service appointment form from the clinic default provider
                                                        if (resultResource._cvt_defaultprovider_value != null) {
                                                            var provider = formContext.getAttribute("cvt_relatedproviderid");
                                                            var providerValue = formContext.getAttribute("cvt_relatedproviderid").getValue();
                                                            if (providerValue == null) {
                                                                MCS.mcs_Service_Activity.SetNewLookup(resultResource._cvt_defaultprovider_value, resultResource["_cvt_defaultprovider_value@OData.Community.Display.V1.FormattedValue"], "SystemUser", provider);
                                                            }
                                                        }
                                                        return;
                                                    }
                                                    //No results
                                                },
                                                function (error) {
                                                    alert('Error occured while searching for valid participating site: ' + error.message);
                                                }
                                            );

                                            //If it wasn't a standalone VC, let's look within PRGs
                                            //Query Scheduling Resources where sr.cvt_resource.Id == resource.Id &&
                                            fetchXml = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>";
                                            fetchXml += "  <entity name='cvt_participatingsite'>";
                                            fetchXml += "    <attribute name='cvt_participatingsiteid' />";
                                            fetchXml += "    <attribute name='cvt_name' />";
                                            fetchXml += "    <attribute name='cvt_site' />";
                                            fetchXml += "    <order attribute='cvt_name' descending='false' />";
                                            fetchXml += "    <filter type='and'>";
                                            fetchXml += "      <condition attribute='cvt_resourcepackage' operator='eq' value='" + schedulingPackage[0].id + "' />";
                                            fetchXml += "      <condition attribute='cvt_locationtype' operator='eq' value='917290000' />";
                                            fetchXml += "      <condition attribute='cvt_scheduleable' operator='eq' value='1' />";
                                            fetchXml += "      <condition attribute='statecode' operator='eq' value='0' />";
                                            fetchXml += "    </filter>";
                                            fetchXml += "    <link-entity name='cvt_schedulingresource' from='cvt_participatingsite' to='cvt_participatingsiteid' alias='ac'>";
                                            fetchXml += "      <filter type='and'>";
                                            fetchXml += "        <condition attribute='cvt_tmpresourcegroup' operator='not-null' />";
                                            fetchXml += "        <condition attribute='statecode' operator='eq' value='0' />";
                                            fetchXml += "      </filter>";
                                            fetchXml += "      <link-entity name='mcs_resourcegroup' from='mcs_resourcegroupid' to='cvt_tmpresourcegroup' alias='ad'>";
                                            fetchXml += "        <filter type='and'>";
                                            fetchXml += "          <condition attribute='statecode' operator='eq' value='0' />";
                                            fetchXml += "        </filter>";
                                            fetchXml += "        <link-entity name='mcs_groupresource' from='mcs_relatedresourcegroupid' to='mcs_resourcegroupid' alias='ae'>";
                                            fetchXml += "          <filter type='and'>";
                                            fetchXml += "            <condition attribute='mcs_relatedresourceid' operator='eq' value='{" + resultEquipment._mcs_relatedresource_value + "}' />";
                                            fetchXml += "            <condition attribute='statecode' operator='eq' value='0' />";
                                            fetchXml += "          </filter>";
                                            fetchXml += "        </link-entity>";
                                            fetchXml += "      </link-entity>";
                                            fetchXml += "    </link-entity>";
                                            fetchXml += "  </entity>";
                                            fetchXml += "</fetch>";

                                            fetchXml = "?fetchXml=" + encodeURIComponent(fetchXml);
                                            Xrm.WebApi.retrieveMultipleRecords("cvt_participatingsite", fetchXml).then(
                                                function success(resultPS2) {
                                                    var countValue = resultPS2.entities.length;
                                                    if (countValue > 0) {
                                                        var relatedSiteId = resultPS2.entities[0]._cvt_site_value;
                                                        var relatedSiteName = resultPS2.entities[0]["_cvt_site_value@OData.Community.Display.V1.FormattedValue"];
                                                        if (providerSite.getValue() == null)
                                                            MCS.mcs_Service_Activity.SetNewLookup(relatedSiteId, relatedSiteName, "mcs_site", providerSite);

                                                        //Set the provider value on the service appointment form from the clinic default provider
                                                        if (resultResource._cvt_defaultprovider_value != null) {
                                                            var provider = formContext.getAttribute("cvt_relatedproviderid");
                                                            var providerValue = formContext.getAttribute("cvt_relatedproviderid").getValue();
                                                            if (providerValue == null) {
                                                                MCS.mcs_Service_Activity.SetNewLookup(resultResource._cvt_defaultprovider_value, resultResource["_cvt_defaultprovider_value@OData.Community.Display.V1.FormattedValue"], "SystemUser", provider);
                                                            }
                                                        }
                                                        return;
                                                    }
                                                    //No results
                                                },
                                                function (error) {
                                                    alert('Error occured while searching for valid participating site: ' + error.message);
                                                }
                                            );
                                        }
                                    },
                                    function (error1) {
                                        alert('Error occured while retrieving Resource: ' + error1.message);
                                    }
                                );

                                //var resource = CrmRestKit.Retrieve('mcs_resource', result.mcs_relatedresource.Id, ['mcs_name', 'mcs_Type', 'cvt_primarystopcode', 'mcs_RelatedSiteId', 'cvt_defaultprovider'], true).fail(
                                //    function (err) {
                                //        alert('Error occured while retrieving Resource: ' + err);
                                //    }).done(
                                //        function (data) {
                                //            if (result.mcs_Type != null && result.mcs_Type.Value == 251920000) { //VistaClinic
                                //                if (result.cvt_primarystopcode == "179" || result.cvt_primarystopcode == "693" || result.cvt_primarystopcode == "692") {
                                //                    var fetchXml = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false' aggregate='true'>";
                                //                    fetchXml += "<entity name='cvt_participatingsite'>";
                                //                    fetchXml += "<attribute name='cvt_name' alias='recordcount' aggregate='count' />";
                                //                    fetchXml += "        <filter type='and'>";
                                //                    fetchXml += "            <condition attribute='cvt_resourcepackage' operator='eq' value='" + schedulingPackage[0].id + "' />";
                                //                    fetchXml += "            <condition attribute='cvt_locationtype' operator='eq' value='917290000' />";
                                //                    fetchXml += "            <condition attribute='cvt_scheduleable' operator='eq' value='1' />";
                                //                    fetchXml += "            <condition attribute='statecode' operator='eq' value='0' />";
                                //                    fetchXml += "</filter>";
                                //                    fetchXml += "</entity>";
                                //                    fetchXml += "</fetch>";

                                //                    XrmSvcToolkit.fetch({
                                //                        fetchXml: fetchXml,
                                //                        async: false,
                                //                        successCallback: function (result) {
                                //                            countValue = result.entities[0].recordcount;
                                //                            if (countValue > 0) {
                                //                                var relatedSite = result.mcs_RelatedSiteId;
                                //                                if (providerSite.getValue() == null)
                                //                                    MCS.mcs_Service_Activity.SetLookup(relatedSite, providerSite);

                                //                                //Set the provider value on the service appointment form from the clinic default provider
                                //                                if (result.cvt_defaultprovider != null) {
                                //                                    var provider = formContext.getAttribute("cvt_relatedproviderid");
                                //                                    var defaultProvider = result.cvt_defaultprovider;
                                //                                    MCS.mcs_Service_Activity.SetLookup(defaultProvider, provider);
                                //                                }
                                //                                MCS.VIALogin.Login();
                                //                                return;
                                //                            }
                                //                        },
                                //                        errorCallback: function (error) {
                                //                            throw error;
                                //                        }
                                //                    });
                                //                }
                                //            }
                                //        });
                            }
                        },
                        function (error) {
                            alert('Error occured while retrieving equipment: ' + error.message);
                        }
                    );

                    ////Retrieve the info from the Scheduling Package
                    //CrmRestKit.Retrieve('Equipment', resources[i].id, ['mcs_relatedresource', 'Name'], false).fail(
                    //    function (err) {
                    //        alert('Error occured while retrieving equipment: ' + err);
                    //    }).done(
                    //        function (data) {
                    //            if (result.mcs_relatedresource.Id == null) {
                    //                if (Xrm.Page.getAttribute("cvt_telehealthmodality").getValue()) {
                    //                    if (data.d.Name.indexOf("SFT Technology") != -1) {
                    //                        var splitString = data.d.Name.split("@");
                    //                        if (splitString != null && splitString.length == 2) {
                    //                            var siteName = splitString[1].trim();

                    //                            var siteFetchXml = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>";
                    //                            siteFetchXml += "   <entity name='mcs_site'>";
                    //                            siteFetchXml += "   <attribute name='mcs_name'/>";
                    //                            siteFetchXml += "       <filter type='and'>";
                    //                            siteFetchXml += "            <condition attribute='mcs_name' operator='eq' value='" + siteName + "' />";
                    //                            siteFetchXml += "       </filter>";
                    //                            siteFetchXml += "   </entity>";
                    //                            siteFetchXml += "</fetch>";

                    //                            XrmSvcToolkit.fetch({
                    //                                fetchXml: siteFetchXml,
                    //                                async: false,
                    //                                successCallback: function (result) {
                    //                                    if (result.entities.length > 0) {
                    //                                        var siteId = result.entities[0].mcs_siteid;
                    //                                        if (siteId != null && siteId != '') {
                    //                                            var site = { Id: siteId, Name: result.entities[0].mcs_name, LogicalName: 'mcs_site' };
                    //                                            MCS.mcs_Service_Activity.SetLookup(site, providerSite);
                    //                                            MCS.VIALogin.Login();
                    //                                            return;
                    //                                        }
                    //                                    }
                    //                                    else
                    //                                        alert("Site could not be found.");
                    //                                },
                    //                                errorCallback: function (error) {
                    //                                    throw error;
                    //                                }
                    //                            });
                    //                        }
                    //                    }
                    //                }
                    //                else
                    //                    alert("Orphaned Resource has been scheduled: " + data.d.name + " with Id: " + data.d.id + ". Please fix this resource and rebuild the Scheduling Package (or just re-link the equipment with the TMP Resource).");
                    //            }
                    //            else {

                    //                var resource = CrmRestKit.Retrieve('mcs_resource', data.d.mcs_relatedresource.Id, ['mcs_name', 'mcs_Type', 'cvt_primarystopcode', 'mcs_RelatedSiteId', 'cvt_defaultprovider'], true).fail(
                    //                    function (err) {
                    //                        alert('Error occured while retrieving Resource: ' + err);
                    //                    }).done(
                    //                        function (data) {
                    //                            if (data.d.mcs_Type != null && data.d.mcs_Type.Value == 251920000) { //VistaClinic
                    //                                if (data.d.cvt_primarystopcode == "179" || data.d.cvt_primarystopcode == "693" || data.d.cvt_primarystopcode == "692") {
                    //                                    var fetchXml = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false' aggregate='true'>";
                    //                                    fetchXml += "<entity name='cvt_participatingsite'>";
                    //                                    fetchXml += "<attribute name='cvt_name' alias='recordcount' aggregate='count' />";
                    //                                    fetchXml += "        <filter type='and'>";
                    //                                    fetchXml += "            <condition attribute='cvt_resourcepackage' operator='eq' value='" + schedulingPackage[0].id + "' />";
                    //                                    fetchXml += "            <condition attribute='cvt_locationtype' operator='eq' value='917290000' />";
                    //                                    fetchXml += "            <condition attribute='cvt_scheduleable' operator='eq' value='1' />";
                    //                                    fetchXml += "            <condition attribute='statecode' operator='eq' value='0' />";
                    //                                    fetchXml += "</filter>";
                    //                                    fetchXml += "</entity>";
                    //                                    fetchXml += "</fetch>";

                    //                                    XrmSvcToolkit.fetch({
                    //                                        fetchXml: fetchXml,
                    //                                        async: false,
                    //                                        successCallback: function (result) {
                    //                                            countValue = result.entities[0].recordcount;
                    //                                            if (countValue > 0) {
                    //                                                var relatedSite = data.d.mcs_RelatedSiteId;
                    //                                                if (providerSite.getValue() == null)
                    //                                                    MCS.mcs_Service_Activity.SetLookup(relatedSite, providerSite);

                    //                                                //Set the provider value on the service appointment form from the clinic default provider
                    //                                                if (data.d.cvt_defaultprovider != null) {
                    //                                                    var provider = Xrm.Page.getAttribute("cvt_relatedproviderid");
                    //                                                    var defaultProvider = data.d.cvt_defaultprovider;
                    //                                                    MCS.mcs_Service_Activity.SetLookup(defaultProvider, provider);
                    //                                                }
                    //                                                MCS.VIALogin.Login();
                    //                                                return;
                    //                                            }
                    //                                        },
                    //                                        errorCallback: function (error) {
                    //                                            throw error;
                    //                                        }
                    //                                    });
                    //                                }
                    //                            }
                    //                        });
                    //            }
                    //        });
                }
                else if (resources[i].type == 8) {//user
                    Xrm.WebApi.retrieveRecord("SystemUser", resources[i].id, "?$select=systemuserid,fullname").then(
                        function success(resultUser) {
                            if (resultUser.systemuserid != null) {
                                var provider = formContext.getAttribute("cvt_relatedproviderid");
                                MCS.mcs_Service_Activity.SetNewLookup(resultUser.systemuserid, resultUser.fullname, "SystemUser", provider);
                            }
                        },
                        function (error) {
                            alert('Error occured while retrieving user: ' + error.message);
                        }
                    );
                }
            }
        }
    }
    else
        MCS.VIALogin.Login();
}

/*MCS.mcs_Service_Activity.SetNewLookup = function (value, Name, LogicalName, targetField) {
    //debugger;
    if (targetField != null) {
        var obj = { id: value, entityType: LogicalName, name: Name }
        if (obj.name == null)
            targetField.setValue(null);
        else
            targetField.setValue([obj]);
    }
}*/

//Description: CreateName for Service Activity Subject
MCS.mcs_Service_Activity.CreateName = function (executionContext) {
    var formContext = executionContext.getFormContext();
    var serviceType = formContext.getAttribute("mcs_servicetype").getValue();
    var serviceSubType = formContext.getAttribute("mcs_servicesubtype").getValue();
    var groupApptOption = formContext.getAttribute("mcs_groupappointment").getValue();
    var derivedResultField = "";
    if (serviceType != null)
        derivedResultField += serviceType[0].name + " ";
    if (serviceSubType != null)
        derivedResultField += " : " + serviceSubType[0].name + " ";
    if (groupApptOption == 1)
        derivedResultField += "Group Appointment";

    formContext.getAttribute("subject").setValue(derivedResultField);
};

//If group appointment, show pro site and patient rooms,
MCS.mcs_Service_Activity.GroupAppt = function (executionContext) {
    var formContext = executionContext.getFormContext();
    var groupAppt = formContext.getAttribute("mcs_groupappointment").getValue();
    var patSite = formContext.getControl("mcs_relatedsite");
    var proSite = formContext.getControl("mcs_relatedprovidersite");

    var patRoomsTab = formContext.ui.tabs.get("tab_groupscheduling");
    //    var grouppatientsTab = Xrm.Page.ui.tabs.get("tab_grouppatients");

    var teleModala = formContext.getAttribute("cvt_telehealthmodality");
    var teleModalc = formContext.getControl("cvt_telehealthmodality");
    var isHomeMobile = formContext.getAttribute("cvt_type").getValue();

    if (groupAppt == true) {
        proSite.setVisible(true);
        patSite.setVisible(false);
        patRoomsTab.setVisible(!isHomeMobile);
        formContext.getControl("customers").setDisabled(!isHomeMobile);

        formContext.getAttribute("mcs_relatedsite").setValue(null);
        formContext.getAttribute("mcs_relatedprovidersite").setRequiredLevel("required");
        formContext.getAttribute("mcs_relatedsite").setRequiredLevel("none");

        var isVVC = formContext.getAttribute("cvt_type").getValue() == true;

        if (!isVVC) {
            teleModala.setValue(false);
            teleModala.fireOnChange();
            teleModalc.setVisible(false);
        }
    }
    else {
        proSite.setVisible(false);
        patSite.setVisible(true);
        patRoomsTab.setVisible(false);
        formContext.getAttribute("mcs_relatedprovidersite").setRequiredLevel("none");
        formContext.getAttribute("mcs_relatedsite").setRequiredLevel("required");
        //if (Xrm.Page.getAttribute("cvt_type").getValue() != true) {
        //    Xrm.Page.getAttribute("mcs_relatedprovidersite").setValue(null);
        //}
        teleModalc.setVisible(true);
        teleModala.fireOnChange();
    }
};

//Group SA - add PatSide AR ResSpec to Resources field
MCS.mcs_Service_Activity.GroupPat = function (patValues, executionContext) {
    //debugger;
    var formContext = executionContext.getFormContext();
    if (formContext.getAttribute("mcs_groupappointment").getValue() == true && formContext.ui.getFormType() == 1) {
        if (patValues != null) {
            //break result into an array
            var resultArray = patValues.split("|");
            if (resultArray.length == 3) {
                //Turn it into an object
                var groupActivityParty = [];
                groupActivityParty[0] = {
                    id: resultArray[0],
                    resouceSpecId: resultArray[2],
                    typeName: resultArray[1],
                    entityType: resultArray[1],
                    name: "Search for All Resources"
                };

                //Set the object to the Resouce field
                formContext.getAttribute("resources").setValue(groupActivityParty);
            }
        }
    }
};
MCS.mcs_Service_Activity.ClearResources = function (executionContext) {
    var formContext = executionContext.getFormContext();
    var schedulingPackage = formContext.getAttribute("cvt_relatedschedulingpackage").getValue();
    if (schedulingPackage == null) {
        formContext.getAttribute("resources").setValue(null);
        formContext.getAttribute("resources").setSubmitMode();
    }
};

//If resources are on Service Activity, display the scheduling tab, otherwise hide it
MCS.mcs_Service_Activity.SchedulingInfo = function (executionContext) {
    var formContext = executionContext.getFormContext();
    var resources = formContext.getAttribute("resources").getValue();
    var schedulingTab = formContext.ui.tabs.get("tab_scheduling").setVisible(resources.length > 0);
};

//TO DO: explore why this is running onSave - probably need to remove this
MCS.mcs_Service_Activity.filterSubGrid = function () {
    // var formContext = executionContext.getFormContext();
    var PatRoomsGrid = document.getElementById("PatientRooms"); //grid to filter 
    if (PatRoomsGrid == null) { //make sure the grid has loaded 
        setTimeout(function () { MCS.mcs_Service_Activity.filterSubGrid(); }, 500); //if the grid hasn’t loaded run this again when it has 
        return;
    }

    var schedulingPackageValue = Xrm.Page.getAttribute("cvt_relatedschedulingpackage").getValue(); //field to filter by 
    var schedulingPackageId = "00000000-0000-0000-0000-000000000000"; //if filter field is null display nothing 

    if (schedulingPackageValue != null)
        var schedulingPackageId = schedulingPackageValue[0].id;

    //fetch xml code which will retrieve all Pat Sites Related to this Service Activity.  
    var fetchXml =
        "<?xml version='1.0'?>" +
        "<fetch distinct='false' mapping='logical' output-format='xml-platform' version='1.0'>" +
        "<entity name='cvt_patientresourcegroup'>" +
        "<attribute name='cvt_roomcapacity'/>" +
        "<attribute name='cvt_type'/>" +
        "<attribute name='cvt_name'/>" +
        "<attribute name='cvt_patientresourcegroupid'/>" +
        "<order descending='false' attribute='cvt_name'/>" +
        "<filter type='and'>" +
        "<condition attribute='cvt_type' value='251920001' operator='eq'/>" +
        "<condition attribute='cvt_relatedtsaid' value='" + schedulingPackageId + "' operator='eq'/>" +
        "<condition attribute='statecode' value='0' operator='eq'/>" +
        "</filter>" +
        "</entity>" +
        "</fetch>";

    PatRoomsGrid.control.SetParameter("fetchXml", fetchXml); //set the fetch xml to the sub grid   
    PatRoomsGrid.control.Refresh(); //refresh the sub grid using the new fetch xml 
};

MCS.mcs_Service_Activity.EnforceChanges = function (executionContext) {
    var formContext = executionContext.getFormContext();
    formContext.getAttribute("resources").setSubmitMode("always");
    formContext.getAttribute("serviceid").setSubmitMode("always");
    formContext.getAttribute("scheduledstart").setSubmitMode("always");
    formContext.getAttribute("scheduledend").setSubmitMode("always");
    formContext.getAttribute("scheduleddurationminutes").setSubmitMode("always");
};

MCS.mcs_Service_Activity.CVTtoHome = function (executionContext) {
    var formContext = executionContext.getFormContext();
    var type = formContext.getAttribute("cvt_type").getValue();
    var typec = formContext.getControl("cvt_type");
    if (formContext.ui.getFormType() == MCS.mcs_Service_Activity.FORM_TYPE_CREATE) {
        typec.setDisabled(false);
    }
    var groupAppta = formContext.getAttribute("mcs_groupappointment");
    var groupApptc = formContext.getControl("mcs_groupappointment");

    var patSitea = formContext.getAttribute("mcs_relatedsite");
    var patSitec = formContext.getControl("mcs_relatedsite");

    var proSitea = formContext.getAttribute("mcs_relatedprovidersite");
    var proSitec = formContext.getControl("mcs_relatedprovidersite");

    var patients = formContext.getControl("customers");
    var patRoomsTab = formContext.ui.tabs.get("tab_groupscheduling");
    var grouppatientsTab = formContext.ui.tabs.get("tab_grouppatients");

    var teleModala = formContext.getAttribute("cvt_telehealthmodality");
    var teleModalc = formContext.getControl("cvt_telehealthmodality");

    var patientResourcesRequiredControl = formContext.getControl("cvt_patientsiteresourcesrequired");

    if (type == true) { //Cvt to VA Video Visit
        patientResourcesRequiredControl.setVisible(true);

        groupApptc.setVisible(true);
        if (groupAppta.getValue())
            formContext.getControl("customers").setDisabled(false);

        teleModala.setValue(false);
        teleModala.fireOnChange();
        teleModalc.setVisible(false);

        proSitec.setVisible(true);
        proSitea.setRequiredLevel("required");

        patSitea.setValue(null);
        patSitea.setRequiredLevel("none");
        patSitec.setVisible(false);

        patRoomsTab.setVisible(false);
    }
    else { //VA Video Visit to Clinic based
        patientResourcesRequiredControl.setVisible(false);

        if (formContext.ui.getFormType() === MCS.mcs_Service_Activity.FORM_TYPE_CREATE) {
            proSitea.setValue(null);
        }

        proSitec.setVisible(false);
        proSitea.setRequiredLevel("none");

        patSitec.setVisible(true);
        patSitea.setRequiredLevel("required");

        groupApptc.setVisible(true);
        groupAppta.fireOnChange();

        teleModalc.setVisible(true);
        teleModala.fireOnChange();
    }
};

MCS.mcs_Service_Activity.HandlePatientSiteResourcesRequiredChange = function (executionContext) {
    var formContext = executionContext.getFormContext();
    if (formContext.getAttribute("cvt_patientsiteresourcesrequired").getValue()) {
        formContext.getControl("mcs_relatedsite").setVisible(true);
        formContext.getAttribute("mcs_relatedsite").setRequiredLevel('required');
        formContext.getControl("mcs_relatedprovidersite").setVisible(false);
        formContext.getAttribute("mcs_relatedprovidersite").setRequiredLevel('none');

        if (formContext.ui.getFormType() === MCS.mcs_Service_Activity.FORM_TYPE_CREATE) {
            formContext.getAttribute("mcs_relatedsite").setValue(null);
            formContext.getAttribute("mcs_relatedprovidersite").setValue(null);
            formContext.getAttribute("serviceid").setValue(null);
            formContext.getAttribute("cvt_relatedschedulingpackage").setValue(null);
            formContext.getAttribute("mcs_servicetype").setValue(null);
        }
    }
    else {
        formContext.getControl("mcs_relatedsite").setVisible(false);
        formContext.getAttribute("mcs_relatedsite").setRequiredLevel('none');
        formContext.getControl("mcs_relatedprovidersite").setVisible(true);
        formContext.getAttribute("mcs_relatedprovidersite").setRequiredLevel('required');

        if (formContext.ui.getFormType() === MCS.mcs_Service_Activity.FORM_TYPE_CREATE) {
            formContext.getAttribute("mcs_relatedsite").setValue(null);
            formContext.getAttribute("mcs_relatedprovidersite").setValue(null);
            formContext.getAttribute("serviceid").setValue(null);
            formContext.getAttribute("cvt_relatedschedulingpackage").setValue(null);
            formContext.getAttribute("mcs_servicetype").setValue(null);
        }
    }
};

MCS.mcs_Service_Activity.BlockAddPatient = function (executionContext) {
    var formContext = executionContext.getFormContext();
    var patientObj = formContext.getAttribute("customers");
    var patients = patientObj != null ? patientObj.getValue() : [];
    var newPatsAdded = MCS.mcs_Service_Activity.compareArrays(MCS.Patients, patients);
    if (newPatsAdded.length == 0) {
        //Determine if we want to re-build the cached patients list after removing a veteran - 
        //scenario: if you remove a veteran - click away - and then try to add a new one, should the one that got removed get added back or not?  
        //    MCS.Patients = Xrm.Page.getAttribute("optionalattendees").getValue();
        return;
    }
    else {
        alert("You can only add patients through the Patient Search.  Not adding: " + newPatsAdded);
        patientObj.setValue(MCS.Patients);
    }
};

MCS.mcs_Service_Activity.compareArrays = function (cachedPatients, newPatients) {
    var newPats = [];
    var newPatString = "";
    if (newPatients == null)
        return "";
    for (var i in newPatients) {
        var alreadyExists = false;
        var newPatIdObj = newPatients[i];
        if (newPatIdObj != null) {
            var newPatId = MCS.cvt_Common.TrimBookendBrackets(newPatIdObj.id.toLowerCase());
            for (var j in cachedPatients) {
                var cachedPatientObj = cachedPatients[j];
                var cachedPatientObjId = MCS.cvt_Common.TrimBookendBrackets(cachedPatientObj.id.toLowerCase());
                if (cachedPatientObj != null && newPatId == cachedPatientObjId) {
                    alreadyExists = true;
                    break;
                }
            }
        }
        if (!alreadyExists) {
            newPats.push(newPatIdObj.name);
        }
    }
    if (newPats.length > 0) {

        for (var pat in newPats) {
            newPatString += newPats[pat] + "; ";
        }
        newPatString = newPatString.substr(0, newPatString.length - 2)
    }
    return newPatString;
};