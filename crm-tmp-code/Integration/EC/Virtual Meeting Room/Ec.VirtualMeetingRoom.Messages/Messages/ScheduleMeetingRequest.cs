using System;
using System.Runtime.Serialization;

namespace Ec.VirtualMeetingRoom.Messages
{
    [DataContract]
    public class EcVyoptaSMScheduleMeetingRequest
    {
        public EcVyoptaSMScheduleMeetingRequest()
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
        public string mcs_EncounterId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public DateTime mcs_EndTime { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string mcs_GuestName { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string mcs_GuestPin { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string mcs_HostName { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string mcs_HostPin { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string mcs_MeetingRoomName { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string mcs_MiscData { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public DateTime mcs_StartTime { get; set; }
    }
}