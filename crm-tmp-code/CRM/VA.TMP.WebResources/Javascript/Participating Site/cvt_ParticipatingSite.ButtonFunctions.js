//If the SDK namespace object is not defined, create it.
if (typeof (MCS) == "undefined") { MCS = {}; }
// Create Namespace container for functions in this library;
if (typeof (MCS.PS_Buttons) == "undefined") {
    MCS.PS_Buttons = {};
}

//Page Scope Variables
MCS.PS_Buttons.Async = false;

function loadScript(url, callback) {
    // Adding the script tag to the head as suggested before
    var head = document.head;
    var script = document.createElement('script');
    script.type = 'text/javascript';
    script.src = url;

    // Then bind the event to the callback function.
    // There are several events for cross browser compatibility.
    script.onreadystatechange = callback;
    script.onload = callback;

    // Fire the loading
    head.appendChild(script);
}

//HELPER - Unsupported method of re-creating Multi-select Lookup window in order to Create a different type of record than you are looking up
MCS.PS_Buttons.buildshowStandardDialog = function (entityName, viewName, callback) {
    var otcCode = MCS.cvt_Common.getObjectTypeCode(entityName);

    if (MCS.ParticipatingSite.Site != null) {
        var customViewId = MCS.ParticipatingSite.Site[0].id;
        var customView;

        if (MCS.ParticipatingSite.Side == "Provider" && MCS.ParticipatingSite.Modality == "SFT") {
            if (viewName == "Resources By Site") {
                //Prevent users from adding Provider Side Resources
                alert("Per Business Rules, you should enter only a Provider or Provider Group on this SFT Resource Package.");
                return;
            }
        }
        else if (MCS.ParticipatingSite.Side == "Patient" && MCS.ParticipatingSite.isGroup) {
            if (viewName != "Resource Groups By Site") {
                //Prevent users from adding Patient Side Resources or Users
                alert("Per Business Rules, you should enter Patient Side Paired Groups on this Group Resource Package.");
                return;
            }
        }

        var globalContext = Xrm.Utility.getGlobalContext();
        var clientUrl = globalContext.getClientUrl();
        var FetchXml;
        var LayoutXml;
        switch (viewName) {
            case "Users By Site":
                FetchXml = getUserFetchXml(MCS.ParticipatingSite.Site);
                LayoutXml = getUserLayout(otcCode);
                break;
            case "Resource Groups By Site":
                if (MCS.ParticipatingSite.Side == "Patient" && MCS.ParticipatingSite.Group){
                    FetchXml = getResourceGroupFetchXml(MCS.ParticipatingSite.Site, true);
                    LayoutXml = getResourceGroupLayout(otcCode);
                }
                else {
                    FetchXml = getResourceGroupFetchXml(MCS.ParticipatingSite.Site, false);
                    LayoutXml = getResourceGroupLayout(otcCode);
                }
                break;
            case "Resources By Site":
                FetchXml = getResourceFetchXml(MCS.ParticipatingSite.Site);
                LayoutXml = getResourceLayout(otcCode)
                break;
        }

        var customView = {
            id: customViewId,//Some fake id
            recordType: otcCode,//Entity Type Code of entity... yes, again
            name: viewName,
            fetchXml: FetchXml,
            layoutXml: LayoutXml,
            Type: 0//Hardcoded, leave it as it is
        };
        var lookupOptions = {
            defaultViewId: customViewId,
            allowMultiSelect: true,
            defaultEntityType: entityName,
            entityTypes: [entityName],
            customViews: [customView]
        };

        Xrm.Utility.lookupObjects(lookupOptions).then(callback, null);
    }
};

MCS.PS_Buttons.BuildRelationshipExisting = function (selectedControl) {
    alert("Resource Filtered Search Button clicked. Still under development.");
    var editable = MCS.PS_Buttons.CheckForScheduled();
    if (editable)
        MCS.ParticipatingSite.LinkResource();
    else
        alert("Participating Site must be in a status of Can Be Scheduled = 'No' in order to add a Scheduling Resource.");
};

