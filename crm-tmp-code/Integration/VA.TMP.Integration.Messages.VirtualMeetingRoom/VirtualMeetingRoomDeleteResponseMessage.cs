using System.Runtime.Serialization;
using VA.TMP.Integration.Core;

namespace VA.TMP.Integration.Messages.VirtualMeetingRoom
{
    /// <summary>
    /// Message for VirtualMeetingRoomCreateResponseMessage.
    /// </summary>
    [DataContract]
    public class VirtualMeetingRoomDeleteResponseMessage : TmpBaseResponseMessage
    {
        /// <summary>
        /// Gets or sets the Misc data.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string MiscData { get; set; }
    }
}