using System;
using System.ComponentModel;

namespace VA.TMP.Integration.Messages.TelehealthSpecialtyLocation
{
    public class TelehealthSpecialtyLocationsFindAvailableTimesRequest
    {
        [DefaultValue(30)]
        public int DateRange { get; set; }
        [DefaultValue(typeof(DateTime),"2023, 9, 1")]
        public DateTime DesiredDate { get; set; }
        [DefaultValue("915")]
        public string PatientFacility { get; set; }
        [DefaultValue("179")]
        public string StopCode { get; set; }
    }
}