//Called By Ribbon Button - Add Resource
MCS.PS_Buttons.BuildRelationshipResourceBySite = function (primaryControl) {
    MCS.primaryControl = primaryControl;

    var editable = MCS.PS_Buttons.CheckForScheduled();
    if (editable)
        MCS.PS_Buttons.BuildRelationshipResourceRunner("mcs_resource");
    else
        alert("Participating Site must be in a status of Can Be Scheduled = 'No' in order to add a Scheduling Resource.");
};
//Called by Ribbon Button - Add Resource Groups
MCS.PS_Buttons.BuildRelationshipResourceGroupBySite = function (primaryControl) {
    MCS.primaryControl = primaryControl;

    var editable = MCS.PS_Buttons.CheckForScheduled();
    if (editable)
        MCS.PS_Buttons.BuildRelationshipResourceRunner("mcs_resourcegroup");
    else
        alert("Participating Site must be in a status of Can Be Scheduled = 'No' in order to add a Scheduling Resource.");
};
// Called by Ribbon Button - Add Users
MCS.PS_Buttons.BuildRelationshipUserBySite = function (primaryControl) {
    MCS.primaryControl = primaryControl;

    var editable = MCS.PS_Buttons.CheckForScheduled();
    if (editable)
        MCS.PS_Buttons.BuildRelationshipUserRunner(primaryControl);
    else
        alert("Participating Site must be in a status of Can Be Scheduled = 'No' in order to add a Scheduling Resource.");
};

MCS.PS_Buttons.BuildRelationshipUserRunnerCallback = function (results) {

    if ((results != null) && (results != undefined)) {
        var returnedItems = results;
        if (typeof (results) == "string")
            returnedItems = JSON.parse(results);
        if (returnedItems != null) {
            for (i = 0; i < returnedItems.length; i++) {
                var systemUser = {
                    'cvt_name': returnedItems[i].name,
                    'cvt_schedulingresourcetype': MCS.schedulingResourceType,
                    'cvt_resourcetype': MCS.resourceType,
                    'cvt_participatingsite@odata.bind': "/cvt_participatingsites(" + MCS.ParticipatingSite.EntityId.replace('{', '').replace('}', '') + ")",
                    'cvt_user@odata.bind': "/systemusers(" + returnedItems[i].id.replace('{', '').replace('}', '') + ")"
                };

                Xrm.WebApi.createRecord("cvt_schedulingresource", systemUser).then(
                    function success(result) {
                        // when done?
                        //console.log('Resource User Created...');

                        var context = MCS.primaryControl.getFormContext();
                        context.getControl("subgrid_SchedulingResources").refresh();
                    },
                    function (error) {
                        alert(error.message);
                    }
                );
            }
        }
    }
    else
        return null;
};

//HELPER - Add User by Site (Patient or Provider)
MCS.PS_Buttons.BuildRelationshipUserRunner = function () {

    //var formContext = primaryControl.getFormContext();
    MCS.schedulingResourceType = (MCS.ParticipatingSite.Side == "Patient") ? 3 : 2;
    MCS.resourceType = (MCS.ParticipatingSite.Side == "Patient") ? 100000000 : 99999999;

    var lookupItems = MCS.PS_Buttons.buildshowStandardDialog('systemuser', "Users By Site", MCS.PS_Buttons.BuildRelationshipUserRunnerCallback);
};

