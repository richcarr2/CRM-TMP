//If the SDK namespace object is not defined, create it.
if (typeof (MCS) == "undefined") { MCS = {}; }
// Create Namespace container for functions in this library;
MCS.Patient = {};

MCS.Patient.FormOnload = function (executionContext) {
    var formContext = executionContext.getFormContext();
    MCS.Patient.SetCommunicationFieldsVisibility(executionContext);
    formContext.getAttribute("cvt_tablettype").addOnChange(MCS.Patient.SetCommunicationFieldsVisibility);
    formContext.getAttribute("donotemail").addOnChange(MCS.Patient.DoNotEmailOnChange);

    var fieldsToLock = ["lastname", "firstname", "suffix", "middlename", "salutation", "mcs_othernames",
        "telephone2", "telephone1",
        "address1_line1", "address1_line2", "address1_line3", "address1_city", "address1_stateorprovince", "address1_postalcode", "address1_country",
        "gendercode", "birthdate", "familystatuscode", "mcs_branchofservice", "mcs_deceased", "mcs_deceaseddate"];
    formContext.data.entity.attributes.forEach(function (attribute, index) {
        var control = Xrm.Page.getControl(attribute.getName());
        if (fieldsToLock.indexOf(attribute.getName()) !== -1) {
            control.setDisabled(true);
        }
    });
};

MCS.Patient.SetCommunicationFieldsVisibility = function (executionContext) {
    //Set Default Visibility
    var formContext = executionContext.getFormContext();
    var sipAddress = formContext.getControl("cvt_bltablet");
    var sipAddressAttribute = formContext.getAttribute("cvt_bltablet");
    var email = formContext.getControl("emailaddress1");
    var emailAttribute = formContext.getAttribute("emailaddress1");
    var donotemail = formContext.getControl("donotemail");
    var donotemailAttribute = formContext.getAttribute("donotemail");
    var staticvmrlinkControl = formContext.getControl("cvt_staticvmrlink");
    var staticvmrlinkControlAttribute = formContext.getAttribute("cvt_staticvmrlink");

    sipAddress.setVisible(true);
    sipAddressAttribute.setRequiredLevel("none");
    email.setVisible(true);
    emailAttribute.setRequiredLevel("none");
    donotemail.setVisible(true);
    donotemailAttribute.setRequiredLevel("required");
    staticvmrlinkControl.setVisible(true);
    staticvmrlinkControlAttribute.setRequiredLevel("none");

    var techType = formContext.getAttribute("cvt_tablettype").getValue();
    switch (techType) {
        case 100000000: //SIP Device (CVT Tablet and COTS Tablet)
            sipAddressAttribute.setRequiredLevel("required");
            sipAddress.setDisabled(false);
            donotemailAttribute.setValue(true);
            email.setDisabled(true);
            staticvmrlinkControl.setDisabled(true);
            break;
        case 917290002: //VA Issued iOS Device
            sipAddress.setDisabled(true);
            donotemail.setDisabled(false);
            MCS.Patient.DoNotEmailOnChange(executionContext); //TODO: Conditionals
            break;
        case 917290003: //Personal VA Video Connect Device
            sipAddress.setDisabled(true);
            donotemailAttribute.setValue(false);
            donotemail.setDisabled(true);
            emailAttribute.setRequiredLevel("required");
            email.setDisabled(false);
            staticvmrlinkControl.setDisabled(true);
            break;
    }
};

MCS.Patient.DoNotEmailOnChange = function (executionContext) {
    var formContext = executionContext.getFormContext();
    var donotemail = formContext.getAttribute("donotemail").getValue();
    var staticvmrlinkControl = formContext.getControl("cvt_staticvmrlink");
    var staticvmrlinkControlAttribute = formContext.getAttribute("cvt_staticvmrlink");
    staticvmrlinkControlAttribute.setRequiredLevel(donotemail ? "required" : "none");
    staticvmrlinkControl.setDisabled(donotemail ? false : true);

    var email = formContext.getControl("emailaddress1");
    var emailAttribute = formContext.getAttribute("emailaddress1");
    emailAttribute.setRequiredLevel(donotemail ? "none" : "required");
    email.setDisabled(donotemail ? true : false);
};

//added by Naveen Dubbaka for New Appointment button 
MCS.Patient.NewSchedulingAppointment = function (formContext) {

    var p = {};

    var value = new Array();
    value[0] = new Object();
    value[0].id = formContext.data.entity.getId();
    value[0].name = formContext.getAttribute("fullname").getValue();
    value[0].entityType = "contact";

    p["customers"] = value;

    var entityFormOption = {};

    //entityFormOption.entityName = "serviceappointment";
    //entityFormOption.entityId = null;
    //entityFormOption.openInNewWindow = true;


    entityFormOption["entityName"] = "serviceappointment";
    entityFormOption["entityId"] = null;
    entityFormOption["openInNewWindow"] = true;


    Xrm.Navigation.openForm(entityFormOption, p);

};

//Added by Naveen 02/09/2021
MCS.Patient.NewSchedulingAppointmentOnHome = function (item) {

    var selectedItem = item[0];
    //alert("You have Select Record with Id=" + selectedItem.Id + "\nName="
    //    + selectedItem.Name + "\nEntity Type Code=" + selectedItem.TypeCode.toString() + "\nEntity=" + selectedItem.TypeName);

    var selectedItem = item[0];
    var p = {};

    var value = new Array();
    value[0] = new Object();
    value[0].id = selectedItem.Id;
    value[0].name = selectedItem.Name;
    value[0].entityType = selectedItem.TypeName;

    p["customers"] = value;

    var entityFormOption = {};

    //entityFormOption.entityName = "serviceappointment";
    //entityFormOption.entityId = null;
    //entityFormOption.openInNewWindow = true;


    entityFormOption["entityName"] = "serviceappointment";
    entityFormOption["entityId"] = null;
    entityFormOption["openInNewWindow"] = true;


    Xrm.Navigation.openForm(entityFormOption, p);


};

MCS.Patient.StaticVmrLinkValidate = function (executionContext) {
    //Set Default Visibility
    var formContext = executionContext.getFormContext();
    var staticvmrlink = formContext.getAttribute("cvt_staticvmrlink");

    if (staticvmrlink != null) {
        var staticvmrlinkValue = staticvmrlink.getValue();
        if (staticvmrlinkValue != null && staticvmrlinkValue.indexOf(' ') >= 0) {
            var message = "Enter a valid Static VMR Link without spaces.";
            formContext.getControl("cvt_staticvmrlink").setNotification(message);
        } else {
            formContext.getControl("cvt_staticvmrlink").clearNotification();
        }
    }
}