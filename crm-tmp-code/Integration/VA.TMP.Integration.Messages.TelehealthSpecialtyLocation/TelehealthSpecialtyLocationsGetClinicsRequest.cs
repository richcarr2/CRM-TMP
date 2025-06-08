using System.ComponentModel;

namespace VA.TMP.Integration.Messages.TelehealthSpecialtyLocation
{
    public class TelehealthSpecialtyLocationsGetClinicLocationsRequest
    {
        [DefaultValue("983")]
        public string FacilityStationNumber { get; set; }
        [DefaultValue("983")]
        public string SiteStationNumber { get; set; }
        [DefaultValue("Dental")]
        public string SpecialtyName { get; set; }
    }
}
