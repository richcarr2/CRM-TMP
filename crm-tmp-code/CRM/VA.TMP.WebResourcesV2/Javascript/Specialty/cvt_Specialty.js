 //If the SDK namespace object is not defined, create it.
//SD: web-use-strict-equality-operators
if (typeof(MCS) === "undefined") {
    MCS = {};
}
// Create Namespace container for functions in this library;
MCS.Specialty = {};

MCS.Specialty.DisableVVS = function (executionContext) {
    //SD - Start
    //web-use-strict-mode
    "use strict";
    //SD - End
    var formContext = executionContext.getFormContext();
    // var hasRole = MCS.cvt_Common.userHasRoleInList("System Administrator");
    var hasRoleDeferred = MCS.cvt_Common.userHasRoleInList("System Administrator");
    $.when(hasRoleDeferred).done(function (returnData) {
        var hasRole = returnData.data.result;
        var vvsControl = formContext.getControl("cvt_usevvs");
        vvsControl.setDisabled(!hasRole);
    });
}