MCS.PS_Buttons.BuildRelationshipResourceRunnerCallback = function (results) {
    if ((results != null) && (results != undefined)) {
        var returnedItems = results;
        if (typeof (results) == "string")
            returnedItems = JSON.parse(results);
        if (returnedItems != null) {
            //alert(returnedItems.length + " records selected.");
            for (i = 0; i < returnedItems.length; i++) {
                var ResourceId = returnedItems[i].id;
                var resourcename, type, relatedSite, relatedSiteName;
                var entityName = returnedItems[i].typename;
                Xrm.WebApi.retrieveRecord(entityName, ResourceId, "?$select=mcs_name,mcs_type").then(
                    function success(result) {
                        resourcename = result.mcs_name;
                        type = result.mcs_type;
                        relatedSite = entityName == "mcs_resourcegroup" ? result.mcs_relatedSiteId : result.mcs_RelatedSiteId;
                        ResourceId = entityName == "mcs_resourcegroup" ? result.mcs_resourcegroupid : result.mcs_resourceid;
                        var newResource = {
                            'cvt_name': resourcename,
                            'cvt_schedulingresourcetype': MCS.SchedulingResourceType,
                            'cvt_resourcetype': type,
                            'cvt_participatingsite@odata.bind': "/cvt_participatingsites(" + MCS.ParticipatingSite.EntityId.replace('{', '').replace('}', '') + ")",
                        };

                        var selectedID = ResourceId.replace('{', '').replace('}', '');

                        if (entityName == "mcs_resourcegroup") {
                            newResource['cvt_tmpresourcegroup@odata.bind'] = "/mcs_resourcegroups(" + selectedID + ")";
                            //alert("About to create GrRes SR for " + resourcename + ". ID: " + selectedID);
                        }
                        else {
                            newResource['cvt_tmpresource@odata.bind'] = "/mcs_resources(" + selectedID + ")";
                            //alert("About to create Res SR for " + resourcename + ". ID: " + selectedID);
                        }

                        Xrm.WebApi.createRecord("cvt_schedulingresource", newResource).then(
                            function success(result) {
                                //Nothing here for now. Moved the Subgrid Refresh outside of loop to only run once all resources are created.  
                                console.log('Resource Created');

                                var context = MCS.primaryControl.getFormContext();
                                context.getControl("subgrid_SchedulingResources").refresh();
                            },
                            function (error) {
                                alert(error.message);
                            }
                        );
                    },
                    function (error) {
                        alert("Failed retrieved" + MCS.cvt_Common.RestError(error));
                    }
                );
            }
        }
    }
    else
        return null;
};

//HELPER - Add Resource by Site (Patient or Provider AND Resource or Resource Group)
MCS.PS_Buttons.BuildRelationshipResourceRunner = function (entityName) {
    //Fields to make this function robust
    var siteField = 'mcs_RelatedSiteId';
    MCS.SchedulingResourceType = 1;
    MCS.title = "Resources";

    if (entityName == "mcs_resourcegroup") {
        MCS.siteField = 'mcs_relatedSiteId';
        MCS.SchedulingResourceType = 0;
        MCS.title = "Resource Groups";
    }

    var lookupItems = MCS.PS_Buttons.buildshowStandardDialog(entityName, MCS.title + " By Site", MCS.PS_Buttons.BuildRelationshipResourceRunnerCallback);
};

MCS.PS_Buttons.CheckForScheduled = function (primaryControl) {
    var isScheduleable = false;

    if (typeof primaryControl === 'undefined' || primaryControl == null) {
        isScheduleable = Xrm.Page.getAttribute('cvt_scheduleable').getValue();
    }
    else {
        var formContext = primaryControl.getFormContext();
        isScheduleable = formContext.getAttribute("cvt_scheduleable").getValue();
    }

    if (isScheduleable == true)
        return false;
    else
        return true;
};

