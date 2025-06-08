using System;
using System.Runtime.Serialization;
using VA.TMP.Integration.Core;

namespace VA.TMP.Integration.Messages.VirtualMeetingRoom
{
    /// <summary>
    /// Message for VirtualMeetingRoomCreateRequestMessage.
    /// </summary>
    [DataContract]
    public class VirtualMeetingRoomCreateRequestMessage : TmpBaseRequestMessage
    {
        /// <summary>
        /// Gets or sets the Appointment Id.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public Guid AppointmentId { get; set; }

        /// <summary>
        /// Gets or sets the Patient Id.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public Guid PatientId { get; set; }

        /// <summary>
        /// Gets or sets the Provider Id.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public Guid ProviderId { get; set; }

        /// <summary>
        /// Gets or sets the Misc Flag.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string MiscData { get; set; }
    }
}