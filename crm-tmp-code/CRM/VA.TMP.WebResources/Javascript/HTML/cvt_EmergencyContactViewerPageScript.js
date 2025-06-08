//TSA iFrame
onLoad = function () {
    var siteEmergency = parent.MCS.SiteEmergencyPhone;
    var TCTPhone = parent.MCS.TCTPhone
    var clinicPhone = parent.MCS.SiteMainPhone;
    var local911 = parent.MCS.SiteLocal911Phone;
    if (siteEmergency === null || siteEmergency.toString() === "undefined") {
        siteEmergency = "";
    }
    if (TCTPhone === null || TCTPhone.toString() === "undefined") {
        TCTPhone = "";
    }
    if (clinicPhone === null || clinicPhone.toString() === "undefined") {
        clinicPhone = "";
    }
    if (local911 === null || local911.toString() === "undefined") {
        local911 = "";
    }

    var body = "<div title=\"Emergency Contact Table\" role=\"grid\" aria-readonly=\"true\"><h4 title=\"Emergency Contact Table\">Emergency Contact Table </h4>";
    body += "<table role=\"presentation\" style=\"font-size: 12px\" class=\"table\">";
    body += "<tr role=\"row\"><th role=\"columnheader\">Name</th><th role=\"columnheader\">Phone #</th></tr>";
    body += "<tr role=\"row\"><td role=\"gridcell\">Patient Site Emergency Phone #</td><td  role=\"gridcell\">" + siteEmergency + "</td></tr>";
    body += "<tr role=\"row\"><td role=\"gridcell\">Patient Site TCT Team Members</td><td role=\"gridcell\">" + TCTPhone + "</td></tr>";
    body += "<tr role=\"row\"><td role=\"gridcell\">Patient Site Main Site Phone #</td><td role=\"gridcell\">" + clinicPhone + "</td></tr>";
    body += "<tr role=\"row\"><td role=\"gridcell\">Patient Site Local 911</td><td role=\"gridcell\">" + local911 + "</td></tr>";
    body += "</tr></table></div>";
    document.getElementById("HTMLBody").innerHTML = body;
};