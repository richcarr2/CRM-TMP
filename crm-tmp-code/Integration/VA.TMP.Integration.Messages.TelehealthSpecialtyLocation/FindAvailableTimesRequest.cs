using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace VA.TMP.Integration.Messages.TelehealthSpecialtyLocation
{
    [Serializable]
    public class FindAvailableTimesRequest
    {
        [JsonPropertyName("parameters")]
        public List<FindAvailableTimesParameter> Parameters { get; set; }

        [JsonPropertyName("endDate")]
        public DateTime? EndDate { get; set; }

        [JsonPropertyName("availabilityStartDate")]
        public DateTime? AvailabilityStartDate { get; set; }
    }

    [Serializable]
    public class FindAvailableTimesParameter
    {
        [JsonPropertyName("clinicId")]
        public Guid ClinicId { get; set; }

        [JsonPropertyName("clinicName")]
        public string ClinicName { get; set; }

        [JsonPropertyName("facilityName")]
        public string FacilityName { get; set; }

        [JsonPropertyName("facilityTimeZoneCode")]
        public int FacilityTimeZoneCode { get; set; }

        [JsonPropertyName("cvt_groupappointment")]
        public bool GroupAppointment { get; set; }

        [JsonPropertyName("cvt_patientlocationtype")]
        public int PatientLocationType { get; set; }

        [JsonPropertyName("providerFacilityTimeZoneCode")]
        public int ProviderFacilityTimeZoneCode { get; set; }

        [JsonPropertyName("providerFacilityStationName")]
        public string ProviderFacilityStationName { get; set; }

        [JsonPropertyName("providerFacilityStationNumber")]
        public string ProviderFacilityStationNumber { get; set; }

        [JsonPropertyName("providerId")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Guid ProviderId { get; set; }

        [JsonPropertyName("schedulingPackageModality")]
        public string SchedulingPackageModality { get; set; }

        [JsonPropertyName("schedulingPackageName")]
        public string SchedulingPackageName { get; set; }

        [JsonPropertyName("serviceId")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Guid ServiceId { get; set; }

        [JsonPropertyName("serviceSpecId")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Guid ServiceSpecId { get; set; }

        [JsonPropertyName("siteName")]
        public string SiteName { get; set; }

        [JsonPropertyName("siteId")]
        public Guid SiteId { get; set; }

        [JsonPropertyName("visn")]
        public string VISN { get; set; }


    }
}
