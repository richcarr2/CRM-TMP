//Library Name: cvt_QualityCheck.OnLoad.js
//If the SDK namespace object is not defined, create it.
if (typeof (MCS) === "undefined") { MCS = {}; }
// Create Namespace container for functions in this library;
MCS.QualityCheck_OnLoad = {};

//Check Status to make entire form readonly.
MCS.QualityCheck_OnLoad.OnLoad = function (executionContext) {
    'use strict';
    var formContext = executionContext.getFormContext();
    if (formContext.ui.getFormType() === 1) {
        formContext.getControl("statuscode").setDisabled(true);
    }

    var statusReason = (formContext.data.entity.attributes.get('statuscode').getValue() != null) ? formContext.data.entity.attributes.get('statuscode').getSelectedOption().value : null;
    if (statusReason === 917290000) {
        formContext.getControl("cvt_flag").setDisabled(true);
        formContext.getControl("cvt_type").setDisabled(true);
        formContext.getControl("cvt_facilityid").setDisabled(true);
        formContext.getControl("statuscode").setDisabled(true);
        //formContext.getControl("cvt_questionsresponses").setDisabled(true);
        formContext.getControl("cvt_tssprivilegingid").setDisabled(true);
        formContext.getControl("ownerid").setDisabled(true);

        var notificationsList = Sys.Application.findComponent('crmNotifications');
        if (notificationsList) {
            notificationsList.AddNotification('noteId1', 3, 'namespace', 'FPPE/OPPE record has already been submitted.');
            //Please "Share" this record with other AOs at your site and service line.
        }
    }
};
