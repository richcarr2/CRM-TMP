using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace VA.TMP.Integration.Messages.TelehealthSpecialtyLocation
{
    public class TelehealthSpecialtyLocationsFindAvailableTimesResponse : TelehealthSpecialtyLocationsResponse
    {
        [JsonPropertyName("availabletimes")]
        public List<AvailableTime> AvailableTimes { get; set; } = new List<AvailableTime>();
    }

    public class AvailableTime
    {
        [JsonPropertyName("clinic")]
        public string Clinic { get; set; }

        [JsonPropertyName("date")]
        public string Date { get; set; }

        [JsonPropertyName("duration")]
        public int Duration { get; set; }

        [JsonPropertyName("facility")]
        public string Facility { get; set; }

        [JsonPropertyName("groupAppointment")]
        public bool GroupAppointment { get; set; }

        [JsonPropertyName("patientLocationType")]
        public string PatientLocationType { get; set; }

        [JsonPropertyName("modality")]
        public string Modality { get; set; }

        [JsonPropertyName("patientFacilityTimeZone")]
        public string PatientFacilityTimeZone { get; set; }

        [JsonPropertyName("provider")]
        public string Provider { get; set; }

        [JsonPropertyName("providerLocationStationName")]
        public string ProviderFacilityStationName { get; set; }

        [JsonPropertyName("providerFacilityStationNumber")]
        public string ProviderFacilityStationNumber { get; set; }

        [JsonPropertyName("providerFacilityTimeZone")]
        public string ProviderFacilityTimeZone { get; set; }

        [JsonPropertyName("schedulingPackageName")]
        public string SchedulingPackageName { get; set; }

        [JsonPropertyName("time")]
        public string Time { get; set; }

        [JsonPropertyName("visn")]
        public string VISN { get; set; }
    }

    public class AvailableTimeComparer : IEqualityComparer<AvailableTime>
    {
        public bool Equals(AvailableTime x, AvailableTime y)
        {
            //Check whether the compared objects reference the same data.
            if (Object.ReferenceEquals(x, y)) return true;

            //Check whether any of the compared objects is null.
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            //Check whether the products' properties are equal.
            return x.Clinic == y.Clinic && x.Time == y.Time && x.Date == y.Date && x.Provider == y.Provider;
        }

        public int GetHashCode(AvailableTime availableTime)
        {
            //Check whether the object is null
            if (Object.ReferenceEquals(availableTime, null)) return 0;

            //Get hash code for the Name field if it is not null.
            int hashClinicName = availableTime.Clinic == null ? 0 : availableTime.Clinic.GetHashCode();

            //Get hash code for the Code field.
            int hashDate = availableTime.Date.GetHashCode();

            int hashProvider = availableTime.Provider.GetHashCode();

            int hashTime = availableTime.Time.GetHashCode();

            //Calculate the hash code for the product.
            return hashClinicName ^ hashProvider ^ hashDate ^ hashTime;
        }
    }
}
