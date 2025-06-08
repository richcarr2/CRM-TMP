//If the SDK namespace object is not defined, create it.
if (typeof(MCS) === "undefined") {
    MCS = {};
}
// Create Namespace container for functions in this library;
MCS.Facility = {};

MCS.Facility.CreateName = function (executionContext) {
    'use strict';
    var formContext = executionContext.getFormContext();
    var facilityLocation = formContext.getAttribute("mcs_facilitylocation").getValue();
    var facilityName = facilityLocation || ""; //Javascript null coalescing operator - returns either facilityLocation (if not null/empty string) or empty string
    if (formContext.getAttribute("mcs_name").getValue() !== facilityName) {
        formContext.getAttribute("mcs_name").setSubmitMode("always");
        formContext.getAttribute("mcs_name").setValue(facilityName);
    }
};