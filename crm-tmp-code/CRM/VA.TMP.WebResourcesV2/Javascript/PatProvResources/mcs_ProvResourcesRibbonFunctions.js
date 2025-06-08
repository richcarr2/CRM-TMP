//If the SDK namespace object is not defined, create it.
//SD: web-use-strict-equality-operators
if (typeof(MCS) === "undefined") {
    MCS = {};
}
// Create Namespace container for functions in this library;
MCS.mcs_Provider_Resource = {};

//Namespace Variables
MCS.mcs_Provider_Resource.relatedProviderSiteId;
MCS.mcs_Provider_Resource.relatedProviderSiteName;
MCS.mcs_Provider_Resource.EntityId;
MCS.mcs_Provider_Resource.EntityName;
MCS.mcs_Provider_Resource.TSAName;
MCS.mcs_Provider_Resource.MTSAName;

//Opens a window for a new Resource Group record using the Information form.
//Example: openNewmcs_resourcegroup("{undefined}","",true,true,"");
MCS.mcs_Provider_Resource.openNewmcs_resourcegroup = function (mcs_relatedsiteid, mcs_relatedsiteidname, mcs_createproviderrg, mcs_createpatientrg) { //, mcs_tsaguid, cvt_mastertsaguid) {
    //SD - Start
    //web-use-strict-mode
    "use strict";
    //SD - End
    //SD: web-use-strict-equality-operators
    if ((typeof Xrm !== "undefined") && (typeof Xrm.Utility !== "undefined")) {
        var p = {};
        p.mcs_relatedsiteid = mcs_relatedsiteid;
        p.mcs_relatedsiteidname = mcs_relatedsiteidname;
        p.mcs_createproviderrg = mcs_createproviderrg;
        p.mcs_createpatientrg = mcs_createpatientrg;
        // p.mcs_tsaguid = mcs_tsaguid;
        // p.cvt_mastertsaguid = cvt_mastertsaguid;

        Xrm.Navigation.openForm("mcs_resourcegroup", null, p);
    }
    else {
        var features = "location=no,menubar=no,status=no,toolbar=no,resizable=yes";
        var extraqs = ["mcs_relatedsiteid=" + mcs_relatedsiteid, "mcs_relatedsiteidname=" + mcs_relatedsiteidname, "mcs_createproviderrg=" + mcs_createproviderrg, "mcs_createpatientrg=" + mcs_createpatientrg]
        //"mcs_tsaguid=" + mcs_tsaguid,
        //"cvt_mastertsaguid=" + cvt_mastertsaguid]
        var globalContext = Xrm.Utility.getGlobalContext();
        var url = globalContext.getClientUrl();
        //var url = Xrm.Page.context.getClientUrl();

        window.open(url + "/main.aspx?etn=mcs_resourcegroup&pagetype=entityrecord&extraqs=" + encodeURIComponent(extraqs.join("&")), "_blank", features, false);
    }
};

//Opens a window for a new Resource record using the Information form.
//Example: openNewmcs_resource("{undefined}","","",true,true);
MCS.mcs_Provider_Resource.openNewmcs_resource = function (mcs_relatedsiteid, mcs_relatedsiteidname, mcs_tsaguid, cvt_mastertsaguid, mcs_createpatientr, mcs_createproviderr) {
    //SD: web-use-strict-equality-operators
    if ((typeof Xrm !== "undefined") && (typeof Xrm.Utility !== "undefined")) {
        var p = {};
        p.mcs_relatedsiteid = mcs_relatedsiteid;
        p.mcs_relatedsiteidname = mcs_relatedsiteidname;
        p.mcs_createpatientr = mcs_createpatientr;
        p.mcs_createproviderr = mcs_createproviderr;
        //p.mcs_tsaguid = mcs_tsaguid;
        //p.cvt_mastertsaguid = cvt_mastertsaguid;
        Xrm.Navigation.openForm("mcs_resource", null, p);
    }
    else {
        var features = "location=no,menubar=no,status=no,toolbar=no,resizable=yes";
        var extraqs = ["mcs_relatedsiteid=" + mcs_relatedsiteid, "mcs_relatedsiteidname=" + mcs_relatedsiteidname, "mcs_createpatientr=" + mcs_createpatientr, "mcs_createproviderr=" + mcs_createproviderr]
        //"mcs_tsaguid=" + mcs_tsaguid,
        //"cvt_mastertsaguid=" + cvt_mastertsaguid]
        var globalContext = Xrm.Utility.getGlobalContext();
        var url = globalContext.getClientUrl();
        //var url = Xrm.Page.context.getClientUrl();

        window.open(url + "/main.aspx?etn=mcs_resource&pagetype=entityrecord&extraqs=" + encodeURIComponent(extraqs.join("&")), "_blank", features, false);
    }
};

