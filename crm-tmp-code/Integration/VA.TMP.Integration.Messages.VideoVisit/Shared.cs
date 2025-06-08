using System.Runtime.Serialization;

namespace VA.TMP.Integration.Messages.VideoVisit
{
    [DataContract]
    public class WriteResults
    {
        [DataMember(EmitDefaultValue = false)]
        public WriteResult[] WriteResult { get; set; }
    }

    [DataContract]
    public class WriteResult
    {
        [DataMember(EmitDefaultValue = false)]
        public string PersonId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public PersonName Name { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string FacilityCode { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string FacilityName { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string ClinicIen { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string ClinicName { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string DateTime { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public VistaStatus VistaStatus { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Reason { get; set; }
    }

    public enum VistaStatus
    {
        BOOKED,
        FAILED_TO_BOOK,
        RECEIVED,
        FAILED_TO_RECEIVE,
        CANCELLED,
        FAILED_TO_CANCEL
    }

    [DataContract]
    public class PersonName
    {
        [DataMember(EmitDefaultValue = false)]
        public string FirstName { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string LastName { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string MiddleInitial { get; set; }
    }
}