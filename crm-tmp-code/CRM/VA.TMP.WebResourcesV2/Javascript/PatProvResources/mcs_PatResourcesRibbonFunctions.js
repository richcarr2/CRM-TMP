//If the SDK namespace object is not defined, create it.
//SD: web-use-strict-equality-operators
if (typeof(MCS) === "undefined") {
    MCS = {};
}
// Create Namespace container for functions in this library;
MCS.mcs_Patient_Resource = {};

//Namespace Variables
MCS.mcs_Patient_Resource.relatedPatientSiteId;
MCS.mcs_Patient_Resource.relatedPatientSiteName;
MCS.mcs_Patient_Resource.EntityId;
MCS.mcs_Patient_Resource.TSAName;
MCS.mcs_Patient_Resource.GroupAppt;

//Opens a window for a new Resource Group record using the Information form.
//Example: openNewmcs_resourcegroup("{undefined}","",true,true,"");
MCS.mcs_Patient_Resource.openNewmcs_resourcegroup = function (mcs_relatedsiteid, mcs_relatedsiteidname, mcs_createproviderrg, mcs_createpatientrg, mcs_tsaguid) {
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
        p.mcs_tsaguid = mcs_tsaguid;
        Xrm.Navigation.openForm("mcs_resourcegroup", null, p);
    }
    else {
        var features = "location=no,menubar=no,status=no,toolbar=no,resizable=yes";
        var extraqs = ["mcs_relatedsiteid=" + mcs_relatedsiteid, "mcs_relatedsiteidname=" + mcs_relatedsiteidname, "mcs_createproviderrg=" + mcs_createproviderrg, "mcs_createpatientrg=" + mcs_createpatientrg, "mcs_tsaguid=" + mcs_tsaguid]
        var globalContext = Xrm.Utility.getGlobalContext();
        var url = globalContext.getClientUrl();
        //var url = Xrm.Page.context.getClientUrl();
        window.open(url + "/main.aspx?etn=mcs_resourcegroup&pagetype=entityrecord&extraqs=" + encodeURIComponent(extraqs.join("&")), "_blank", features, false);
    }
};

//Opens a window for a new Resource record using the Information form.
//Example: openNewmcs_resource("{undefined}","","",true,true);
MCS.mcs_Patient_Resource.openNewmcs_resource = function (mcs_relatedsiteid, mcs_relatedsiteidname, mcs_tsaguid, mcs_createpatientr, mcs_createproviderr) {
    //SD: web-use-strict-equality-operators
    if ((typeof Xrm !== "undefined") && (typeof Xrm.Utility !== "undefined")) {
        var p = {};
        p.mcs_relatedsiteid = mcs_relatedsiteid;
        p.mcs_relatedsiteidname = mcs_relatedsiteidname;
        p.mcs_tsaguid = mcs_tsaguid;
        p.mcs_createpatientr = mcs_createpatientr;
        p.mcs_createproviderr = mcs_createproviderr;
        Xrm.Navigation.openForm("mcs_resource", null, p);
    }
    else {
        var features = "location=no,menubar=no,status=no,toolbar=no,resizable=yes";
        var extraqs = ["mcs_relatedsiteid=" + mcs_relatedsiteid, "mcs_relatedsiteidname=" + mcs_relatedsiteidname, "mcs_tsaguid=" + mcs_tsaguid, "mcs_createpatientr=" + mcs_createpatientr, "mcs_createproviderr=" + mcs_createproviderr]
        var globalContext = Xrm.Utility.getGlobalContext();
        var url = globalContext.getClientUrl();
        // var url = Xrm.Page.context.getClientUrl();
        window.open(url + "/main.aspx?etn=mcs_resource&pagetype=entityrecord&extraqs=" + encodeURIComponent(extraqs.join("&")), "_blank", features, false);
    }
};

//Opens a window for a new Patient Resource record using the Information form.
MCS.mcs_Patient_Resource.openNewcvt_patientresourcegroupTSA = function (cvt_relatedsiteid, cvt_relatedsiteidname, cvt_capacityrequired, cvt_relatedtsaid, cvt_relatedtsaidname) {
    //SD: web-use-strict-equality-operators
    if ((typeof Xrm !== "undefined") && (typeof Xrm.Utility !== "undefined")) {
        var p = {};

        //p.cvt_relatedsiteid = cvt_relatedsiteid;
        //p.cvt_relatedsiteidname = cvt_relatedsiteidname;
        //p.cvt_capacityrequired = cvt_capacityrequired;
        //p.cvt_relatedtsaid = cvt_relatedtsaid;
        //p.cvt_relatedtsaidname = cvt_relatedtsaidname;
        //Xrm.Navigation.openForm("cvt_patientresourcegroup", null, p);
        //Modified by Naveen Dubbaka
        p["cvt_relatedsiteid"] = cvt_relatedsiteid;
        p["cvt_relatedsiteidname"] = cvt_relatedsiteidname;
        p["cvt_capacityrequired"] = cvt_capacityrequired;
        p["cvt_relatedtsaid"] = cvt_relatedtsaid;
        p["cvt_relatedtsaidname"] = cvt_relatedtsaidname;

        var entityFormOptions = {};
        entityFormOptions["entityName"] = "cvt_patientresourcegroup";

        Xrm.Navigation.openForm(entityFormOptions, p);

    }
    else {
        var features = "location=no,menubar=no,status=no,toolbar=no,resizable=yes";
        var extraqs = ["cvt_relatedsiteid=" + cvt_relatedsiteid, "cvt_relatedsiteidname=" + cvt_relatedsiteidname, "cvt_capacityrequired=" + cvt_capacityrequired, "cvt_relatedtsaid=" + cvt_relatedtsaid, "cvt_relatedtsaidname=" + cvt_relatedtsaidname]
        var globalContext = Xrm.Utility.getGlobalContext();
        var url = globalContext.getClientUrl();
        //var url = Xrm.Page.context.getClientUrl();
        window.open(url + "/main.aspx?etn=cvt_patientresourcegroup&pagetype=entityrecord&extraqs=" + encodeURIComponent(extraqs.join("&")), "_blank", features, false);
    }
};

