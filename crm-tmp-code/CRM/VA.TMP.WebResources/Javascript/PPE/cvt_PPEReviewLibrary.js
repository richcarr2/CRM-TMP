//If the SDK namespace object is not defined, create it.
if (typeof (MCS) === "undefined") { MCS = {}; }
// Create Namespace container for functions in this library;
MCS.PPEReview = {};

//Check Status to make entire form readonly.
MCS.PPEReview.OnLoad = function (executionContext) {
    var formContext = executionContext.getFormContext();
    if (formContext.ui.getFormType() !== 1) {
        formContext.getControl("cvt_telehealthprivileging").setDisabled(true);
        formContext.getControl("cvt_initiateddate").setDisabled(true);
        formContext.getControl("cvt_duedate").setDisabled(true);
    }
};

//Check Status to make entire form readonly.
MCS.PPEReview.InitiatedDate = function (executionContext) {
    var formContext = executionContext.getFormContext();
    var initiatedDate = new Date(formContext.getAttribute('cvt_initiateddate').getValue());
    var daysToAdd = 14;
    var proposedDueDate = new Date();

    //Determine the Day of the week
    var weekday = new Array(7);
    weekday[0] = "Sunday";
    weekday[1] = "Monday";
    weekday[2] = "Tuesday";
    weekday[3] = "Wednesday";
    weekday[4] = "Thursday";
    weekday[5] = "Friday";
    weekday[6] = "Saturday";

    var initiatedDateDay = weekday[initiatedDate.getDay()];

    //Determine the date 10 business days ahead (14 days)
    switch (initiatedDateDay) {
        case "Sunday":
            //Need to add + 1 to move it to a business day (Monday)
            daysToAdd += 1;
            break;
        case "Saturday":
            //Need to add + 2 to move it to a business day (Monday)
            daysToAdd += 2;
            break;
        default:
            //It is a business day, no need to modify
            break;
    }

    //Calcualte the Due Date
    proposedDueDate.setDate(initiatedDate.getDate() + daysToAdd);

    //alert("Proposed Due Date is set in the Due Date field. You may change it to a more suitable date.");
    formContext.getAttribute("cvt_duedate").setValue(proposedDueDate);
};

MCS.PPEReview.OnSave = function (executionContext) {
    var formContext = executionContext.getFormContext();
    if (formContext.getAttribute("cvt_telehealthprivileging") === null) {
        alert("Telehealth Privileging is required.");
        executionContext.getEventArgs().preventDefault();
        return;
    }
    //
    ////Check for Proxy Privs
    //var thisPriv = [];
    //var filter = "cvt_tssprivilegingId/Id eq (Guid'" + Xrm.Page.getAttribute("cvt_telehealthprivileging").getValue()[0].Id + "')";
    //calls = CrmRestKit.ByQuery("cvt_tssprivileging", ['cvt_TypeofPrivileging'], filter, false);
    //calls.fail(
    //        function (error) {
    //        });
    //calls.done(function (data) {
    //    if (data && data.d.results && data.d.results.length > 0) {
    //        thisPriv = data.d.results;
    //    }
    //});
    //if (thisPriv.length == 1) {
    //    alert("Found priv record.");
    //    for (record in thisPriv) {
    //        if (thisPriv[record].cvt_TypeofPrivileging != 917290000) {
    //            alert("Telehealth Privileging must be Home/Primary.");
    //            context.getEventArgs().preventDefault();
    //            return;
    //        }
    //        else
    //            alert("Priv is Home/Primary.");
    //    }
    //}

    ////var calls = CrmRestKit.Retrieve("cvt_tssprivileging", Xrm.Page.getAttribute("cvt_telehealthprivileging").getValue()[0].Id, ['cvt_TypeofPrivileging'], false);
    ////calls.fail(
    ////    function (error) {
    ////    });

    ////calls.done(function (data) {
    ////    alert("Finished Retrieve");
    ////    if (data && data.d && data.d.cvt_TypeofPrivileging) {
    ////        //Check and Set Facility
    ////        if (data.d.cvt_TypeofPrivileging != 917290000) {
    ////            alert("Telehealth Privileging must be Home/Primary.");
    ////            context.getEventArgs().preventDefault();
    ////            return;

    ////        }
    ////        else
    ////            alert("Priv is Home/Primary.");
    ////    }
    ////});

    ////Check for Proxy Privs
    //var proxyPrivs = [];
    //var filter = "cvt_ReferencedPrivilegeid/Id eq (Guid'" + Xrm.Page.getAttribute("cvt_telehealthprivileging").getValue()[0].Id + "')";
    //calls = CrmRestKit.ByQuery("cvt_tssprivileging", ['cvt_name'], filter, false);
    //calls.fail(
    //        function (error) {
    //        });
    //calls.done(function (data) {
    //    if (data && data.d.results && data.d.results.length > 0) {
    //        proxyPrivs = data.d.results;
    //    }
    //});
    //if (proxyPrivs.length < 1) {
    //    alert("Telehealth Privileging must have at least one Proxy/Secondary Privilege.");
    //    context.getEventArgs().preventDefault();
    //    return;
    //}
    //Check for Dates
    if (formContext.getAttribute("cvt_initiateddate") === null) {
        alert("Initiated Date is required.");
        executionContext.getEventArgs().preventDefault();
        return;
    }

    if (formContext.getAttribute("cvt_duedate") === null) {
        alert("Due Date is required.");
        executionContext.getEventArgs().preventDefault();
        return;
    }
    var date = new Date(formContext.getAttribute("cvt_initiateddate").getValue());
    var name = formContext.getAttribute("cvt_telehealthprivileging").getValue()[0].name + " Review (";
    name += date.getFullYear() + "/";
    name += (date.getMonth() + 1) < 10 ? "0" + (date.getMonth() + 1) : (date.getMonth() + 1);
    name += "/";
    name += (date.getDate()) < 10 ? "0" + (date.getDate()) : (date.getDate());
    name += ")";

    if (formContext.ui.getFormType() === 1 || name !== formContext.getAttribute("cvt_name").getValue()) {
        formContext.getAttribute("cvt_name").setSubmitMode("always");
        formContext.getAttribute("cvt_name").setValue(name);
    }
};