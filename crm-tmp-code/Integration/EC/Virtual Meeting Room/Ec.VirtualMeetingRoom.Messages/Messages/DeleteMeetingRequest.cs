using System;
using System.Runtime.Serialization;

namespace Ec.VirtualMeetingRoom.Messages
{
    [DataContract]
    public class EcVirtualDeleteMeetingRequest
    {
        public EcVirtualDeleteMeetingRequest()
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
        public string mcs_MiscData { get; set; }
    }
}