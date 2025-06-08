using System;
using System.Runtime.Serialization;
using VA.TMP.Integration.Core;

namespace VA.TMP.Integration.Messages.VirtualMeetingRoom
{
    /// <summary>
    /// Message for VirtualMeetingRoomCreateRequestMessage.
    /// </summary>
    [DataContract]
    public class VirtualMeetingRoomDeleteRequestMessage : TmpBaseRequestMessage
    {
        /// <summary>
        /// Gets or sets the CorrelationId.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public Guid AppointmentId { get; set; }

        /// <summary>
        /// Gets or sets the Misc Flag.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string MiscData { get; set; }
    }
}