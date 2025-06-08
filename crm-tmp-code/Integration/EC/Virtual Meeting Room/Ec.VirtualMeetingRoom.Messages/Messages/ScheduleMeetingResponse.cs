using System;
using System.Runtime.Serialization;

namespace Ec.VirtualMeetingRoom.Messages
{
    [DataContract]
    public class EcVyoptaSMScheduleMeetingResponse
    {
        public EcVyoptaSMScheduleMeetingResponse()
        {
            MessageId = Guid.NewGuid().ToString();
        }

        [DataMember(EmitDefaultValue = false)]
        public string MessageId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string mcs_DialingAlias { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string mcs_EncounterId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string mcs_MiscData { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool ExceptionOccured { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string ExceptionMessage { get; set; }
    }
}