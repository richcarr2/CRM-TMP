 //Library Name: cvt_KPI.OnChange.js
//If the SDK namespace object is not defined, create it.
//SD: web-use-strict-equality-operators
if (typeof(MCS) === "undefined") {
    MCS = {};
}
// Create Namespace container for functions in this library;
MCS.cvt_KPI_OnChange = {};

//Map different field types to same field.
MCS.cvt_KPI_OnChange.KPI_MapResponse = function (incomingfield, executionContext) {
    //SD - Start
    //web-use-strict-mode
    "use strict";
    //SD - End
    var formContext = executionContext.getFormContext();
    //Validate parameter & Validate field exists & Validate Value
    if ((incomingfield != null) && (formContext.getAttribute(incomingfield) != null) && (formContext.getAttribute(incomingfield).getValue() != null)) {
        //Map value to Response field
        //Write condition for Date YYYY-MM-dd
        formContext.getAttribute("cvt_response").setValue(formContext.getAttribute(incomingfield).getValue().toString());
    }
};