//Library Name: cvt_QualityCheck.OnChange.js
//If the SDK namespace object is not defined, create it.
if (typeof (MCS) === "undefined") { MCS = {}; }
// Create Namespace container for functions in this library;
MCS.QualityCheck_OnChange = {};

//Make Flag mandatory based on Status Reason Submitted
MCS.QualityCheck_OnChange.FlagStatusReason = function (executionContext) {
    'use strict';
    var formContext = executionContext.getFormContext();
    var statusReason = (formContext.getAttribute('statuscode').getValue() != null) ? formContext.getAttribute('statuscode').getSelectedOption().value : null;
    var typeOfPrivileging = null;

    /*
    Draft	    1
    Submitted	917290000
    Inactive	0
    */
    var thisrecord = formContext.data.entity.getId();
    switch (statusReason) {
        case 917290000: //Submitted
            //Should we verify there are KPI records?

            //Search for child records with red flag
            var filter = "statuscode/Value eq 1 and cvt_QualityCheckId/Id eq (Guid'" + thisrecord + "') and cvt_Flag/Value eq 917290001";

            Xrm.WebApi.retrieveMultipleRecords("cvt_kpi", "?$select=cvt_Indicator,cvt_kpiId&$filter=" + filter).then(
                function success(redFlagData) {
                    if (redFlagData && redFlagData.length > 0) {
                        //Results, therefore RED
                        //alert('Flag is set to Red because a KPI is flagged Red.');
                        formContext.getAttribute("cvt_flag").setValue(917290001);
                        formContext.getAttribute("cvt_flag").setSubmitMode('always');

                        //Refresh the WebResource
                        var wrControl = formContext.ui.controls.get("WebResource_KPIform");
                        wrControl.setSrc(wrControl.getSrc());
                    }
                    else { //Set to Green
                        formContext.getAttribute("cvt_flag").setValue(917290000);
                        formContext.getAttribute("cvt_flag").setSubmitMode('always');

                        //Refresh the WebResource
                        var wrControl = formContext.ui.controls.get("WebResource_KPIform");
                        wrControl.setSrc(wrControl.getSrc());
                    }
                },
                function (error) {
                    //console.log(error.message);
                    //// handle error conditions
                }
            );

        //CrmRestKit.ByQuery("cvt_kpi", ['cvt_Indicator', 'cvt_kpiId'], filter, true)
        //.fail(function (error) {
        //    //return;
        //})
        //.done(function (redFlagData) {
        //    //Successful query on conflict search
        //    if (redFlagData && redFlagData.d.results && redFlagData.d.results.length > 0) {
        //        //Results, therefore RED
        //        //alert('Flag is set to Red because a KPI is flagged Red.');
        //        Xrm.Page.getAttribute("cvt_flag").setValue(917290001);
        //        Xrm.Page.getAttribute("cvt_flag").setSubmitMode('always');

        //        //Refresh the WebResource
        //        var wrControl = Xrm.Page.ui.controls.get("WebResource_KPIform");
        //        wrControl.setSrc(wrControl.getSrc());
        //    }
        //    else { //Set to Green
        //        Xrm.Page.getAttribute("cvt_flag").setValue(917290000);
        //        Xrm.Page.getAttribute("cvt_flag").setSubmitMode('always');

        //        //Refresh the WebResource
        //        var wrControl = Xrm.Page.ui.controls.get("WebResource_KPIform");
        //        wrControl.setSrc(wrControl.getSrc());
        //    }
        //});
    }
};

//Description: TSS Privileging Lookup and set Facility
MCS.QualityCheck_OnChange.TSSPrivLookup = function (executionContext) {
    var formContext = executionContext.getFormContext();
    var statusReason = null;
    if (formContext.getAttribute('cvt_tssprivilegingid').getValue() != null) {
        privLookup = formContext.getAttribute('cvt_tssprivilegingid').getValue()[0].id;
    }
    else {
        return;
    }
    Xrm.WebApi.retrieveRecord("cvt_tssprivileging", privLookup, "?$select=cvt_PrivilegedAtId,cvt_ProviderId").then(
        function success(result) {
            if (result) {
                //Check and Set Facility
                var value = new Array();
                value[0] = new Object();
                value[0].id = '{' + result.cvt_PrivilegedAtId.Id + '}';
                value[0].name = result.cvt_PrivilegedAtId.Name;
                value[0].entityType = "mcs_facility";

                var providervalue = new Array();
                providervalue[0] = new Object();
                providervalue[0].id = '{' + result.cvt_ProviderId.Id + '}';
                providervalue[0].name = result.cvt_ProviderId.Name;
                providervalue[0].entityType = "systemuser";

                formContext.data.entity.attributes.get('cvt_facilityid').setSubmitMode('always');
                formContext.data.entity.attributes.get('cvt_facilityid').setValue(value);

                formContext.data.entity.attributes.get('cvt_providerid').setSubmitMode('always');
                formContext.data.entity.attributes.get('cvt_providerid').setValue(providervalue);
            }
        },
        function (error) {
        }
    );

    //var calls = CrmRestKit.Retrieve("cvt_tssprivileging", privLookup, ['cvt_PrivilegedAtId', 'cvt_ProviderId'], false);
    //calls.fail(
    //        function (error) {
    //}).done(function (data) {
    //    if (data && data.d) {
    //        //Check and Set Facility
    //        var value = new Array();
    //        value[0] = new Object();
    //        value[0].id = '{' + data.d.cvt_PrivilegedAtId.Id + '}';
    //        value[0].name = data.d.cvt_PrivilegedAtId.Name;
    //        value[0].entityType = "mcs_facility";

    //        var providervalue = new Array();
    //        providervalue[0] = new Object();
    //        providervalue[0].id = '{' + data.d.cvt_ProviderId.Id + '}';
    //        providervalue[0].name = data.d.cvt_ProviderId.Name;
    //        providervalue[0].entityType = "systemuser";

    //        Xrm.Page.data.entity.attributes.get('cvt_facilityid').setSubmitMode('always');
    //        Xrm.Page.data.entity.attributes.get('cvt_facilityid').setValue(value);

    //        Xrm.Page.data.entity.attributes.get('cvt_providerid').setSubmitMode('always');
    //        Xrm.Page.data.entity.attributes.get('cvt_providerid').setValue(providervalue);
    //    }
    //});
};