var FORM_TYPE_CREATE = 1;
function MakeFieldsReadOnly(executionContext) {
    'use strict';
    var formContext = executionContext.getFormContext();
    var relatedSite = formContext.ui.controls.get("cvt_relatedsiteid");
    var relatedSiteValue = relatedSite.getAttribute("cvt_relatedsiteid").getValue();
    var relatedTSA = formContext.ui.controls.get("cvt_relatedtsaid");
    var relatedTSAValue = relatedTSA.getAttribute("cvt_relatedtsaid").getValue();
    if (formContext.ui.controls.get("cvt_relatedmastertsaid") != null) {
        var relatedMasterTSAValue = formContext.getAttribute("cvt_relatedmastertsaid").getValue();
    }

    if (relatedSiteValue != null) {
        formContext.ui.controls.get("cvt_relatedsiteid").setDisabled(true);
    }

    if (relatedTSAValue != null) {
        formContext.ui.controls.get("cvt_relatedtsaid").setDisabled(true);
        if (formContext.ui.controls.get("cvt_relatedmastertsaid") != null) {
            formContext.ui.controls.get("cvt_relatedmastertsaid").setDisabled(true);
        }
        formContext.ui.controls.get("cvt_capacityrequired").setDisabled(true);
    }
    if (relatedMasterTSAValue != null) {
        formContext.ui.controls.get("cvt_relatedtsaid").setDisabled(true);
        formContext.ui.controls.get("cvt_relatedmastertsaid").setDisabled(true);
        formContext.ui.controls.get("cvt_capacityrequired").setDisabled(true);
    }
}
function RemovePersonOption(executionContext) {
    var formContext = executionContext.getFormContext();
    var options = formContext.ui.controls.get("cvt_tsaresourcetype");
    options.removeOption(3);
}

function RemoveProviderOption(executionContext) {
    var formContext = executionContext.getFormContext();
    var options = formContext.ui.controls.get("cvt_tsaresourcetype");
    options.removeOption(2);
}

function RemoveProTypeOption(executionContext) {
    var formContext = executionContext.getFormContext();
    var options = formContext.ui.controls.get("cvt_type");
    options.removeOption(99999999);

}

function RemoveTeleTypeOption(executionContext) {
    var formContext = executionContext.getFormContext();
    var options = formContext.ui.controls.get("cvt_type");
    options.removeOption(100000000);
}


