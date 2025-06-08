function ReserveResourceOpen() {
    var entityFormOptions = {};
    entityFormOptions["entityName"] = "appointment"; //whatever the entity name is for reserve resource
    // Set default values for the form if needed
    var formParameters = {};
    // Set lookup field if we need to set a lookup on the form sample
    //formParameters["preferredsystemuserid"] = "3493e403-fc0c-eb11-a813-002248e258e0"; // ID of the user.
    //formParameters["preferredsystemuseridname"] = "Admin user"; // Name of the user.
    //formParameters["preferredsystemuseridtype"] = "systemuser"; // Entity name.
    Xrm.Navigation.openForm(entityFormOptions, formParameters).then(function (success) { console.log(success); }, function (error) { console.log(error); });
}

function RecurringReserveResourceOpen() {
    var entityFormOptions = {};
    entityFormOptions["entityName"] = "recurringappointmentmaster"; //whatever the entity name is for reserve resource
    // Set default values for the form if needed
    var formParameters = {};
    // Set lookup field if we need to set a lookup on the form sample
    //formParameters["preferredsystemuserid"] = "3493e403-fc0c-eb11-a813-002248e258e0"; // ID of the user.
    //formParameters["preferredsystemuseridname"] = "Admin user"; // Name of the user.
    //formParameters["preferredsystemuseridtype"] = "systemuser"; // Entity name.
    Xrm.Navigation.openForm(entityFormOptions, formParameters).then(function (success) { console.log(success); }, function (error) { console.log(error); });
}