MCS.PS_Buttons.buildSiteConditions = function (site, type) {
    var siteXml = '';
    switch (type) {
        case "U":
            siteXml = '<condition attribute="cvt_site" operator="in">';
            break;
        default:
            siteXml = '<condition attribute="mcs_relatedsiteid" operator="in">';
            break;
    }
    siteXml += '<value uiname="' + MCS.cvt_Common.formatXML(site[0].mcs_name) + '" uitype="mcs_site">' + site[0].id + '</value></condition>';

    return siteXml;
};
//HELPER - creates the fetchXML to be used in grid views
function getResourceGroupFetchXml(site, isAllReqd) {
    var columns = [
        'mcs_name',
        'mcs_type',
        'mcs_relatedsiteid',
        'modifiedon',
        'createdon',
        'mcs_resourcegroupid'
    ];
    var conditions = [];

    if (isAllReqd) {
        conditions = [
            '<condition attribute="statecode" operator="eq" value="0"/>',
            '<condition attribute="mcs_type" operator="eq" value="917290000"/>',
            MCS.PS_Buttons.buildSiteConditions(site, "RG")
        ];
    }
    else {
        conditions = [
            '<condition attribute="statecode" operator="eq" value="0"/>',
            MCS.PS_Buttons.buildSiteConditions(site, "RG")
        ];
    }

    fetchXml = MCS.cvt_Common.CreateFetch('mcs_resourcegroup', columns, conditions, ['mcs_name', false]);
    return fetchXml;
};
function getResourceFetchXml(site) {

    var columns = [
        'mcs_resourceid',
        'mcs_name',
        'createdon'
    ];

    var conditions = [
        '<condition attribute="statecode" operator="eq" value="0"/>',
        MCS.PS_Buttons.buildSiteConditions(site, "R")
    ];

    fetchXml = MCS.cvt_Common.CreateFetch('mcs_resource', columns, conditions, ['mcs_name', false]);

    return fetchXml;
};
function getUserFetchXml(site) {
    fetchXml = '<fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="false"><entity name="systemuser"><attribute name="fullname"/><attribute name="businessunitid"/><attribute name="cvt_type"/><attribute name="domainname"/><attribute name="jobtitle"/><attribute name="internalemailaddress"/><attribute name="modifiedon"/><attribute name="cvt_site"/><attribute name="cvt_facility"/><attribute name="systemuserid"/><order attribute="fullname" descending="false"/><filter type="and"><condition attribute="isdisabled" operator="eq" value="0"/><condition attribute="accessmode" operator="ne" value="3"/>';
    fetchXml += MCS.PS_Buttons.buildSiteConditions(site, "U");
    fetchXml += '</filter><link-entity name="team" from="teamid" to="cvt_primaryteam" visible="false" link-type="outer" alias="a_8f34fae9459de2118b0978e3b511a629"><attribute name="name"/></link-entity></entity></fetch>';
    return fetchXml;
};
//HELPER - sets the FetchXML view layout
function getResourceGroupLayout(otc) {
    return '<grid name="resultset" object="' + otc + '" jump="mcs_name" select="1" icon="0" preview="0"><row name="result" id="mcs_resourcegroupid"><cell name="mcs_name" width="300" /><cell name="mcs_relatedsiteid" width="300" /><cell name="mcs_type" width="150" /><cell name="modifiedon" width="125" /><cell name="createdon" width="125" /></row></grid>';
};
function getResourceLayout(otc) {
    return '<grid name="resultset" object="' + otc + '" jump="mcs_name" select="1" icon="0" preview="0"><row name="result" id="mcs_resourceid"><cell name="mcs_name" width="300" /><cell name="mcs_relatedsiteid" width="200" /><cell name="mcs_type" width="100" /><cell name="mcs_businessunitid" width="100" /><cell name="ownerid" width="150" /><cell name="modifiedon" width="125" /><cell name="createdon" width="125" /></row></grid>';
};
function getUserLayout(otc) {
    return '<grid name="resultset" object="8" jump="fullname" select="1" icon="0" preview="0"><row name="result" id="systemuserid"><cell name="fullname" width="200" /><cell name="jobtitle" width="150" /><cell name="domainname" width="200" /><cell name="internalemailaddress" width="150" /><cell name="cvt_type" width="125" /><cell name="cvt_facility" width="150" /><cell name="cvt_site" width="200" /><cell name="a_8f34fae9459de2118b0978e3b511a629.name" width="150" disableSorting="1" /><cell name="businessunitid" width="150" /><cell name="modifiedon" width="100" /></row></grid>';
};

function openDialogProcess(url, widthInput, heightInput) {
    var width = 400;
    var height = 400;
    var left = (screen.width - width) / 2;
    var top = (screen.height - height) / 2;
    return window.open(url, '', 'location=0,menubar=1,resizable=1,width=' + width + ',height=' + height + ',top=' + top + ',left=' + left + '');
};