function HandleOnChangeLookup(executionContext) {
    var formContext = executionContext.getFormContext();

    var site = new Array();
    site = formContext.getAttribute("cvt_relatedsiteid").getValue();
    var type = formContext.getAttribute("cvt_type");

    var tsaType = formContext.ui.controls.get("cvt_tsaresourcetype");
    var ResourceGroupControl = formContext.ui.controls.get("cvt_relatedresourcegroupid");
    var ResourceControl = formContext.ui.controls.get("cvt_relatedresourceid");
    var UserControl = formContext.ui.controls.get("cvt_relateduserid");

    var OwningUser = new Array();
    OwningUser = formContext.getAttribute("ownerid").getValue();



    if ((tsaType !== "undefined") && (tsaType.getAttribute().getValue() !== null)) {

        var attribute = tsaType.getAttribute();
        var tsaTypeFieldValue = attribute.getValue();

        var ProvViewID = "{9CFC3E6D-C3A2-E211-9B92-00155D144F0F}";
        var TeleViewID = "{47127E79-C3A2-E211-9B92-00155D144F0F}";

        var ViewDisplayNameUser = "Filtered By Pat/Prov Type";
        var viewIsDefault = true;
        var ProvTeleDefaultView = false;

        if (tsaTypeFieldValue === 2) {
            var UserfetchXml = '<fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="false"><entity name="systemuser"><attribute name="fullname"/><attribute name="title"/><attribute name="address1_telephone1"/><attribute name="businessunitid"/><attribute name="cvt_type"/><attribute name="systemuserid"/><order attribute="fullname" descending="false"/><filter type="and"><condition attribute="isdisabled" operator="eq" value="0"/><condition attribute="accessmode" operator="ne" value="3"/><condition attribute="cvt_type" operator="eq" value="917290001"/></filter></entity></fetch>';
            var UserlayoutXml = '<grid name="resultset" object="8" jump="fullname" select="1" icon="0" preview="0"><row name="result" id="systemuserid"><cell name="fullname" width="200" /><cell name="cvt_type" width="125" /><cell name="businessunitid" width="150" /><cell name="title" width="100" /><cell name="address1_telephone1" width="100" /></row></grid>';
            UserControl.addCustomView(ProvViewID, "systemuser", ViewDisplayNameUser, UserfetchXml, UserlayoutXml, ProvTeleDefaultView);
        }
        if (tsaTypeFieldValue === 3) {
            var UserfetchXml = '<fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="false"><entity name="systemuser"><attribute name="fullname"/><attribute name="title"/><attribute name="address1_telephone1"/><attribute name="businessunitid"/><attribute name="cvt_type"/><attribute name="systemuserid"/><order attribute="fullname" descending="false"/><filter type="and"><condition attribute="isdisabled" operator="eq" value="0"/><condition attribute="accessmode" operator="ne" value="3"/><condition attribute="cvt_type" operator="eq" value="917290000"/></filter></entity></fetch>';
            var UserlayoutXml = '<grid name="resultset" object="8" jump="fullname" select="1" icon="0" preview="0"><row name="result" id="systemuserid"><cell name="fullname" width="200" /><cell name="cvt_type" width="125" /><cell name="businessunitid" width="150" /><cell name="title" width="100" /><cell name="address1_telephone1" width="100" /></row></grid>';
            UserControl.addCustomView(TeleViewID, "systemuser", ViewDisplayNameUser, UserfetchXml, UserlayoutXml, ProvTeleDefaultView);
        }

        if ((type !== "undefined") && (type.getValue() !== null) && (site !== null)) {

            var ViewDisplayName = "Filtered by Site and Type";
            var siteID = site[0].id;
            var viewID = siteID;

            var RGfetchXml = '<fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="false"><entity name="mcs_resourcegroup"><attribute name="mcs_resourcegroupid"/><attribute name="mcs_name"/><attribute name="createdon"/><order attribute="mcs_name" descending="false"/><filter type="and"><condition attribute="statecode" operator="eq" value="0"/><condition attribute="mcs_relatedsiteid" operator="eq" uiname="' + formatXML(site[0].name) + '" uitype="mcs_site" value="' + siteID + '"/><condition attribute="mcs_type" operator="eq" value="' + type.getValue() + '"/></filter></entity></fetch>';
            var RGlayoutXml = '<grid name="resultset" object="10007" jump="mcs_name" select="1" icon="0" preview="0"><row name="result" id="mcs_resourcegroupid"><cell name="mcs_name" width="200" /><cell name="mcs_relatedsiteid" width="125" /><cell name="mcs_type" width="125" /></row></grid>';
            ResourceGroupControl.addCustomView(viewID, "mcs_resourcegroup", ViewDisplayName, RGfetchXml, RGlayoutXml, viewIsDefault);

            var ResourcefetchXml = '<fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="false"><entity name="mcs_resource"><attribute name="mcs_resourceid"/><attribute name="mcs_name"/><attribute name="createdon"/><order attribute="mcs_name" descending="false"/><filter type="and"><condition attribute="statecode" operator="eq" value="0"/><condition attribute="mcs_relatedsiteid" operator="eq" uiname="' + formatXML(site[0].name) + '" uitype="mcs_site" value="' + siteID + '"/><condition attribute="mcs_type" operator="eq" value="' + type.getValue() + '"/></filter></entity></fetch>';
            var ResourcelayoutXml = '<grid name="resultset" object="10006" jump="mcs_name" select="1" icon="0" preview="0"><row name="result" id="mcs_resourceid"><cell name="mcs_name" width="200" /><cell name="mcs_relatedsiteid" width="125" /><cell name="mcs_type" width="125" /></row></grid>';
            ResourceControl.addCustomView(viewID, "mcs_resource", ViewDisplayName, ResourcefetchXml, ResourcelayoutXml, viewIsDefault);

        }
        else {

            if ((site != null)) {

                var ViewDisplayName = "Filtered by Site";
                var siteID = site[0].id;
                var viewID = siteID;

                var RGfetchXml = '<fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="false"><entity name="mcs_resourcegroup"><attribute name="mcs_resourcegroupid"/><attribute name="mcs_name"/><attribute name="createdon"/><order attribute="mcs_name" descending="false"/><filter type="and"><condition attribute="statecode" operator="eq" value="0"/><condition attribute="mcs_relatedsiteid" operator="eq" uiname="' + formatXML(site[0].name) + '" uitype="mcs_site" value="' + siteID + '"/></filter></entity></fetch>';
                var RGlayoutXml = '<grid name="resultset" object="10007" jump="mcs_name" select="1" icon="0" preview="0"><row name="result" id="mcs_resourcegroupid"><cell name="mcs_name" width="200" /><cell name="mcs_relatedsiteid" width="125" /><cell name="mcs_type" width="125" /></row></grid>';
                ResourceGroupControl.addCustomView(viewID, "mcs_resourcegroup", ViewDisplayName, RGfetchXml, RGlayoutXml, viewIsDefault);

                var ResourcefetchXml = '<fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="false"><entity name="mcs_resource"><attribute name="mcs_resourceid"/><attribute name="mcs_name"/><attribute name="createdon"/><order attribute="mcs_name" descending="false"/><filter type="and"><condition attribute="statecode" operator="eq" value="0"/><condition attribute="mcs_relatedsiteid" operator="eq" uiname="' + formatXML(site[0].name) + '" uitype="mcs_site" value="' + siteID + '"/></filter></entity></fetch>';
                var ResourcelayoutXml = '<grid name="resultset" object="10006" jump="mcs_name" select="1" icon="0" preview="0"><row name="result" id="mcs_resourceid"><cell name="mcs_name" width="200" /><cell name="mcs_relatedsiteid" width="125" /><cell name="mcs_type" width="125" /></row></grid>';
                ResourceControl.addCustomView(viewID, "mcs_resource", ViewDisplayName, ResourcefetchXml, ResourcelayoutXml, viewIsDefault);
            }

            else {

                if ((type !== "undefined") && (type.getValue() !== null)) {

                    var ViewDisplayName = "Filtered by Resource Type";
                    var OwningUserID = OwningUser[0].id;
                    var typeViewID = OwningUserID;

                    var RGfetchXml = '<fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="false"><entity name="mcs_resourcegroup"><attribute name="mcs_resourcegroupid"/><attribute name="mcs_name"/><attribute name="createdon"/><order attribute="mcs_name" descending="false"/><filter type="and"><condition attribute="statecode" operator="eq" value="0"/><condition attribute="mcs_type" operator="eq" value="' + type.getValue() + '"/></filter></entity></fetch>';
                    var RGlayoutXml = '<grid name="resultset" object="10007" jump="mcs_name" select="1" icon="0" preview="0"><row name="result" id="mcs_resourcegroupid"><cell name="mcs_name" width="200" /><cell name="mcs_relatedsiteid" width="125" /><cell name="mcs_type" width="125" /></row></grid>';
                    ResourceGroupControl.addCustomView(typeViewID, "mcs_resourcegroup", ViewDisplayName, RGfetchXml, RGlayoutXml, viewIsDefault);

                    var ResourcefetchXml = '<fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="false"><entity name="mcs_resource"><attribute name="mcs_resourceid"/><attribute name="mcs_name"/><attribute name="createdon"/><order attribute="mcs_name" descending="false"/><filter type="and"><condition attribute="statecode" operator="eq" value="0"/><condition attribute="mcs_type" operator="eq" value="' + type.getValue() + '"/></filter></entity></fetch>';
                    var ResourcelayoutXml = '<grid name="resultset" object="10006" jump="mcs_name" select="1" icon="0" preview="0"><row name="result" id="mcs_resourceid"><cell name="mcs_name" width="200" /><cell name="mcs_relatedsiteid" width="125" /><cell name="mcs_type" width="125" /></row></grid>';
                    ResourceControl.addCustomView(typeViewID, "mcs_resource", ViewDisplayName, ResourcefetchXml, ResourcelayoutXml, viewIsDefault);
                }

                else {

                    var ViewDisplayNameRG = "All Resource Groups ";
                    var ViewDisplayNameR = "All Resources";
                    var OwningUserID = OwningUser[0].id;
                    var typeViewID = OwningUserID;

                    var RGfetchXml = '<fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="false"><entity name="mcs_resourcegroup"><attribute name="mcs_resourcegroupid"/><attribute name="mcs_name"/><attribute name="createdon"/><order attribute="mcs_name" descending="false"/><filter type="and"><condition attribute="statecode" operator="eq" value="0"/></filter></entity></fetch>';
                    var RGlayoutXml = '<grid name="resultset" object="10007" jump="mcs_name" select="1" icon="0" preview="0"><row name="result" id="mcs_resourcegroupid"><cell name="mcs_name" width="200" /><cell name="mcs_relatedsiteid" width="125" /><cell name="mcs_type" width="125" /></row></grid>';
                    ResourceGroupControl.addCustomView(typeViewID, "mcs_resourcegroup", ViewDisplayNameRG, RGfetchXml, RGlayoutXml, viewIsDefault);

                    var ResourcefetchXml = '<fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="false"><entity name="mcs_resource"><attribute name="mcs_resourceid"/><attribute name="mcs_name"/><attribute name="createdon"/><order attribute="mcs_name" descending="false"/><filter type="and"><condition attribute="statecode" operator="eq" value="0"/></filter></entity></fetch>';
                    var ResourcelayoutXml = '<grid name="resultset" object="10006" jump="mcs_name" select="1" icon="0" preview="0"><row name="result" id="mcs_resourceid"><cell name="mcs_name" width="200" /><cell name="mcs_relatedsiteid" width="125" /><cell name="mcs_type" width="125" /></row></grid>';
                    ResourceControl.addCustomView(typeViewID, "mcs_resource", ViewDisplayNameR, ResourcefetchXml, ResourcelayoutXml, viewIsDefault);
                }
            }
        }
    }
}

