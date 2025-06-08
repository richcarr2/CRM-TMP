//If the SDK namespace object is not defined, create it.
if (typeof (MCS) === "undefined") { MCS = {}; }
// Create Namespace container for functions in this library;
MCS.GroupResource = {};

///Return the view of only resources of that Type and Site.
MCS.GroupResource.HandleOnChangeLookup = function (executionContext) {
    'use strict';


    var formContext = executionContext.getFormContext();
    var site = formContext.getAttribute("mcs_relatedsiteid").getValue();
    var type = formContext.getAttribute("mcs_type").getValue();
    if (type != null && site != null) {

        //Get the Facility of the Site - for the User view
        var facilityId = null;;

        Xrm.WebApi.retrieveRecord("mcs_site", formContext.getAttribute('mcs_relatedsiteid').getValue()[0].id, "?$select=mcs_FacilityId").then(
            function success(result) {
                if (result.mcs_FacilityId != null) {
                    facilityId = result.mcs_FacilityId;
                }
            },
            function (error) {
                //console.log(error.message);
                // handle error conditions
            }
        );

        /*
        //Get the Facility of the Site - for the User view
        var facilityId = null;;
        var SiteCall = CrmRestKit.Retrieve("mcs_site", Xrm.Page.getAttribute('mcs_relatedsiteid').getValue()[0].id, ['mcs_FacilityId'], false);
        SiteCall.done(function (data) {
            if (data && data.d) {
                if (data.d.mcs_FacilityId != null) {
                    facilityId = data.d.mcs_FacilityId;
                }
            }
        }).fail(
                function (error) {
                });
        */

        var ResourceControl = formContext.getControl("mcs_relatedresourceid");
        var ResourceViewID = site[0].id;
        var ResourceViewName = "Filtered by Site and Type";

        var UserControl = formContext.getControl("mcs_relateduserid");
        var UserViewID = "{47127E79-C3A2-E211-9B92-00155D144F0F}";
        var UserViewName = "Filtered By Site and Type (Provider/Clinician)";

        var ResourceColumns = [
            'mcs_resourceid',
            'mcs_name',
            'createdon'
        ];
        var ResourceConditions = [
            '<condition attribute="statecode" operator="eq" value="0"/>',
            '<condition attribute="mcs_relatedsiteid" operator="eq" uiname="' + MCS.cvt_Common.formatXML(site[0].name) + '" uitype="mcs_site" value="' + site[0].id + '"/>'
        ];
        var ResourceOrder = [
            'mcs_name',
            false
        ];
        var ResourceXmlLayout = '<grid name="resultset" object="10006" jump="mcs_name" select="1" icon="0" preview="0"><row name="result" id="mcs_resourceid"><cell name="mcs_name" width="200" /><cell name="mcs_relatedsiteid" width="125" /><cell name="mcs_type" width="125" /></row></grid>';
        var UserColumns = [
            'fullname',
            'title',
            'address1_telephone1',
            'businessunitid',
            'cvt_type',
            'systemuserid'
        ];
        var UserOrder = [
            'fullname',
            false
        ];
        var UserConditions = [
            '<condition attribute="isdisabled" operator="eq" value="0"/>',
            '<condition attribute="accessmode" operator="ne" value="3"/>',
            '<condition attribute="cvt_type" operator="eq" value="917290001"/>',
            '<condition attribute="cvt_site" operator="eq" uiname="' + MCS.cvt_Common.formatXML(site[0].name) + '" uitype="mcs_site" value="' + site[0].id + '"/>'
        ];

        var UserXmlLayout = '<grid name="resultset" object="8" jump="fullname" select="1" icon="0" preview="0"><row name="result" id="systemuserid"><cell name="fullname" width="200" /><cell name="cvt_type" width="125" /><cell name="businessunitid" width="150" /><cell name="title" width="100" /><cell name="address1_telephone1" width="100" /></row></grid>';

        if (type === 917290000) //All Required 
        {
            ResourceViewName = 'Filtered by Site (All Required)';
            //if TSS Res Group is patient, then change user filter to Telepresenter
            if (formContext.getAttribute('mcs_relatedresourcegroupid').getValue() != null) {

                Xrm.WebApi.retrieveRecord("mcs_resourcegroup", formContext.getAttribute('mcs_relatedresourcegroupid').getValue()[0].id, "?$select=cvt_location").then(
                    function success(result) {
                        //If TMP Resource Group location = Patient, then filter instead by TCT or Telepresenter/Imager
                        if (result.cvt_location !== null && result.cvt_location.Value === 917290001) {
                            //UserConditions[2] = '<condition attribute="cvt_type" operator="eq" value="917290000"/>';
                            UserConditions[2] = '<condition attribute="cvt_type" operator="in"><value>917290005</value><value>917290000</value></condition>';
                            UserViewName = "Filtered By Site and Type (TCT or Telepresenter/Imager)";
                        }
                    },
                    function (error) {
                        //console.log(error.message);
                        // handle error conditions
                    }
                );

                /*
                                var calls = CrmRestKit.Retrieve("mcs_resourcegroup", Xrm.Page.getAttribute('mcs_relatedresourcegroupid').getValue()[0].id, ['cvt_location'], false);
                                calls.done(function (data) {
                                    if (data && data.d) {
                                        //If TMP Resource Group location = Patient, then filter instead by TCT or Telepresenter/Imager
                                        if (data.d.cvt_location != null && data.d.cvt_location.Value == 917290001) {
                                            //UserConditions[2] = '<condition attribute="cvt_type" operator="eq" value="917290000"/>';
                                            UserConditions[2] = '<condition attribute="cvt_type" operator="in"><value>917290005</value><value>917290000</value></condition>';
                                            UserViewName = "Filtered By Site and Type (TCT or Telepresenter/Imager)";
                                        }
                                    }
                                }).fail(
                                        function (error) {
                                    });
                                */
            }
        }
        else {
            if (type === 100000000) //Telepresenter
            {
                //UserConditions[2] = '<condition attribute="cvt_type" operator="eq" value="917290000"/>';
                UserConditions[2] = '<condition attribute="cvt_type" operator="in"><value>917290005</value><value>917290000</value></condition>';
                UserViewName = "Filtered By Site and Type (TCT or Telepresenter/Imager)";
            }
            ResourceConditions[2] = '<condition attribute="mcs_type" operator="eq" value="' + type + '"/>';
        }

        //Switch out to Facility for User View
        if (facilityId != null) {
            UserConditions[3] = '<condition attribute="cvt_facility" operator="eq" uiname="' + MCS.cvt_Common.formatXML(facilityId.name) + '" uitype="cvt_facility" value="' + facilityId.Id + '"/>';
            UserViewName = UserViewName.replace("Site", "Facility");
        }
        var FetchBase = MCS.cvt_Common.CreateFetch('systemuser', UserColumns, UserConditions, UserOrder);
        UserControl.addCustomView(UserViewID, "systemuser", UserViewName, FetchBase, UserXmlLayout, true);

        FetchBase = MCS.cvt_Common.CreateFetch('mcs_resource', ResourceColumns, ResourceConditions, ResourceOrder);
        ResourceControl.addCustomView(ResourceViewID, "mcs_resource", ResourceViewName, FetchBase, ResourceXmlLayout, true);
    }
};

