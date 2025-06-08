using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VA.TMP.Integration.Api.TelehealthSpecialtyLocation.Helpers;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Messages.TelehealthSpecialtyLocation;

namespace VA.TMP.Integration.Api.TelehealthSpecialtyLocation.Processor
{
    public class GetClinicLocationsProcessor
    {
        private readonly IConfiguration _config;
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;
        private IOptions<ApplicationSettings> _settings;

        private enum ParticipatingSiteLocations
        {
            Provider = 917290000,
            Patient = 917290001
        }

        private enum ResourceType
        {
            VistaClinic = 251920000,
            Provider = 99999999
        }

        private enum SchedulingResourceType
        {
            PairedResourceGroup,
            SingleResource,
            SingleProvider
        }

        public GetClinicLocationsProcessor(ILoggerFactory logger, IOptions<ApplicationSettings> settings, IConfiguration config)
        {
            _config = config;
            _logger = logger.CreateLogger<GetClinicLocationsProcessor>();
            _loggerFactory = logger;
            _settings = settings;
        }

        public TelehealthSpecialtyLocationsGetClinicLocationsResponse Execute(TelehealthSpecialtyLocationsGetClinicLocationsRequest message)
        {
            return null;
        //    var helper = new RestHelper(_logger, _settings, _config);

        //    var encoder = UrlEncoder.Default;

        //    var singleResourcesFetchXml = encoder.Encode($"<fetch distinct=\"true\"><entity name=\"cvt_facilityapproval\"><link-entity name=\"mcs_facility\" to=\"cvt_providerfacility\" from=\"mcs_facilityid\" alias=\"pf\" link-type=\"inner\"><attribute name=\"mcs_facilityid\" /><link-entity name=\"cvt_resourcepackage\" to=\"mcs_facilityid\" from=\"cvt_providerfacility\" alias=\"rp\" link-type=\"inner\"><attribute name=\"cvt_intraorinterfacility\" /><attribute name=\"cvt_resourcepackageid\" /><filter><condition attribute=\"cvt_intraorinterfacility\" operator=\"eq\" value=\"917290001\" /></filter><link-entity name=\"mcs_servicetype\" to=\"cvt_specialty\" from=\"mcs_servicetypeid\" alias=\"sp\" link-type=\"inner\"><attribute name=\"mcs_servicetypeid\" /><filter><condition attribute=\"mcs_name\" operator=\"eq\" value=\"{message.SpecialtyName}\" /></filter></link-entity><link-entity name=\"cvt_participatingsite\" to=\"cvt_resourcepackageid\" from=\"cvt_resourcepackage\" alias=\"ps\" link-type=\"inner\"><attribute name=\"cvt_locationtype\" /><attribute name=\"statecode\" /><attribute name=\"statuscode\" /><attribute name=\"cvt_participatingsiteid\" /><filter><condition attribute=\"statecode\" operator=\"eq\" value=\"0\" /><condition attribute=\"statuscode\" operator=\"eq\" value=\"1\" /><condition attribute=\"cvt_locationtype\" operator=\"eq\" value=\"917290000\" /></filter><link-entity name=\"mcs_site\" from=\"mcs_siteid\" to=\"cvt_site\" link-type=\"inner\" alias=\"pss\"><attribute name=\"mcs_name\" /><attribute name=\"mcs_siteid\" /><order attribute=\"mcs_siteid\" /><filter><condition attribute=\"mcs_stationnumber\" operator=\"eq\" value=\"{message.SiteStationNumber}\" /></filter></link-entity><link-entity name=\"cvt_schedulingresource\" to=\"cvt_participatingsiteid\" from=\"cvt_participatingsite\" alias=\"sr\" link-type=\"inner\"><attribute name=\"cvt_schedulingresourceid\" /><link-entity name=\"mcs_resource\" from=\"mcs_resourceid\" to=\"cvt_tmpresource\" link-type=\"inner\" alias=\"srr\"><attribute name=\"mcs_name\" /><attribute name=\"mcs_type\" /><attribute name=\"mcs_resourceid\" /><filter type=\"or\"><condition attribute=\"mcs_type\" operator=\"eq\" value=\"99999999\" /><condition attribute=\"mcs_type\" operator=\"eq\" value=\"251920000\" /></filter><link-entity name=\"mcs_site\" from=\"mcs_siteid\" to=\"mcs_relatedsiteid\" link-type=\"inner\" alias=\"srrs\"><attribute name=\"mcs_name\" /><attribute name=\"mcs_siteid\" /><order attribute=\"mcs_siteid\" /></link-entity><order attribute=\"mcs_resourceid\" /></link-entity></link-entity><link-entity name=\"cvt_schedulingresource\" from=\"cvt_participatingsite\" to=\"cvt_participatingsiteid\" link-type=\"inner\" alias=\"srs\"><link-entity name=\"systemuser\" from=\"systemuserid\" to=\"cvt_user\" link-type=\"inner\" alias=\"srsu\"><attribute name=\"fullname\" /><filter><condition attribute=\"isdisabled\" operator=\"eq\" value=\"0\" /></filter></link-entity></link-entity></link-entity><filter><condition attribute=\"cvt_intraorinterfacility\" operator=\"eq\" value=\"917290001\" /></filter></link-entity></link-entity><filter type=\"and\"><condition entityname=\"paf\" attribute=\"mcs_stationnumber\" operator=\"eq\" value=\"{message.FacilityStationNumber}\" /><filter type=\"or\"><condition attribute=\"cvt_approvalstatusprovidercos\" operator=\"eq\" value=\"917290001\" /><condition attribute=\"cvt_approvalstatusproviderftc\" operator=\"eq\" value=\"917290001\" /><condition attribute=\"cvt_approvalstatusprovidersc\" operator=\"eq\" value=\"917290001\" /></filter></filter><link-entity name=\"mcs_facility\" from=\"mcs_facilityid\" to=\"cvt_patientfacility\" alias=\"paf\" /></entity></fetch>");
        //    var groupResourcesFetchXml = encoder.Encode($"<fetch distinct=\"true\"><entity name=\"cvt_facilityapproval\"><attribute name=\"cvt_name\" /><attribute name=\"cvt_facilityapprovalid\" /><link-entity name=\"mcs_facility\" to=\"cvt_providerfacility\" from=\"mcs_facilityid\" alias=\"pf\" link-type=\"inner\"><attribute name=\"mcs_facilityid\" /><link-entity name=\"cvt_resourcepackage\" to=\"mcs_facilityid\" from=\"cvt_providerfacility\" alias=\"rp\" link-type=\"inner\"><attribute name=\"cvt_intraorinterfacility\" /><attribute name=\"cvt_resourcepackageid\" /><link-entity name=\"mcs_servicetype\" to=\"cvt_specialty\" from=\"mcs_servicetypeid\" alias=\"sp\" link-type=\"inner\"><attribute name=\"mcs_servicetypeid\" /><filter><condition attribute=\"mcs_name\" operator=\"eq\" value=\"{message.SpecialtyName}\" /></filter><order attribute=\"mcs_servicetypeid\" /></link-entity><link-entity name=\"cvt_participatingsite\" to=\"cvt_resourcepackageid\" from=\"cvt_resourcepackage\" alias=\"ps\" link-type=\"inner\"><attribute name=\"cvt_locationtype\" /><attribute name=\"statecode\" /><attribute name=\"statuscode\" /><filter><condition attribute=\"statecode\" operator=\"eq\" value=\"0\" /><condition attribute=\"statuscode\" operator=\"eq\" value=\"1\" /></filter><link-entity name=\"mcs_site\" to=\"cvt_site\" from=\"mcs_siteid\" alias=\"pss\" link-type=\"inner\"><attribute name=\"mcs_name\" /><attribute name=\"mcs_siteid\" /><order attribute=\"mcs_siteid\" /><filter><condition attribute=\"mcs_stationnumber\" operator=\"eq\" value=\"{message.SiteStationNumber}\" /></filter></link-entity><link-entity name=\"cvt_schedulingresource\" from=\"cvt_participatingsite\" to=\"cvt_participatingsiteid\" link-type=\"outer\" alias=\"sr\"><attribute name=\"cvt_schedulingresourceid\" /><link-entity name=\"mcs_resourcegroup\" from=\"mcs_resourcegroupid\" to=\"cvt_tmpresourcegroup\" link-type=\"inner\" alias=\"rg\"><attribute name=\"mcs_resourcegroupid\" /><filter><condition attribute=\"mcs_type\" operator=\"eq\" value=\"917290000\" /><filter type=\"and\"><condition attribute=\"mcs_type\" operator=\"ne\" value=\"99999999\" /><condition attribute=\"mcs_type\" operator=\"ne\" value=\"251920001\" /><condition attribute=\"mcs_type\" operator=\"ne\" value=\"251920002\" /><condition attribute=\"mcs_type\" operator=\"ne\" value=\"100000000\" /><condition attribute=\"mcs_type\" operator=\"ne\" value=\"251920000\" /></filter></filter><link-entity name=\"mcs_groupresource\" from=\"mcs_relatedresourcegroupid\" to=\"mcs_resourcegroupid\" link-type=\"outer\" alias=\"gr\"><attribute name=\"mcs_groupresourceid\" /><filter type=\"and\"><condition attribute=\"mcs_type\" operator=\"ne\" value=\"251920001\" /><condition attribute=\"mcs_type\" operator=\"ne\" value=\"251920002\" /><condition attribute=\"mcs_type\" operator=\"ne\" value=\"917290000\" /><condition attribute=\"mcs_type\" operator=\"ne\" value=\"100000000\" /><filter type=\"or\"><condition attribute=\"mcs_type\" operator=\"eq\" value=\"99999999\" /><condition attribute=\"mcs_type\" operator=\"eq\" value=\"251920000\" /></filter></filter><link-entity name=\"mcs_resource\" from=\"mcs_resourceid\" to=\"mcs_relatedresourceid\" link-type=\"outer\" alias=\"grr\"><attribute name=\"mcs_name\" /><attribute name=\"mcs_type\" /><attribute name=\"mcs_resourceid\" /><filter type=\"or\"><condition attribute=\"mcs_type\" operator=\"eq\" value=\"99999999\" /><condition attribute=\"mcs_type\" operator=\"eq\" value=\"251920000\" /></filter><link-entity name=\"mcs_site\" from=\"mcs_siteid\" to=\"mcs_relatedsiteid\" link-type=\"outer\" alias=\"grrs\"><attribute name=\"mcs_name\" /></link-entity><link-entity name=\"systemuser\" from=\"systemuserid\" to=\"cvt_relateduser\" link-type=\"outer\" alias=\"grru\"><attribute name=\"fullname\" /><filter><condition attribute=\"isdisabled\" operator=\"eq\" value=\"0\" /></filter></link-entity></link-entity><link-entity name=\"systemuser\" from=\"systemuserid\" to=\"mcs_relateduserid\" link-type=\"outer\" alias=\"gru\"><attribute name=\"fullname\" /><filter><condition attribute=\"isdisabled\" operator=\"eq\" value=\"0\" /></filter></link-entity></link-entity></link-entity><link-entity name=\"systemuser\" from=\"systemuserid\" to=\"cvt_user\" link-type=\"outer\" alias=\"sru\"><attribute name=\"fullname\" /><filter><condition attribute=\"isdisabled\" operator=\"eq\" value=\"0\" /></filter></link-entity></link-entity><filter><condition attribute=\"cvt_locationtype\" operator=\"eq\" value=\"917290000\" /></filter></link-entity><filter><condition attribute=\"cvt_intraorinterfacility\" operator=\"eq\" value=\"917290001\" /></filter></link-entity></link-entity><filter type=\"and\"><condition entityname=\"paf\" attribute=\"mcs_stationnumber\" operator=\"eq\" value=\"{message.FacilityStationNumber}\" /><filter type=\"or\"><condition attribute=\"cvt_approvalstatusprovidercos\" operator=\"eq\" value=\"917290001\" /><condition attribute=\"cvt_approvalstatusproviderftc\" operator=\"eq\" value=\"917290001\" /><condition attribute=\"cvt_approvalstatusprovidersc\" operator=\"eq\" value=\"917290001\" /></filter></filter><order attribute=\"cvt_facilityapprovalid\" /><link-entity name=\"mcs_facility\" from=\"mcs_facilityid\" to=\"cvt_patientfacility\" link-type=\"inner\" alias=\"paf\" /></entity></fetch>");

        //    var uri = $"{_settings.Value.BaseUrl}/api/data/{_settings.Value.ApiVersion}/cvt_facilityapprovals?fetchXml={singleResourcesFetchXml}";
        //    _logger.LogDebug($"Calling Web API Url: {uri}");

        //    try
        //    {
        //        var responseMessage = new FacilityApprovalsResponseMessage { FacilityApprovals = new List<FacilityApproval>() };
        //        var singleResourcesResponseMessage = helper.Get<FacilityApprovalsResponseMessage>(uri);

        //        responseMessage.FacilityApprovals.AddRange(singleResourcesResponseMessage.FacilityApprovals);

        //        uri = $"{_settings.Value.BaseUrl}/api/data/{_settings.Value.ApiVersion}/cvt_facilityapprovals?fetchXml={groupResourcesFetchXml}";
        //        var groupResourcesResponseMessage = helper.Get<FacilityApprovalsResponseMessage>(uri);

        //        responseMessage.FacilityApprovals.AddRange(groupResourcesResponseMessage.FacilityApprovals);

        //        var clinicLocationsFound = ProcessResponseMessage(responseMessage, message);

        //        return new TelehealthSpecialtyLocationsGetClinicLocationsResponse { ClinicLocations = clinicLocationsFound };
        //    }
        //    catch (System.Exception e)
        //    {
        //        return new TelehealthSpecialtyLocationsGetClinicLocationsResponse { ErrorMessage = e.Message };
        //    }
        //}

        //private List<FacilityClinicLocation> ProcessResponseMessage(FacilityApprovalsResponseMessage responseMessage, TelehealthSpecialtyLocationsGetClinicLocationsRequest message)
        //{
        //    var approvalClinics = new List<FacilityClinicLocation>();

        //    _logger.LogDebug($"Unfiltered Facility Approvals Count: {responseMessage.FacilityApprovals.Count}");

        //    #region OldCode
        //    //responseMessage.FacilityApprovals = responseMessage.FacilityApprovals.Where(fa => fa.ProviderFacility.SchedulingPackages.Any(sp => sp.Specialty.Name.Equals(message.SpecialtyName) && sp.IntraOrInterFacility.Equals(917290001))).ToList();
        //    //_logger.LogDebug($"Filtered by Specialty Count: {responseMessage.FacilityApprovals.Count}");

        //    //responseMessage.FacilityApprovals = responseMessage.FacilityApprovals.FindAll(fa =>
        //    //    fa.ProviderFacility.SchedulingPackages.Any(sp => sp.ParticipatingSites.Any(ps => ps.Site.StationNumber.Equals(message.SiteStationNumber) &&
        //    //    ps.LocationType.Equals((int)ParticipatingSiteLocations.Provider) && ps.StateCode.Equals(0) && ps.StateCode.Equals(1))));

        //    //_logger.LogDebug($"Filtered by Site Station Number Count: {responseMessage.FacilityApprovals.Count}");

        //    //responseMessage.FacilityApprovals.ForEach(facilityApproval =>
        //    //{
        //    //    facilityApproval.ProviderFacility.SchedulingPackages.AsParallel().ToList().ForEach(schedulingPackage =>
        //    //    {
        //    //        schedulingPackage.ParticipatingSites.AsParallel().ToList().ForEach(participatingSite =>
        //    //        {
        //    //            if (participatingSite.SchedulingResources.Count > 0)
        //    //            {
        //    //                _logger.LogDebug($"Participating Site Location Type:{participatingSite.LocationType}");
        //    //                _logger.LogDebug($"Participating Site Site Station Number:{participatingSite.Site.StationNumber}");

        //    //                if (participatingSite.LocationType.Equals((int)ParticipatingSiteLocations.Provider) &&
        //    //                    participatingSite.Site.StationNumber.Equals(message.SiteStationNumber) &&
        //    //                    participatingSite.SchedulingResources.Count <= 2)
        //    //                {
        //    //                    //Check for Paired Resource Group containing 1 site and 1 provider
        //    //                    if (participatingSite.SchedulingResources.Count.Equals(1))
        //    //                    {
        //    //                        if (participatingSite.SchedulingResources[0].SchedulingResourceType.Equals((int)SchedulingResourceType.PairedResourceGroup) &&
        //    //                            participatingSite.SchedulingResources[0].ResourceGroup.GroupResources.Count.Equals(2))
        //    //                        {
        //    //                            var resourceGrp = participatingSite.SchedulingResources[0].ResourceGroup;
        //    //                            if (resourceGrp.GroupResources[0].RelatedUser != null && resourceGrp.GroupResources[1].RelatedSite != null)
        //    //                            {
        //    //                                approvalClinics.Add(new FacilityClinicLocation
        //    //                                {
        //    //                                    ClinicName = resourceGrp.GroupResources[1].RelatedSite.Name.Trim(),
        //    //                                    SiteName = participatingSite.Name.Trim()
        //    //                                });
        //    //                            }
        //    //                            if (resourceGrp.GroupResources[0].RelatedSite != null && resourceGrp.GroupResources[1].RelatedUser != null)
        //    //                            {
        //    //                                approvalClinics.Add(new FacilityClinicLocation
        //    //                                {
        //    //                                    ClinicName = resourceGrp.GroupResources[0].RelatedSite.Name.Trim(),
        //    //                                    SiteName = participatingSite.Name.Trim()
        //    //                                });
        //    //                            }
        //    //                        }
        //    //                    }
        //    //                    else
        //    //                    {
        //    //                        //Check for Scheduling Resources containing 1 site and 1 provider
        //    //                        if (participatingSite.SchedulingResources[0].Resource != null && participatingSite.SchedulingResources[0].SchedulingResourceType.Equals((int)SchedulingResourceType.SingleResource) &&
        //    //                            participatingSite.SchedulingResources[1].SchedulingResourceType.Equals((int)SchedulingResourceType.SingleProvider))
        //    //                        {
        //    //                            if (participatingSite.SchedulingResources[0].Resource.Type.Equals((int)ResourceType.VistaClinic))
        //    //                            {
        //    //                                approvalClinics.Add(new FacilityClinicLocation
        //    //                                {
        //    //                                    ClinicName = participatingSite.SchedulingResources[0].Resource.Name.Trim(),
        //    //                                    SiteName = participatingSite.Name.Trim()
        //    //                                });
        //    //                            }
        //    //                        }

        //    //                        if (participatingSite.SchedulingResources[0].SchedulingResourceType.Equals((int)SchedulingResourceType.SingleProvider) &&
        //    //                            participatingSite.SchedulingResources[1].Resource != null && participatingSite.SchedulingResources[1].SchedulingResourceType.Equals((int)SchedulingResourceType.SingleResource))
        //    //                        {
        //    //                            if (participatingSite.SchedulingResources[1].Resource.Type.Equals((int)ResourceType.VistaClinic))
        //    //                            {
        //    //                                approvalClinics.Add(new FacilityClinicLocation
        //    //                                {
        //    //                                    ClinicName = participatingSite.SchedulingResources[1].Resource.Name.Trim(),
        //    //                                    SiteName = participatingSite.Name.Trim()
        //    //                                });
        //    //                            }
        //    //                        }

        //    //                        //Check for Paired Resource Group containing 1 site and 1 provider
        //    //                        if (participatingSite.SchedulingResources[0].Resource != null && participatingSite.SchedulingResources[0].SchedulingResourceType.Equals((int)SchedulingResourceType.SingleResource) &&
        //    //                            participatingSite.SchedulingResources[1].ResourceGroup != null && participatingSite.SchedulingResources[1].ResourceGroup.GroupResources.Count.Equals(1))
        //    //                        {
        //    //                            var resourceGrp = participatingSite.SchedulingResources[1].ResourceGroup;
        //    //                            if (resourceGrp.GroupResources[0].RelatedUser != null || resourceGrp.GroupResources[0].RelatedSite != null)
        //    //                            {
        //    //                                approvalClinics.Add(new FacilityClinicLocation
        //    //                                {
        //    //                                    ClinicName = participatingSite.SchedulingResources[0].Resource.Name.Trim(),
        //    //                                    SiteName = participatingSite.Name.Trim()
        //    //                                });
        //    //                            }
        //    //                        }

        //    //                        //Check for Paired Resource Group containing 1 site and 1 provider
        //    //                        if (participatingSite.SchedulingResources[0].ResourceGroup != null && participatingSite.SchedulingResources[0].ResourceGroup.GroupResources.Count.Equals(1) &&
        //    //                            participatingSite.SchedulingResources[1].Resource != null && participatingSite.SchedulingResources[1].SchedulingResourceType.Equals((int)SchedulingResourceType.SingleResource))
        //    //                        {
        //    //                            var resourceGrp = participatingSite.SchedulingResources[0].ResourceGroup;
        //    //                            if (resourceGrp.GroupResources[0].RelatedUser != null || resourceGrp.GroupResources[0].RelatedSite != null)
        //    //                            {
        //    //                                approvalClinics.Add(new FacilityClinicLocation
        //    //                                {
        //    //                                    ClinicName = participatingSite.SchedulingResources[1].Resource.Name.Trim(),
        //    //                                    SiteName = participatingSite.Name.Trim()
        //    //                                });
        //    //                            }
        //    //                        }
        //    //                    }
        //    //                }
        //    //            }
        //    //        });
        //    //    });

        //    //    _logger.LogDebug($"Approval ClinicLocations found: {approvalClinics.Count}");
        //    //}); 
        //    #endregion

        //    responseMessage.FacilityApprovals.AsParallel().ToList().ForEach(fa =>
        //    {
        //        if (!string.IsNullOrEmpty(fa.SingleResourceClinicName) && !string.IsNullOrEmpty(fa.SingleResourceUserName))
        //        {
        //            approvalClinics.Add(new FacilityClinicLocation { ClinicName = fa.SingleResourceClinicName, SiteName = fa.SiteName });
        //        }
        //        else if (!string.IsNullOrEmpty(fa.GroupResourceClinicName) && !string.IsNullOrEmpty(fa.GroupResourceUserName))
        //        {
        //            approvalClinics.Add(new FacilityClinicLocation { ClinicName = fa.GroupResourceClinicName, SiteName = fa.SiteName });
        //        }
        //    });

        //    return approvalClinics;
        }
    }
}