//Opens a window for a new Provider Resource record using the Information form.
//Example: openNewcvt_providerresourcegroup("{undefined}","","{undefined}","","{undefined}","",0);
MCS.mcs_Provider_Resource.openNewcvt_providerresourcegroup = function (cvt_relatedsiteid, cvt_relatedsiteidname, cvt_relatedmastertsaid, cvt_relatedmastertsaidname, cvt_relatedtsaid, cvt_relatedtsaidname, cvt_capacityrequired) {
    //SD: web-use-strict-equality-operators
    if (typeof(MCS.mcs_TSA_OnLoad) !== "undefined") EntityName = MCS.mcs_TSA_OnLoad.EntityName;
    else if (typeof(MCS.cvt_MTSA_OnLoad) !== "undefined") EntityName = MCS.cvt_MTSA_OnLoad.EntityName;
    else if (typeof(window.parent.MCS) !== "undefined") {
        if (typeof(window.parent.MCS.mcs_TSA_OnLoad) !== "undefined") EntityName = window.parent.MCS.mcs_TSA_OnLoad.EntityName;
        else if (typeof(window.parent.MCS.cvt_MTSA_OnLoad) !== "undefined") EntityName = window.parent.MCS.cvt_MTSA_OnLoad.EntityName;
    }
    if ((typeof Xrm !== "undefined") && (typeof Xrm.Utility !== "undefined")) {
        //Checking which entity this is running on to determine the parameters to pass through.
        var p = {};
        p.cvt_relatedsiteid = cvt_relatedsiteid;
        p.cvt_relatedsiteidname = cvt_relatedsiteidname;
        /*  Master TSA deprecated
	    if (EntityName == "cvt_mastertsa") {
            p.cvt_relatedmastertsaid = cvt_relatedmastertsaid;
            p.cvt_relatedmastertsaidname = cvt_relatedmastertsaidname;
        }*/
        /* TSA Deprecated
        if (EntityName == "mcs_services") {
            p.cvt_relatedtsaid = cvt_relatedtsaid;
            p.cvt_relatedtsaidname = cvt_relatedtsaidname;
        }
		*/
        p.cvt_capacityrequired = cvt_capacityrequired;
        Xrm.Navigation.openForm("cvt_providerresourcegroup", null, p);
    }
    else {
        var features = "location=no,menubar=no,status=no,toolbar=no,resizable=yes";

        var extraqs = [];
        /* MTSA & TSA Deprecated
        if (EntityName == "cvt_mastertsa") {
            extraqs = ["cvt_relatedsiteid=" + cvt_relatedsiteid,
            "cvt_relatedsiteidname=" + cvt_relatedsiteidname,
            "cvt_relatedmastertsaid=" + cvt_relatedmastertsaid,
            "cvt_relatedmastertsaidname=" + cvt_relatedmastertsaidname,
            "cvt_capacityrequired=" + cvt_capacityrequired];
        }
        if (EntityName == "mcs_services") {
            extraqs = ["cvt_relatedsiteid=" + cvt_relatedsiteid,
            "cvt_relatedsiteidname=" + cvt_relatedsiteidname,
            "cvt_relatedtsaid=" + cvt_relatedtsaid,
            "cvt_relatedtsaidname=" + cvt_relatedtsaidname,
            "cvt_capacityrequired=" + cvt_capacityrequired];
        }
		*/
        var globalContext = Xrm.Utility.getGlobalContext();
        var url = globalContext.getClientUrl();
        //var url = Xrm.Page.context.getClientUrl();

        window.open(url + "/main.aspx?etn=cvt_providerresourcegroup&pagetype=entityrecord&extraqs=" + encodeURIComponent(extraqs.join("&")), "_blank", features, false);
    }
};