//Check Type; Sets Visibility and Requirements.
MCS.GroupResource.ShowDetails = function (executionContext) {
    //debugger;
    var formContext = executionContext.getFormContext();
    var siteFieldValue = formContext.getAttribute("mcs_relatedsiteid").getValue();
    var typeDisable = true;

    switch (formContext.getAttribute("mcs_type").getValue()) {
        case 251920001: //Room
        case 251920002: //Technology
        case 251920000: //VistA Clinic
            formContext.getControl("mcs_relatedresourceid").setVisible(true);
            formContext.getControl("mcs_relatedresourceid").setFocus();
            formContext.getAttribute("mcs_relatedresourceid").setRequiredLevel("required");

            formContext.getControl("mcs_relateduserid").setVisible(false);
            formContext.getAttribute("mcs_relateduserid").setValue(null);
            formContext.getAttribute("mcs_relateduserid").setRequiredLevel("none");
            break;
        case 100000000: //Telepresenter
        case 99999999: //Provider
            formContext.getControl("mcs_relateduserid").setVisible(true);
            formContext.getControl("mcs_relateduserid").setFocus();
            formContext.getAttribute("mcs_relateduserid").setRequiredLevel("required");

            formContext.getControl("mcs_relatedresourceid").setVisible(false);
            formContext.getAttribute("mcs_relatedresourceid").setValue(null);
            formContext.getAttribute("mcs_relatedresourceid").setRequiredLevel("none");
            break;
        case 917290000: //All Required
            formContext.getControl("mcs_relateduserid").setVisible(true);
            formContext.getAttribute("mcs_relateduserid").setRequiredLevel("none");
            formContext.getControl("mcs_relatedresourceid").setVisible(true);
            formContext.getAttribute("mcs_relatedresourceid").setRequiredLevel("none");

            if (formContext.getAttribute("mcs_relatedresourceid").getValue() != null) {
                formContext.getAttribute("mcs_relateduserid").setValue(null);
                formContext.getAttribute("mcs_relateduserid").setRequiredLevel("none");
                formContext.getControl("mcs_relateduserid").setVisible(false);
            }
            if (formContext.getAttribute("mcs_relateduserid").getValue() != null) {
                formContext.getAttribute("mcs_relatedresourceid").setValue(null);
                formContext.getAttribute("mcs_relatedresourceid").setRequiredLevel("none");
                formContext.getControl("mcs_relatedresourceid").setVisible(false);
            }
            break;
        default:
            typeDisable = false;
            //Clears value of related Resource/User if no type is selected. 
            formContext.getControl("mcs_relateduserid").setVisible(true);
            formContext.getAttribute("mcs_relateduserid").setRequiredLevel("none");
            formContext.getAttribute("mcs_relateduserid").setValue(null);

            formContext.getControl("mcs_relatedresourceid").setVisible(true);
            formContext.getAttribute("mcs_relatedresourceid").setRequiredLevel("none");
            formContext.getAttribute("mcs_relatedresourceid").setValue(null);
            break;
    }
    formContext.getControl("mcs_type").setDisabled(typeDisable);
    formContext.getControl("mcs_relatedsiteid").setDisabled(siteFieldValue != null);
};
var alertOptions = { height: 200, width: 300 };
MCS.GroupResource.CreateName = function (executionContext) {
    var formContext = executionContext.getFormContext();
    var recordName = formContext.getAttribute("mcs_name").getValue();
    var builtName = "";

    if (formContext.getControl("mcs_relatedresourceid").getVisible() && formContext.getAttribute("mcs_relatedresourceid").getValue() != null)
        builtName = formContext.getAttribute("mcs_relatedresourceid").getValue()[0].name;

    if (formContext.getControl("mcs_relateduserid").getVisible() && formContext.getAttribute("mcs_relateduserid").getValue() != null)
        builtName = formContext.getAttribute("mcs_relateduserid").getValue()[0].name;

    if (builtName === "") {
        var alertStrings = { confirmButtonLabel: "OK", text: "Please pick the related resource or user before saving.", title: "user before saving." };
        Xrm.Navigation.openAlertDialog(alertStrings, alertOptions)
        //alert("Please pick the related resource or user before saving."); //Modified by Naveen Dubbaka
        //executionObj.getEventArgs().preventDefault();
        return;
    }

    if (recordName !== builtName) {
        formContext.getAttribute("mcs_name").setSubmitMode("always");
        formContext.getAttribute("mcs_name").setValue(builtName);
    }
};

