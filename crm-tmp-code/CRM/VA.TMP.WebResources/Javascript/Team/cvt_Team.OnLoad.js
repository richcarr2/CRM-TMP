if (typeof MCS === "undefined")
    MCS = {};
// Create Namespace container for functions in this library;
if (typeof MCS.Team === "undefined")
    MCS.Team = {};
MCS.Team.OnLoad = {};

MCS.Team.OnLoad.SetAdmin = function (executionContext) {
    'use strict';
    var formContext = executionContext.getFormContext();
    var globalContext = Xrm.Utility.getGlobalContext();
    var userName = "";
    var currentAdmin = formContext.getAttribute("administratorid");
    if (currentAdmin.getValue() != null)
        return;

    //Xrm.WebApi.retrieveRecord("systemuser", globalContext.userSettings.userId, "?$select=systemuserid,fullname)").then(
    //    function success(result) {
    //        //userName = result.FullName;
    //        userName = result["fullname"];
    //        //var newAdmin = { id: globalContext.userSettings.userId, entityType: 'systemuser', name: userName };
    //        //currentAdmin.setValue(newAdmin);
    //    },
    //    function (error) {
    //        //console.log(error.message);
    //    }
    //);
    //Modified by Naveen Dubbaka 09/22/2020
    Xrm.WebApi.retrieveRecord("systemuser", Xrm.Utility.getGlobalContext().userSettings.userId, "?$select=fullname").then(
        function success(result) {
            userName = result["fullname"];
            formContext.getAttribute("administratorid").setValue([{ id: globalContext.userSettings.userId, entityType: 'systemuser', name: userName }]);
        },
        function (error) {
            Xrm.Utility.alertDialog(error.message);
        }
    );
    //var newAdmin = { id: globalContext.userSettings.userId, entityType: 'SystemUser', name: userName };
    //currentAdmin.setValue([newAdmin]);
};

MCS.Team.OnLoad.LockFields = function (executionContext) {
    var formContext = executionContext.getFormContext();
    if (formContext.ui.getFormType() !== 1) {
        formContext.getControl("cvt_type").setDisabled(true);
        formContext.getControl("cvt_facility").setDisabled(true);
        formContext.getControl("cvt_servicetype").setDisabled(true);
    }
};

