//If the SDK namespace object is not defined, create it.
if (typeof (MCS) == "undefined")
{ MCS = {}; }
// Create Namespace container for functions in this library;
MCS.cvt_MTSA_OnLoad = {};

//Page space variables
MCS.cvt_MTSA_OnLoad.EntityId = null;
MCS.cvt_MTSA_OnLoad.EntityName = null;
MCS.cvt_MTSA_OnLoad.relatedProviderSiteName = null;
MCS.cvt_MTSA_OnLoad.relatedProviderSiteId = null;
MCS.cvt_MTSA_OnLoad.GroupAppt = false;

//OnLoad
MCS.cvt_MTSA_OnLoad.SetDefaults = function (executionContext) {
    var formContext = executionContext.getFormContext();
    formContext.getControl("cvt_type").setDisabled(false);

    if (formContext.ui.getFormType() == MCS.cvt_Common.FORM_TYPE_CREATE) {
        alert('The Master TSA functionality is obselete and the new record creation is not available.');
        formContext.ui.close();
    }
    else
    {
        formContext.ui.setFormNotification('Note: The Master TSA functionality is obselete. Please use Telehealth Administration >> Scheduling Package instead.', 'WARNING', '123');
        //If not Create
        //Read Only Fields
        formContext.getControl("cvt_relatedsiteid").setDisabled(true);
        formContext.getControl("cvt_servicetype").setDisabled(true);
        formContext.getControl("cvt_servicesubtype").setDisabled(true);
        formContext.getControl("cvt_groupappointment").setDisabled(true);
        formContext.getControl("cvt_availabletelehealthmodalities").setDisabled(true);
        formContext.getControl("cvt_type").setDisabled(true);
        //Conditional because of potentially missing data.
        if (formContext.getAttribute("cvt_providerlocationtype").getValue() != null) {
            formContext.getControl("cvt_providerlocationtype").setDisabled(true);
        }
        else {
            formContext.getControl("cvt_providerlocationtype").setDisabled(false);
        }

        MCS.cvt_MTSA_OnLoad.SOS(executionContext);

        //Load Operations Guide
        var filter = "mcs_name eq 'Active Settings'";
        calls = CrmRestKit.ByQuery("mcs_setting", ['cvt_telehealthoperationsmanual'], filter, false);
        calls.fail(function (err) {
        }).done(function (data) {
            if (data && data.d && data.d.results != null && data.d.results.length != 0) {
                var url = data.d.results[0].cvt_telehealthoperationsmanual != null ? data.d.results[0].cvt_telehealthoperationsmanual : null;
                if (url != null)
                    formContext.getAttribute("cvt_telehealthoperationsmanual").setValue(url);
            }
        });
    }
    //Field manipulation - These are all called by specific field OnChange
    if (formContext.getAttribute("cvt_servicesubtype").getValue() != null)
        MCS.cvt_Common.EnableDependentLookup("cvt_servicetype", "cvt_servicesubtype");

    MCS.cvt_MTSA_OnLoad.EntityId = formContext.data.entity.getId();
    MCS.cvt_MTSA_OnLoad.EntityName = formContext.data.entity.getEntityName();
    if (formContext.getAttribute("cvt_relatedsiteid").getValue() != null) {
        MCS.cvt_MTSA_OnLoad.relatedProviderSiteName = formContext.getAttribute("cvt_relatedsiteid").getValue()[0].name;
        MCS.cvt_MTSA_OnLoad.relatedProviderSiteId = formContext.getAttribute("cvt_relatedsiteid").getValue()[0].id;
    }
    MCS.cvt_MTSA_OnLoad.GroupAppt = formContext.getAttribute("cvt_groupappointment").getValue();
};

MCS.cvt_MTSA_OnLoad.SOS = function (executionContext) {
    var formContext = executionContext.getFormContext();
    //Make the SOS field visible and get the url
    if (formContext.getAttribute("cvt_servicetype").getValue() != null) {
        Xrm.WebApi.retrieveRecord("mcs_servicetype", formContext.getAttribute("cvt_servicetype").getValue()[0].id, "?$select=cvt_specialtyoperationssupplement").then(
            function success(result) {
                saRecord = result;
                if (result.cvt_specialtyoperationssupplement != null && result.cvt_specialtyoperationssupplement != undefined) {
                        formContext.getControl("cvt_specialtyoperationssupplement").setVisible(true);
                        formContext.getAttribute("cvt_specialtyoperationssupplement").setValue(result.cvt_specialtyoperationssupplement);
                }
            }
        );
    }
};