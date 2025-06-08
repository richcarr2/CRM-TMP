using System.Runtime.Serialization;
using VA.TMP.Integration.Core;

namespace VA.TMP.Integration.Messages.VirtualMeetingRoom
{
    /// <summary>
    /// Message for VirtualMeetingRoomCreateResponseMessage.
    /// </summary>
    [DataContract]
    public class VirtualMeetingRoomCreateResponseMessage : TmpBaseResponseMessage
    {
        /// <summary>
        /// Gets or sets the Meeting Room Name.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string MeetingRoomName { get; set; }

        /// <summary>
        /// Gets or sets the Patient URL.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string PatientUrl { get; set; }

        /// <summary>
        /// Gets or sets the Provider URL.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string ProviderUrl { get; set; }

        /// <summary>
        /// Gets or sets the Patient PIN.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string PatientPin { get; set; }

        /// <summary>
        /// Gets or sets the Provider PIN.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string ProviderPin { get; set; }

        /// <summary>
        /// Gets or sets the CorrelationId.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string AppointmentId { get; set; }

        /// <summary>
        /// Gets or sets the Dialing Alias.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string DialingAlias { get; set; }

        /// <summary>
        /// Gets or sets the Misc data.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string MiscData { get; set; }
    }
}