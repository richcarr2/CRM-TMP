//Library Name: cvt_KPI.OnLoad.js
//If the SDK namespace object is not defined, create it.
//SD: web-use-strict-equality-operators
if (typeof (MCS) === "undefined") { MCS = {}; }
// Create Namespace container for functions in this library;
MCS.cvt_KPI_OnLoad = {};

MCS.cvt_KPI_OnLoad.OnLoad = function (executionContext) {
    //SD - Start
    //web-use-strict-mode
    "use strict";
    //SD - End
    var formContext = executionContext.getFormContext();
    //SD: web-use-strict-equality-operators
    if (formContext.ui.getFormType() === 1) { //Create
        {
            MCS.cvt_Common.Notifications("Ok", 'Please determine if this is a template.');

            //Editable only on create
            formContext.getControl("cvt_template").setDisabled(false);
            formContext.getControl("cvt_responsetype").setDisabled(false);
        }
    }
    //If not a template
    //SD: web-use-strict-equality-operators
    if (formContext.getAttribute("cvt_template").getValue() !== true) {
        formContext.ui.tabs.get('tab_general').sections.get('section_response').setVisible(true);
        formContext.ui.tabs.get('tab_3').sections.get('section_qc').setVisible(true);

        //If not - include correct reponse field
        if (formContext.getAttribute("cvt_responsetype").getValue() != null) {
            //make one visible
            switch (formContext.getAttribute("cvt_responsetype").getValue()) {
                case 917290000: //Boolean
                    formContext.getControl("cvt_responseboolean").setVisible(true);
                    break;
                case 917290001: //Whole Number
                    formContext.getControl("cvt_responsewholenumber").setVisible(true);
                    break;
                case 917290002: //Date
                    formContext.getControl("cvt_responsedate").setVisible(true);
                    break;
                case 917290003: //Text
                    formContext.getControl("cvt_responsetext").setVisible(true);
                    break;
            }
        }

        //Hide response type
        formContext.getControl("cvt_responsetype").setDisabled(true);
    }
    else { //Template
        formContext.ui.tabs.get('tab_general').sections.get('section_setup').setVisible(true);
        MCS.cvt_Common.Notifications("Add", 3, 'Record is a template.');
    }
};

onSave = function (context) {
    var saveEvent = context.getEventArgs();
    //SD: web-remove-debug-script
    //alert("Prevent Save");
    saveEvent.preventDefault();
    return false;
};