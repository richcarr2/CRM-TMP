//If the SDK namespace object is not defined, create it.
if (typeof (MCS) == "undefined")
{ MCS = {}; }
// Create Namespace container for functions in this library;
MCS.cvt_MTSA_OnSave = {};

MCS.cvt_MTSA_OnSave.CreateName = function (executionContext) {
    var formContext = executionContext.getFormContext();
    var cvt_servicetype = formContext.getAttribute("cvt_servicetype").getValue();
    var cvt_servicesubtype = formContext.getAttribute("cvt_servicesubtype").getValue();
    var cvt_relatedsiteid = formContext.getAttribute("cvt_relatedsiteid").getValue();
    var cvt_type = formContext.getAttribute("cvt_type").getValue();
    var cvt_groupappointment = formContext.getAttribute("cvt_groupappointment").getValue();;

    var derivedResultField = "";

    if (cvt_servicetype != null) {
        derivedResultField += cvt_servicetype[0].name;
    }

    if (cvt_servicesubtype != null) {
        derivedResultField += " : ";
        derivedResultField += cvt_servicesubtype[0].name;
    }

    derivedResultField += " @ ";

    if (cvt_relatedsiteid != null) {
        derivedResultField += cvt_relatedsiteid[0].name;
    }
    ////Group
    if (cvt_groupappointment == true) {
    } else {
        //CVT to home
        if (cvt_type == true) {
            derivedResultField += " to VA Video Connect";
        }
    }
    if (formContext.getAttribute("cvt_name").getValue() != derivedResultField) {
        formContext.getAttribute("cvt_name").setSubmitMode("always");
        formContext.getAttribute("cvt_name").setValue(derivedResultField);
    }
};