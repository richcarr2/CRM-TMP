using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using VA.TMP.Integration.Core;

namespace VA.TMP.Integration.Api.TelehealthSpecialtyLocation.Models
{
    public class Facility
    {
        [JsonPropertyName("@odata.etag")]
        public string eTag { get; set; }

        [JsonPropertyName("mcs_facilityid")]
        public Guid Id { get; set; }

        [JsonPropertyName("mcs_timezone")]
        public int Timezone { get; set; }

        [JsonPropertyName("VISN.name")]
        public string Visn { get; set; }
    }

    public class FacilityResponse : TmpBaseResponseMessage
    {
        [JsonPropertyName("@odata.context")]
        public string Context { get; set; }

        [JsonPropertyName("value")]
        public List<Facility> Facilities { get; set; }
    }
}
