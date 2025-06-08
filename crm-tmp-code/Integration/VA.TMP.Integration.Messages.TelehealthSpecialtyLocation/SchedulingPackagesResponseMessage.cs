using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using VA.TMP.Integration.Core;

namespace VA.TMP.Integration.Messages.TelehealthSpecialtyLocation
{
    public class SchedulingPackagesResponseMessage : TmpBaseResponseMessage
    {
        [JsonPropertyName("@odata.context")]
        public string Context { get; set; }

        [JsonPropertyName("value")]
        public List<ResourcePackage> SchedulingPackages { get; set; }
    }

    public class ResourcePackage
    {
        [JsonPropertyName("@odata.etag")]
        public string Etag { get; set; }

        public List<ParticipatingSite> ParticipatingSites { get; set; } = new List<ParticipatingSite>();

        [JsonPropertyName("paf.mcs_name")]
        public string PatientFacilityName { get; set; }

        [JsonPropertyName("paf.mcs_timezone")]
        public int PatientFacilityTimeZoneCode { get; set; }

        [JsonPropertyName("pafv.name")]
        public string PatientFacilityVISN { get; set; }

        [JsonPropertyName("pas.cvt_participatingsiteid")]
        public Guid PatientPariticipatingSiteId { get; set; }

        [JsonPropertyName("pas.cvt_relatedservice")]
        public Guid? PatientParticipatingSiteServiceId { get; set; }

        [JsonPropertyName("pasvc.resourcespecid")]
        public Guid? PatientParticipatingSiteServiceSpecId { get; set; }

        [JsonPropertyName("prs.cvt_participatingsiteid")]
        public Guid ProviderPariticipatingSiteId { get; set; }

        [JsonPropertyName("prs.cvt_relatedservice")]
        public Guid? ProviderParticipatingSiteServiceId { get; set; }

        [JsonPropertyName("prsvc.resourcespecid")]
        public Guid? ProviderParticipatingSiteServiceSpecId { get; set; }

        [JsonPropertyName("psf.mcs_facilityid")]
        public Guid ProviderFacilityId { get; set; }

        [JsonPropertyName("psf.mcs_name")]
        public string ProviderFacilityName { get; set; }

        [JsonPropertyName("psf.mcs_stationnumber")]
        public string ProviderFacilityStationNumber { get; set; }

        [JsonPropertyName("psf.mcs_timezone")]
        public int ProviderFacilityTimeZoneCode { get; set; }

        [JsonPropertyName("psfv.name")]
        public string ProviderFacilityVISN { get; set; }

        [JsonPropertyName("cvt_resourcepackageid")]
        public Guid SchedulingPackageId { get; set; }

        [JsonPropertyName("cvt_availabletelehealthmodality")]
        public int SchedulingPackageModalityCode { get; set; }

        [JsonPropertyName("cvt_name")]
        public string SchedulingPackageName { get; set; }

        [JsonPropertyName("cvt_patientlocationtype")]
        public int PatientLocationType { get; set; }

        [JsonPropertyName("cvt_groupappointment")]
        public bool GroupAppointment { get; set; }

        [JsonPropertyName("_cvt_relatedservice_value")]
        public Guid? SchedulingPackageServiceId { get; set; }

        [JsonPropertyName("spsvc.resourcespecid")]
        public Guid? SchedulingPackageServiceSpecId { get; set; }
    }

    public class ParticipatingSitesResponseMessage : TmpBaseResponseMessage
    {
        [JsonPropertyName("@odata.context")]
        public string Context { get; set; }

        [JsonPropertyName("value")]
        public List<ParticipatingSite> ParticipatingSites { get; set; }
    }

    public class ParticipatingSite
    {
        [JsonPropertyName("gru.systemuserid")]
        public Guid GroupRelatedUserId { get; set; }

        [JsonPropertyName("grrer.resourceid")]
        public Guid? GroupRelatedResourceClinicId { get; set; }

        [JsonPropertyName("grrer.name")]
        public string GroupRelatedResourceClinicName { get; set; }

        [JsonPropertyName("gr.mcs_groupresourceid")]
        public Guid? GroupResourceClinicId { get; set; }

        [JsonPropertyName("gr.mcs_name")]
        public string GroupResourceClinicName { get; set; }

        [JsonPropertyName("grr.mcs_resourceid")]
        public Guid? GroupResourceRelatedClinicId { get; set; }

        [JsonPropertyName("grr.mcs_name")]
        public string GroupResourceRelatedClinicName { get; set; }

        [JsonPropertyName("grr.mcs_type")]
        public int GroupResourceRelatedResourceType { get; set; }

        [JsonPropertyName("grru.systemuserid")]
        public Guid GroupResourceRelatedUserId { get; set; }

        [JsonPropertyName("grrs.mcs_timezone")]
        public int GroupResourceTimeZoneCode { get; set; }

        [JsonPropertyName("ras.siteid")]
        public Guid RelatedActualSiteId { get; set; }

        [JsonPropertyName("ras.name")]
        public string RelatedActualSiteName { get; set; }

        [JsonPropertyName("rg.mcs_resourcegroupid")]
        public Guid? ResourceGroupId { get; set; }

        [JsonPropertyName("sru.systemuserid")]
        public Guid ResourceRelatedUserId { get; set; }

        [JsonPropertyName("schedulingPackageId")]
        public Guid SchedulingPackageId { get; set; }

        [JsonPropertyName("sr.cvt_schedulingresourceid")]
        public Guid SchedulingResourceId { get; set; }

        [JsonPropertyName("srr.mcs_resourceid")]
        public Guid? SingleResourceClinicId { get; set; }

        [JsonPropertyName("srr.mcs_name")]
        public string SingleResourceClinicName { get; set; }

        [JsonPropertyName("srrer.resourceid")]
        public Guid? SingleRelatedResourceClinicId { get; set; }

        [JsonPropertyName("srrer.name")]
        public string SingleRelatedResourceClinicName { get; set; }

        [JsonPropertyName("srsu.systemuserid")]
        public Guid SingleResourceUserId { get; set; }
    }

    public class ResourcePackageComparer : IEqualityComparer<ResourcePackage>
    {
        public bool Equals(ResourcePackage x, ResourcePackage y)
        {
            return x.SchedulingPackageId.Equals(y.SchedulingPackageId) &&
                x.PatientPariticipatingSiteId.Equals(y.PatientPariticipatingSiteId) &&
                x.ProviderPariticipatingSiteId.Equals(y.ProviderPariticipatingSiteId);
        }

        public int GetHashCode(ResourcePackage rp) => rp.SchedulingPackageId.GetHashCode() ^ rp.PatientPariticipatingSiteId.GetHashCode() ^ rp.ProviderPariticipatingSiteId.GetHashCode();
    }
}
