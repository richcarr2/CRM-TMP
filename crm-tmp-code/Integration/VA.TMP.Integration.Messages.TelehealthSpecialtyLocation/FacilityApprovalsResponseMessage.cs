using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace VA.TMP.Integration.Messages.TelehealthSpecialtyLocation
{
    public class FacilityApprovalsResponseMessage
    {
        [JsonPropertyName("@odata.context")]
        public string Context { get; set; }

        [JsonPropertyName("value")]
        public List<FacilityApproval> FacilityApprovals { get; set; }
    }

    public class FacilityApproval
    {
        [JsonPropertyName("@odata.etag")]
        public string Etag { get; set; }

        [JsonPropertyName("psf.mcs_facilityid")]
        public Guid FacilityId { get; set; }

        [JsonPropertyName("psf.mcs_name")]
        public string FacilityName { get; set; }

        [JsonPropertyName("gru.systemuserid")]
        public Guid GroupRelatedUserId { get; set; }

        [JsonPropertyName("grs.mcs_name")]
        public string GroupResourceClinicName { get; set; }

        [JsonPropertyName("grru.systemuserid")]
        public Guid GroupResourceRelatedUserId { get; set; }

        [JsonPropertyName("gru.fullname")]
        public string GroupResourceUserName { get; set; }

        [JsonPropertyName("ps.cvt_relatedservice")]
        public Guid? ParticipatingSiteServiceId { get; set; }

        [JsonPropertyName("ras.siteid")]
        public Guid RelatedActualSiteId { get; set; }

        [JsonPropertyName("sru.systemuserid")]
        public Guid ResourceRelatedUserId { get; set; }

        [JsonPropertyName("rp.cvt_relatedservice")]
        public Guid? SchedulingPackageServiceId { get; set; }

        [JsonPropertyName("srr.mcs_name")]
        public string SingleResourceClinicName { get; set; }

        [JsonPropertyName("srsu.fullname")]
        public string SingleResourceUserName { get; set; }

        [JsonPropertyName("srsu.systemuserid")]
        public Guid SingleResourceUserId { get; set; }

        [JsonPropertyName("pss.mcs_name")]
        public string SiteName { get; set; }
    }
}
