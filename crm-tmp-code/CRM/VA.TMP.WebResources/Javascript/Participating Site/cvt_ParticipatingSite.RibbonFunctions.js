//If the SDK namespace object is not defined, create it.
if (typeof (MCS) == "undefined")
{ MCS = {}; }
// Create Namespace container for functions in this library;
if (typeof (MCS.ParticipatingSite) == "undefined") {
    MCS.ParticipatingSite = {};
}

//Opens a window for a new TMP Resource Group record using the Information form.
MCS.ParticipatingSite.openNewmcs_resourcegroup = function (EntityId) {
    var globalContext = Xrm.Utility.getGlobalContext();
    if ((typeof Xrm != "undefined") && (typeof Xrm.Utility != "undefined")) {
        var p = {};
        p.mcs_relatedsiteid = MCS.ParticipatingSite.Site[0].id;
        p.mcs_relatedsiteidname = MCS.ParticipatingSite.Site[0].name;
        p.mcs_createproviderrg = true;
        p.mcs_tsaguid = EntityId;

        Xrm.Navigation.openForm("mcs_resourcegroup", null, p);
    }
    else {
        var features = "location=no,menubar=no,status=no,toolbar=no,resizable=yes";
        var extraqs = ["mcs_relatedsiteid=" + MCS.ParticipatingSite.Site[0].id,
        "mcs_relatedsiteidname=" + MCS.ParticipatingSite.Site[0].name,
        "mcs_createproviderrg=true",
        "mcs_tsaguid=" + EntityId]
        // var url = Xrm.Page.context.getClientUrl();
        var url = globalContext.getClientUrl();
        window.open(url + "/main.aspx?etn=mcs_resourcegroup&pagetype=entityrecord&extraqs=" + encodeURIComponent(extraqs.join("&")), "_blank", features, false);
    }
};

//Opens a window for a new TMP Resource record using the Information form.
MCS.ParticipatingSite.openNewmcs_resource = function (EntityId) {
    var globalContext = Xrm.Utility.getGlobalContext();
    if ((typeof Xrm != "undefined") && (typeof Xrm.Utility != "undefined")) {
        var p = {};
        p.mcs_relatedsiteid = MCS.ParticipatingSite.Site[0].id;
        p.mcs_relatedsiteidname = MCS.ParticipatingSite.Site[0].name;
        p.mcs_createproviderr = true;
        p.mcs_tsaguid = EntityId;
        Xrm.Navigation.openForm("mcs_resource", null, p);
    }
    else {
        var features = "location=no,menubar=no,status=no,toolbar=no,resizable=yes";
        var extraqs = ["mcs_relatedsiteid=" + MCS.ParticipatingSite.Site[0].id,
        "mcs_relatedsiteidname=" + MCS.ParticipatingSite.Site[0].name,
        "mcs_createproviderr=true",
        "mcs_tsaguid=" + EntityId]
       // var url = Xrm.Page.context.getClientUrl();
        var url = globalContext.getClientUrl();

        window.open(url + "/main.aspx?etn=mcs_resource&pagetype=entityrecord&extraqs=" + encodeURIComponent(extraqs.join("&")), "_blank", features, false);
    }
};

//Opens a window for a new Scheduling Resource record using the Information form.
MCS.ParticipatingSite.openNewcvt_schedulingresource = function () {
    var globalContext = Xrm.Utility.getGlobalContext();
    if ((typeof Xrm != "undefined") && (typeof Xrm.Utility != "undefined")) {
        //Checking which entity this is running on to determine the parameters to pass through. 
        var p = {};
        p.cvt_siteid = MCS.ParticipatingSite.Site[0].id;
        p.cvt_siteidname = MCS.ParticipatingSite.Site[0].name;
        p.cvt_participatingsiteid = MCS.ParticipatingSite.EntityId;
        p.cvt_participatingsiteidname = MCS.ParticipatingSite.PSName;
        Xrm.Utility.openEntityForm("cvt_schedulingresource", null, p);
    }
    else {
        var features = "location=no,menubar=no,status=no,toolbar=no,resizable=yes";

        var extraqs = ["cvt_siteid=" + MCS.ParticipatingSite.Site[0].id,
            "cvt_siteidname=" + MCS.ParticipatingSite.Site[0].name,
            "cvt_participatingsiteid=" + MCS.ParticipatingSite.EntityId,
            "cvt_participatingsiteidname=" + MCS.ParticipatingSite.PSName];

        //var url = Xrm.Page.context.getClientUrl();
        var url = globalContext.getClientUrl();

        window.open(url + "/main.aspx?etn=cvt_schedulingresource&pagetype=entityrecord&extraqs=" + encodeURIComponent(extraqs.join("&")), "_blank", features, false);
    }
};

//Function called from the PS Ribbon Button
MCS.ParticipatingSite.QuickCreateResourceGroup = function () {
    MCS.ParticipatingSite.openNewmcs_resourcegroup(MCS.ParticipatingSite.EntityId);
};

//Function called from the PS Ribbon Button
MCS.ParticipatingSite.QuickCreateResource = function () {
    MCS.ParticipatingSite.openNewmcs_resource(MCS.ParticipatingSite.EntityId);
};

//Function called from the PS Ribbon Button
MCS.ParticipatingSite.AddResource = function () {
    MCS.ParticipatingSite.openNewcvt_schedulingresource(MCS.ParticipatingSite.EntityId, MCS.ParticipatingSite.PSName);
};

MCS.ParticipatingSite.LinkResource = function () {
    //if (MCS.mcs_Patient_Resource.GroupAppt == 0)
        MCS.ParticipatingSite.openNewcvt_schedulingresource();
    //else
    //    MCS.ParticipatingSite.openNewcvt_patientresourcegroupGroupAppt(1, MCS.mcs_Patient_Resource.EntityId, MCS.mcs_Patient_Resource.TSAName);
};