//Opens a window for a new Patient Site Resource record using the Information form.
MCS.mcs_Patient_Resource.openNewcvt_patientresourcegroupGroupAppt = function (cvt_capacityrequired, cvt_relatedtsaid, cvt_relatedtsaidname) {
    //SD: web-use-strict-equality-operators
    if ((typeof Xrm !== "undefined") && (typeof Xrm.Utility !== "undefined")) {
        var p = {};
        //  p.formid = "fdb4a4ff-ba87-49d4-8f8e-1d457cd2e278"
        //p.cvt_capacityrequired = cvt_capacityrequired;
        //p.cvt_relatedtsaid = cvt_relatedtsaid;
        //p.cvt_relatedtsaidname = cvt_relatedtsaidname;
        //Xrm.Navigation.openForm("cvt_patientresourcegroup",p);
        //Modified by Naveen Dubbaka
        p["cvt_capacityrequired"] = cvt_capacityrequired;
        p["cvt_relatedtsaid"] = cvt_relatedtsaid;
        p["cvt_relatedtsaidname"] = cvt_relatedtsaidname;

        var entityFormOptions = {};
        entityFormOptions["entityName"] = "cvt_patientresourcegroup";

        Xrm.Navigation.openForm(entityFormOptions, p);
    }
    else {
        var features = "location=no,menubar=no,status=no,toolbar=no,resizable=yes";
        var extraqs = ["formid=fdb4a4ff-ba87-49d4-8f8e-1d457cd2e278", "cvt_capacityrequired=" + cvt_capacityrequired, "cvt_relatedtsaid=" + cvt_relatedtsaid, "cvt_relatedtsaidname=" + cvt_relatedtsaidname]
        var globalContext = Xrm.Utility.getGlobalContext();
        var url = globalContext.getClientUrl();
        //var url = Xrm.Page.context.getClientUrl();
        window.open(url + "/main.aspx?etn=cvt_patientresourcegroup&pagetype=entityrecord&extraqs=" + encodeURIComponent(extraqs.join("&")), "_blank", features, false);
    }
};

MCS.mcs_Patient_Resource.QuickCreatePatientResource = function () {
    MCS.mcs_Patient_Resource.gatherParameters();
    MCS.mcs_Patient_Resource.openNewmcs_resource(MCS.mcs_Patient_Resource.relatedPatientSiteId, MCS.mcs_Patient_Resource.relatedPatientSiteName, MCS.mcs_Patient_Resource.EntityId, true, false);
};

MCS.mcs_Patient_Resource.QuickCreatePatientResourceGroup = function () {
    MCS.mcs_Patient_Resource.gatherParameters();
    MCS.mcs_Patient_Resource.openNewmcs_resourcegroup(MCS.mcs_Patient_Resource.relatedPatientSiteId, MCS.mcs_Patient_Resource.relatedPatientSiteName, false, true, MCS.mcs_Patient_Resource.EntityId);
};

MCS.mcs_Patient_Resource.AddPatientResource = function () {
    MCS.mcs_Patient_Resource.gatherParameters();
    //SD: web-use-strict-equality-operators
    //if (MCS.mcs_Patient_Resource.GroupAppt === 0) //Modified by Naveen Dubbaka
    if (MCS.mcs_Patient_Resource.GroupAppt === false) MCS.mcs_Patient_Resource.openNewcvt_patientresourcegroupTSA(MCS.mcs_Patient_Resource.relatedPatientSiteId, MCS.mcs_Patient_Resource.relatedPatientSiteName, 1, MCS.mcs_Patient_Resource.EntityId, MCS.mcs_Patient_Resource.TSAName);
    else MCS.mcs_Patient_Resource.openNewcvt_patientresourcegroupGroupAppt(1, MCS.mcs_Patient_Resource.EntityId, MCS.mcs_Patient_Resource.TSAName);
};

MCS.mcs_Patient_Resource.gatherParameters = function () {
    //Set the right depth to get the variables
    //SD: web-use-strict-equality-operators
    if (typeof(MCS.cvt_Common) === "undefined") MCS = window.parent.MCS;

    MCS.mcs_Patient_Resource.relatedPatientSiteId = MCS.mcs_TSA_OnLoad.relatedPatientSiteId;
    MCS.mcs_Patient_Resource.relatedPatientSiteName = MCS.mcs_TSA_OnLoad.relatedPatientSiteName;
    MCS.mcs_Patient_Resource.EntityId = MCS.mcs_TSA_OnLoad.EntityId;
    MCS.mcs_Patient_Resource.TSAName = MCS.mcs_TSA_OnLoad.TSAName;
    MCS.mcs_Patient_Resource.GroupAppt = MCS.mcs_TSA_OnLoad.GroupAppt;
};