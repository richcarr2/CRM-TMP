
if (typeof MCS == "undefined") {
    MCS = {};
}
// Create Namespace container for functions in this library;

if (typeof MCS.PhoneBook == "undefined") {
    MCS.PhoneBook = {};
}

MCS.PhoneBook.SetName = function (executionContext) {
    var formContext = executionContext.getFormContext();
    var name = "";
    var providerObj = formContext.getAttribute("cvt_provider");
    var provider = "";
    if (providerObj != null && providerObj.getValue() != null) {
        provider = providerObj.getValue()[0].name;
        name += provider;
    }
    var startTime = formContext.getAttribute("cvt_starttime").getValue();
    if (startTime != null && startTime != "") name += ": " + startTime.toLocaleDateString();

    formContext.getAttribute("cvt_name").setValue(name);
};