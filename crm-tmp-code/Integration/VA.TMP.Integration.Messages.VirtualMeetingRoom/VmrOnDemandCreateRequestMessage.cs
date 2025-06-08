using System;
using System.Runtime.Serialization;
using VA.TMP.Integration.Core;

namespace VA.TMP.Integration.Messages.VirtualMeetingRoom
{
    /// <summary>
    /// Message for VmrOnDemandCreateRequestMessage.
    /// </summary>
    [DataContract]
    public class VmrOnDemandCreateRequestMessage : TmpBaseRequestMessage
    {
        /// <summary>
        /// Gets or sets the Video On Demand Id.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public Guid VideoOnDemandId { get; set; }

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
