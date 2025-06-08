using System;
using System.Runtime.Serialization;

namespace Ec.VideoVisit.Messages
{
    [DataContract]
    public class EcTmpUpdateAppointmentRequest
    {
        public EcTmpUpdateAppointmentRequest()
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
        public EcTmpPatients[] Patients { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int Duration { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string DateTime { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public EcTmpStatus Status { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public EcTmpSchedulingRequestType SchedulingRequestType { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool SchedulingRequestTypeSpecified { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string AppointmentKind { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public EcTmpAppointmentType Type { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool TypeSpecified { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string BookingNotes { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string DesiredDate { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool DesiredDateSpecified { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public EcTmpProviders[] Providers { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Version { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string SamlToken { get; set; }
    }
}