function showDetails(executionContext) {
    var formContext = executionContext.getFormContext();

    formContext.getAttribute("cvt_capacityrequired").setValue(1);
    //get control for field
    var tsaType = formContext.ui.controls.get("cvt_tsaresourcetype");
    //get control for dependent field
    var type = formContext.ui.controls.get("cvt_type");
    var relatedResourceGroup = formContext.ui.controls.get("cvt_relatedresourcegroupid");
    var relatedUser = formContext.ui.controls.get("cvt_relateduserid");
    var relatedResource = formContext.ui.controls.get("cvt_relatedresourceid");
    //Clinic Name field removed for now
    //var vistaclinic = formContext.ui.controls.get("cvt_vistaclinicname");

    //check if tsaType is null then get attribute value if not. 
    if (tsaType != null) var tsaTypeFieldValue = tsaType.getAttribute("cvt_tsaresourcetype").getValue();
    //check if type is null and then get attritbute value if not. 
    if (type != null) var typeFieldValue = type.getAttribute("cvt_type").getValue();

    if ((tsaType !== "undefined") && (tsaType !== null)) {
        //set visible based on dependent field's value       
        //if TSA is Resource or Single Resource, we need type
        if ((tsaTypeFieldValue <= 1) && (tsaTypeFieldValue != null)) {
            type.setVisible(true);
        }
        else {
            type.setVisible(false);
            formContext.getAttribute("cvt_type").setValue(null);
            formContext.getAttribute("cvt_type").setRequiredLevel("none");
        }
        //Check to see if tsaType is a Resource Group(0), and then see if there is a value for Type. Sets Visibility and Requirements.
        // Clears value of related Resource Group if no type is selected. 
        if (tsaTypeFieldValue === 0) {
            relatedResourceGroup.setVisible(true);
            formContext.getAttribute("cvt_relatedresourcegroupid").setRequiredLevel("required");
        }
        else {
            relatedResourceGroup.setVisible(false);
            formContext.getAttribute("cvt_relatedresourcegroupid").setValue(null);
            formContext.getAttribute("cvt_relatedresourcegroupid").setRequiredLevel("none");
        }
        //Check to see if tsaType is a Resource(1), and then checks value for Type. Sets Visiblity and Requirements levels. 
        if (tsaTypeFieldValue === 1) {
            relatedResource.setVisible(true);
            formContext.getAttribute("cvt_relatedresourceid").setRequiredLevel("required");
        }
        else {
            relatedResource.setVisible(false);
            formContext.getAttribute("cvt_relatedresourceid").setValue(null);
            formContext.getAttribute("cvt_relatedresourceid").setRequiredLevel("none");

        }
        //Check to see if tsaType is a Single User, and then sets visiblity and requirements levels. 
        if ((tsaTypeFieldValue === 2) || (tsaTypeFieldValue === 3)) {
            relatedUser.setVisible(true);
            formContext.getAttribute("cvt_relateduserid").setRequiredLevel("required");
        }
        else {
            relatedUser.setVisible(false);
            formContext.getAttribute("cvt_relateduserid").setValue(null);
            formContext.getAttribute("cvt_relateduserid").setRequiredLevel("none");
        }
    }
    if ((type !== "undefined") && (type !== null)) {
        //We have removed Clinic Name field for now
        /*
        if (typeFieldValue == 251920000) {
            vistaclinic.setVisible(true);
        }
        else {
            vistaclinic.setVisible(false);
            formContext.getAttribute("cvt_vistaclinicname").setValue(null);
        }
        */
    }
}