//Same button, 2 places.  Depending on the place we want to pass the ID differently
MCS.mcs_Provider_Resource.QuickCreateProviderResourceGroup = function () {
    MCS.mcs_Provider_Resource.gatherParameters();
    //TSA
    //SD: web-use-strict-equality-operators
    if (MCS.mcs_Provider_Resource.EntityName === "mcs_services") MCS.mcs_Provider_Resource.openNewmcs_resourcegroup(MCS.mcs_Provider_Resource.relatedProviderSiteId, MCS.mcs_Provider_Resource.relatedProviderSiteName, true, false, MCS.mcs_Provider_Resource.EntityId, null);

    //MTSA
    else MCS.mcs_Provider_Resource.openNewmcs_resourcegroup(MCS.mcs_Provider_Resource.relatedProviderSiteId, MCS.mcs_Provider_Resource.relatedProviderSiteName, true, false, null, MCS.mcs_Provider_Resource.EntityId);
};

//Same button, 2 places.  Depending on the place we want to pass the ID differently
MCS.mcs_Provider_Resource.QuickCreateProviderResource = function () {
    MCS.mcs_Provider_Resource.gatherParameters();
    //TSA
    //SD: web-use-strict-equality-operators
    if (MCS.mcs_Provider_Resource.EntityName === "mcs_services") MCS.mcs_Provider_Resource.openNewmcs_resource(MCS.mcs_Provider_Resource.relatedProviderSiteId, MCS.mcs_Provider_Resource.relatedProviderSiteName, MCS.mcs_Provider_Resource.EntityId, null, false, true);
    //MTSA
    else MCS.mcs_Provider_Resource.openNewmcs_resource(MCS.mcs_Provider_Resource.relatedProviderSiteId, MCS.mcs_Provider_Resource.relatedProviderSiteName, null, MCS.mcs_Provider_Resource.EntityId, false, true);
};

//Same button, 2 places.  Depending on the place we want to pass the ID differently
MCS.mcs_Provider_Resource.AddProviderResource = function () {
    MCS.mcs_Provider_Resource.gatherParameters();
    //TSA
    //SD: web-use-strict-equality-operators
    if (MCS.mcs_Provider_Resource.EntityName === "mcs_services") MCS.mcs_Provider_Resource.openNewcvt_providerresourcegroup(MCS.mcs_Provider_Resource.relatedProviderSiteId, MCS.mcs_Provider_Resource.relatedProviderSiteName, null, null, MCS.mcs_Provider_Resource.EntityId, MCS.mcs_Provider_Resource.TSAName, 1);

    else MCS.mcs_Provider_Resource.openNewcvt_providerresourcegroup(MCS.mcs_Provider_Resource.relatedProviderSiteId, MCS.mcs_Provider_Resource.relatedProviderSiteName, MCS.mcs_Provider_Resource.EntityId, MCS.mcs_Provider_Resource.MTSAName, null, null, 1);
};

MCS.mcs_Provider_Resource.gatherParameters = function () {
    //Set the right depth to get the variables
    //SD: web-use-strict-equality-operators
    if (typeof(MCS.cvt_Common) === "undefined") MCS = window.parent.MCS;
    //Determine if TSA/MTSA, variable names are different
    //SD: web-use-strict-equality-operators
    if (typeof(MCS.mcs_TSA_OnLoad) !== "undefined") {
        //TSA
        MCS.mcs_Provider_Resource.relatedProviderSiteId = MCS.mcs_TSA_OnLoad.relatedProviderSiteId;
        MCS.mcs_Provider_Resource.relatedProviderSiteName = MCS.mcs_TSA_OnLoad.relatedProviderSiteName;
        MCS.mcs_Provider_Resource.EntityId = MCS.mcs_TSA_OnLoad.EntityId;
        MCS.mcs_Provider_Resource.EntityName = MCS.mcs_TSA_OnLoad.EntityName;
        MCS.mcs_Provider_Resource.TSAName = MCS.mcs_TSA_OnLoad.TSAName;
    }
    else { //MTSA
        MCS.mcs_Provider_Resource.relatedProviderSiteId = MCS.cvt_MTSA_OnLoad.relatedProviderSiteId;
        MCS.mcs_Provider_Resource.relatedProviderSiteName = MCS.cvt_MTSA_OnLoad.relatedProviderSiteName;
        MCS.mcs_Provider_Resource.EntityId = MCS.cvt_MTSA_OnLoad.EntityId;
        MCS.mcs_Provider_Resource.EntityName = MCS.cvt_MTSA_OnLoad.EntityName;
        MCS.mcs_Provider_Resource.MTSAName = MCS.cvt_MTSA_OnLoad.MTSAName;
    }
};