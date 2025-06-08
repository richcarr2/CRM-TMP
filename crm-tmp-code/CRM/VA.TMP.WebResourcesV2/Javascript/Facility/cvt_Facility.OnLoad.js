 //If the SDK namespace object is not defined, create it.
if (typeof(MCS) == "undefined") {
    MCS = {};
}
// Create Namespace container for functions in this library;
MCS.Facility = {};

MCS.Facility.FilterLookups = function (executionContext) {
    var formContext = executionContext.getFormContext();
    alert("Facility Filter Lookups deprecated");
    return;
    var COSViewId = formContext.getAttribute("mcs_businessunitid");
    var FTCViewId = formContext.data.entity.getId();
    var CPViewId = formContext.getAttribute("createdby");

    if (COSViewId == null || FTCViewId == null || CPViewId == null) return;
    COSViewId = COSViewId.getValue()[0].id;
    //FTCViewId = FTCViewId.getValue()[0].id;
    CPViewId = CPViewId.getValue()[0].id;

    var facility = formContext.getAttribute("mcs_name").getValue();
    var fetchBase = null;
    //Create Fetch and set the lookup view
    var columns = ['teamid', 'name', 'createdon'];
    var conditions = ['<condition attribute="cvt_facility" value="' + formContext.data.entity.getId() + '" uitype="mcs_facility" uiname="' + facility + '" operator="eq"/>', ];

    var TeamlayoutXml = '<grid name="resultset" object="9" jump="name" select="1" icon="0" preview="0"><row name="result" id="teamid"><cell name="name" width="300"/><cell name="createdon" width="100"/></row></grid>';

    conditions[1] = '<condition attribute="cvt_type" operator="eq" value="917290000" />';
    fetchBase = MCS.cvt_Common.CreateFetch('team', columns, conditions, ['name', false]);
    var FTCControl = formContext.ui.controls.get("cvt_ftcteam");
    FTCControl.addCustomView(FTCViewId, "team", "FTC at " + facility, fetchBase, TeamlayoutXml, true);

    conditions[1] = '<condition attribute="cvt_type" operator="eq" value="917290002" />';
    fetchBase = MCS.cvt_Common.CreateFetch('team', columns, conditions, ['name', false]);
    var chiefofStaffControl = formContext.ui.controls.get("cvt_chiefofstaffteam");
    chiefofStaffControl.addCustomView(COSViewId, "team", "Chief of Staff at " + facility, fetchBase, TeamlayoutXml, true);

    conditions[1] = '<condition attribute="cvt_type" operator="eq" value="917290003" />';
    fetchBase = MCS.cvt_Common.CreateFetch('team', columns, conditions, ['name', false]);
    var CPControl = formContext.ui.controls.get("cvt_cpteam");
    CPControl.addCustomView(CPViewId, "team", "C&P Officers at " + facility, fetchBase, TeamlayoutXml, true);

}

MCS.Facility.DisableVVS = function (executionContext) {
    var formContext = executionContext.getFormContext();
    var hasRole = MCS.cvt_Common.userHasRoleInList("System Administrator");
    var vvsControl = formContext.getControl("cvt_usevistaintrafacility");
    vvsControl.setDisabled(!hasRole);
}