CreateName = function (executionContext) {
    var formContext = executionContext.getFormContext();
    /***********************************************************************
    /** 
    /** Description: CreateName
    /** 
    ***********************************************************************/

    var ResourceGroupControl = formContext.ui.controls.get("cvt_relatedresourcegroupid");
    var ResourceControl = formContext.ui.controls.get("cvt_relatedresourceid");
    var UserControl = formContext.ui.controls.get("cvt_relateduserid");
    //Removed for now
    //var ClinicNameControl = formContext.ui.controls.get("cvt_vistaclinicname");

    var derivedResultField = "";

    if (ResourceGroupControl.getVisible()) {
        var cvt_relatedresourcegroupid = formContext.getAttribute("cvt_relatedresourcegroupid").getValue();

        if (cvt_relatedresourcegroupid != null) {
            derivedResultField = cvt_relatedresourcegroupid[0].name;
        }
    }

    if (ResourceControl.getVisible()) {
        var cvt_relatedresourceid = formContext.getAttribute("cvt_relatedresourceid").getValue();
        //Field removed for now
        //  var ClinicNameText = formContext.getAttribute("cvt_vistaclinicname").getValue();

        if (cvt_relatedresourceid != null) {
            derivedResultField = cvt_relatedresourceid[0].name;
        }
        //Control removed for now
        /*
        if (ClinicNameControl.getVisible()) {
            if (ClinicNameText != null) {
                derivedResultField = ClinicNameText;
            }
        }
        */
    }

    if (UserControl.getVisible()) {
        var cvt_relateduserid = formContext.getAttribute("cvt_relateduserid").getValue();

        if (cvt_relateduserid != null) {
            derivedResultField = cvt_relateduserid[0].name;
        }
    }

    formContext.getAttribute("cvt_name").setSubmitMode("always");
    formContext.getAttribute("cvt_name").setValue(derivedResultField);
};

