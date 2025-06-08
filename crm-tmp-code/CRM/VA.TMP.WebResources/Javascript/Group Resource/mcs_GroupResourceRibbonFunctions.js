function openNewmcs_resource(mcs_relatedsiteid, mcs_relatedsiteidname, mcs_resourcegroupguid) {
    'use strict';
    //  var formContext = executionContext.getFormContext();
    //valid values for type align with resource type flobal option set
    if ((typeof Xrm !== "undefined") && (typeof Xrm.Utility !== "undefined")) {
        var p = {};
        // p.mcs_resourcegroupguid = mcs_resourcegroupguid;
        p["mcs_resourcegroupguid"] = mcs_resourcegroupguid;
        //if (mcs_type !== 0)
        //    p.mcs_type = mcs_type;
        //p["mcs_type"] = mcs_type.toString();
        if (mcs_relatedsiteid !== "")
            //p.mcs_relatedsiteid = mcs_relatedsiteid;
            p["mcs_relatedsiteid"] = mcs_relatedsiteid;
        if (mcs_relatedsiteidname !== "")
            //p.mcs_relatedsiteidname = mcs_relatedsiteidname;
            p["mcs_relatedsiteidname"] = mcs_relatedsiteidname;
        // Xrm.Navigation.openForm("mcs_resource", null, p);

        //Xrm.Navigation.openForm("mcs_resource", p);

        var entityFormOptions = {};
        entityFormOptions["entityName"] = "mcs_resource";

        Xrm.Navigation.openForm(entityFormOptions, p);
    }
    else {
        var features = "location=no,menubar=no,status=no,toolbar=no,resizable=yes";
        var extraqs = ["mcs_resourcegroupguid=" + mcs_resourcegroupguid];
        if (mcs_relatedsiteid !== "")
            extraqs.push("mcs_relatedsiteid=" + mcs_relatedsiteid);
        if (mcs_relatedsiteidname !== "")
            extraqs.push("mcs_relatedsiteidname=" + mcs_relatedsiteidname);
        //if (mcs_type !== 0)
        //    extraqs.push("mcs_type=" + mcs_type);

        var globalContext = Xrm.Utility.getGlobalContext();
        var url = globalContext.getClientUrl();
        window.open(url + "/main.aspx?etn=mcs_resource&pagetype=entityrecord&extraqs=" + encodeURIComponent(extraqs.join("&")), "_blank", features, false);
    }
}

//Example:
//openNewmcs_resource(251920001,"{9352b512-7686-e211-ba96-00155d010613}","boston",105,"wewer");function QuickCreateResource() {
function QuickCreateResource(formContext) {
    if (formContext !== null) {
        //var formContext = executionContext.getFormContext();
        var relatedSite = formContext.getAttribute("mcs_relatedsiteid") != null ? formContext.getAttribute("mcs_relatedsiteid").getValue()[0] : { id: "", name: "" }; //get the site object or set up an object will empty values
        var resourceType = formContext.getAttribute("mcs_type") != null ? formContext.getAttribute("mcs_type").getValue() : 0;

        if (resourceType === 99999999 || resourceType === 100000000) {
            //alert("To Create a Provider/Telepresenter, contact NTTHD at 866-651-3180 or email VHA_NTTHD@va.gov"); --Modified by Naveen Dubbaka on 09/21/2020
            var alertOptions = { height: 200, width: 300 };
            var alertStrings = { confirmButtonLabel: "OK", text: "To Create a Provider/Telepresenter, contact NTTHD at 866-651-3180 or email VHA_NTTHD@va.gov", title: "Quick Create Resource Rules." };
            Xrm.Navigation.openAlertDialog(alertStrings, alertOptions);
            return;
        }
    }
    openNewmcs_resource(relatedSite.id, relatedSite.name, formContext.data.entity.getId());
}// JavaScript source code
