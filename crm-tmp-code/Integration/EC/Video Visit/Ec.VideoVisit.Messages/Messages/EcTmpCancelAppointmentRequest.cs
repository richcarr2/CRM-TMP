using System;
using System.Runtime.Serialization;

namespace Ec.VideoVisit.Messages
{
    [DataContract(Name = "Appointment", Namespace = "gov.va.vamf.videoconnect/1.0")]
    public class EcTmpCancelAppointmentRequest
    {
        public EcTmpCancelAppointmentRequest()
        {
            MessageId = Guid.NewGuid().ToString();
        }

        [DataMember(EmitDefaultValue = false)]
        public string MessageId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string OrganizationName { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public Guid UserId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public Guid RelatedParentId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string RelatedParentEntityName { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string RelatedParentFieldName { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Id { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string SourceSystem { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public EcTmpPersonBookingStatuses PatientBookingStatuses { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string SamlToken { get; set; }
    }

    [DataContract]
    public class EcTmpPersonBookingStatus
    {
        [DataMember(EmitDefaultValue = false)]
        public EcTmpPersonIdentifier Id { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public EcTmpStatus Status { get; set; }
    }

    [DataContract]
    public class EcTmpPersonBookingStatuses
    {
        [DataMember(EmitDefaultValue = false)]
        public EcTmpPersonBookingStatus[] PersonBookingStatus { get; set; }
    }
}