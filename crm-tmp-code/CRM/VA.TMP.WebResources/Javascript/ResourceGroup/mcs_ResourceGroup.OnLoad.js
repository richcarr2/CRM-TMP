//If the SDK namespace object is not defined, create it.
if (typeof (MCS) === "undefined") { MCS = {}; }
// Create Namespace container for functions in this library;
MCS.mcs_Resource_Group = {};
//DO NOT MANUALLY ADD CODE TO THIS FILE AS THIS FILE IS COMPLETELY RE-WRITTEN FROM CRM Rules! EVERY TIME THIS ENTITY IS DEPLOYED
MCS.mcs_Resource_Group.FORM_TYPE_CREATE = 1;
MCS.mcs_Resource_Group.FORM_TYPE_UPDATE = 2;
MCS.mcs_Resource_Group.FORM_TYPE_READ_ONLY = 3;
MCS.mcs_Resource_Group.FORM_TYPE_DISABLED = 4;

MCS.mcs_Resource_Group.RemoveVistaClincOption = function (executionContext) {
    'use strict';
    /***********************************************************************
    /** 
    /** Description: Removes the Provider and Telepresenter options from the Option Set. 
    /** 
    ***********************************************************************/
    var formContext = executionContext.getFormContext();
    var options = formContext.ui.controls.get("mcs_type");
    options.removeOption(251920000);

};