GetResourceData = function (executionContext) {
    var formContext = executionContext.getFormContext();
    /***********************************************************************
    /** 
    /** Description: GetResourceData - Gets the Resource Data and populates fields on Pat/Pro Site RE
    /** 
    /** 
    ***********************************************************************/
    var mcs_relatedresource = formContext.getAttribute("cvt_relatedresourceid").getValue();
    if (mcs_relatedresource != null) {
        getmcs_relatedresourceLookupData(mcs_relatedresource[0].id);
    }
    else {
        formContext.getAttribute("cvt_type").setValue(null);

    }
};
getmcs_relatedresourceLookupData = function (mcs_relatedresource) {
    retrievemcs_relatedresourceData(mcs_relatedresource);
};
retrievemcs_relatedresourceData = function (mcs_relatedresource) {
    var retrievemcs_relatedresourceInfoReq = new XMLHttpRequest();
    var request = MCS.GlobalFunctions._getServerUrl("ODATA") + "/mcs_resourceSet?$select=mcs_Type&$filter=mcs_servicesId eq guid'" + mcs_relatedresource + "'";
    retrievemcs_relatedresourceInfoReq.open("GET", request, true);
    retrievemcs_relatedresourceInfoReq.setRequestHeader("Accept", "application/json");
    retrievemcs_relatedresourceInfoReq.setRequestHeader("Content-Type", "application/json; charset=utf-8");
    retrievemcs_relatedresourceInfoReq.onreadystatechange = function () { retrievemcs_relatedresourceInfoReqCallBack(this); };
    retrievemcs_relatedresourceInfoReq.send();
};
retrievemcs_relatedresourceInfoReqCallBack = function (executionContext, retrievemcs_relatedresourceInfoReq) {
    var formContext = executionContext.getFormContext();
    if (retrievemcs_relatedresourceInfoReq.readyState === 4 /* complete */) {
        if (retrievemcs_relatedresourceInfoReq.status === 200) {
            //Success
            var retrievemcs_relatedresourceInfo = window.JSON.parse(retrievemcs_relatedresourceInfoReq.responseText).d;
            if (retrievemcs_relatedresourceInfo.results[0] != null) {
                //check to see if the control is on the form before you try to setvalue on it
                if (formContext.getAttribute("cvt_type") != null) {
                    if (retrievemcs_relatedresourceInfo.results[0].mcs_Type != null) {

                        //    var olookup = new Object();
                        //    olookup.value = retrievemcs_relatedresourceInfo.results[0].mcs_Type.value;                            
                        //    var olookupValue = new OptionSetValue();
                        //     olookupValue[0] = olookup;
                        //     var currentAttribute = formContext.getAttribute("cvt_type");
                        //     if (currentAttribute != null) {
                        //         var currentAttributeValue = currentAttribute.getValue();
                        //        if (currentAttributeValue != null) {                                  
                        //               formContext.getAttribute("cvt_type").setValue(olookupValue);

                        //        }
                        //        else {
                        //            formContext.getAttribute("cvt_type").setValue(olookupValue);
                        //        }

                        //     }
                        //    else {
                        //    formContext.getAttribute("cvt_type").setValue(null);
                        // }

                        formContext.getAttribute("cvt_type").setValue(null);
                        formContext.getAttribute("cvt_type").fireOnChange();
                    }
                    else {
                        formContext.getAttribute("cvt_type").setValue(null);
                    }
                }
            }

            else {
                // alert('No records were returned from the copy function, please contact your administrator'); //Modified by Naveen Dubbaka
                var alertOptions = { height: 200, width: 300 };
                var alertStrings = { confirmButtonLabel: "OK", text: "No records were returned from the copy function, please contact your administrator", title: "Patient Provier Resource Rules." };
                Xrm.Navigation.openAlertDialog(alertStrings, alertOptions);
            }
        } else {
            //Failure
            MCS.GlobalFunctions.errorHandler(retrievemcs_relatedresourceInfoReq);
        }
    }
};

UpdateProviderTeleSite = function (executionContext) {
    var formContext = executionContext.getFormContext();


    if (formContext.ui.controls.get("cvt_relateduserid") != null) {
        var User = formContext.getAttribute("cvt_relateduserid").getValue();
    };
    if (formContext.ui.controls.get("cvt_relatedsiteid") != null) {
        var Site = formContext.getAttribute("cvt_relatedsiteid").getValue();
    };

    if ((User != null) && (Site == null)) {

        formContext.ui.controls.get("cvt_relatedsiteid").setDisabled(false);
    }
};

formatXML = function (str) {
    //replace & with &amp;
    if (str) {
        str = str.replace(/&/g, "&amp;");

        return str;
    }
};

