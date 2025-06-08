using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace VA.TMP.Integration.Messages.TelehealthSpecialtyLocation
{
    public class FindAvailableTimesResponse : TelehealthSpecialtyLocationsResponse
    {
        [JsonPropertyName("providerAvailability")]
        public List<AvailableAppointmentTime> AvailableAppointmentTimes { get; set; }
    }

    public class AvailableAppointmentTime
    {
        [JsonPropertyName("availabilityStartDate")]
        public string AvailabilityStartDate { get; set; }

        [JsonPropertyName("clinicId")]
        public Guid ClinicId { get; set; }

        [JsonPropertyName("clinicName")]
        public string ClinicName { get; set; }

        [JsonPropertyName("duration")]
        public int Duration { get; set; }

        [JsonPropertyName("endDate")]
        public string EndDate { get; set; }

        [JsonPropertyName("facility")]
        public string Facility { get; set; }

        [JsonPropertyName("groupAppointment")]
        public bool GroupAppointment { get; set; }

        [JsonPropertyName("patientLocationType")]
        public string PatientLocationType { get; set; }

        [JsonPropertyName("patientFacilityTimeZone")]
        public string PatientFacilityTimeZone { get; set; }

        [JsonPropertyName("providerFacilityStationName")]
        public string ProviderFacilityStationName { get; set; }

        [JsonPropertyName("providerFacilityStationNumber")]
        public string ProviderFacilityStationNumber { get; set; }

        [JsonPropertyName("providerFacilityTimeZone")]
        public string ProviderFacilityTimeZone { get; set; }

        [JsonPropertyName("providerId")]
        public Guid ProviderId { get; set; }

        [JsonPropertyName("providerName")]
        public string ProviderName { get; set; }

        [JsonPropertyName("schedulingPackageModality")]
        public string SchedulingPackageModality { get; set; }

        [JsonPropertyName("schedulingPackageName")]
        public string SchedulingPackageName { get; set; }

        [JsonPropertyName("siteId")]
        public Guid SiteId { get; set; }

        [JsonPropertyName("siteName")]
        public string SiteName { get; set; }

        [JsonPropertyName("visn")]
        public string VISN { get; set; }
    }
}
