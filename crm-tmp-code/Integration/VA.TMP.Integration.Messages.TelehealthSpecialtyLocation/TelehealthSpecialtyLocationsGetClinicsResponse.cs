using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace VA.TMP.Integration.Messages.TelehealthSpecialtyLocation
{
    public class TelehealthSpecialtyLocationsGetClinicLocationsResponse : TelehealthSpecialtyLocationsResponse
    {
        [JsonPropertyName("clinicLocations")]
        public List<FacilityClinicLocation> ClinicLocations { get; set; }
    }
}

public class FacilityClinicLocation
{
    [JsonPropertyName("clinicName")]
    public string ClinicName { get; set; }
    [JsonPropertyName("siteName")]
    public string SiteName { get; set; }
}
