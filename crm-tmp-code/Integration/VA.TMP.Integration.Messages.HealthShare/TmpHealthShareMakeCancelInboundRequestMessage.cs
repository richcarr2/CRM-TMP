using System.Runtime.Serialization;
using VA.TMP.Integration.Core;

namespace VA.TMP.Integration.Messages.HealthShare
{
    /// <summary>
    /// Message for TmpHealthShareMakeCancelInboundRequestMessage.
    /// </summary>
    [DataContract]
    public class TmpHealthShareMakeCancelInboundRequestMessage : TmpBaseRequestMessage
    {
        /// <summary>
        /// Gets or Sets ControlId.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string ControlId { get; set; }

        /// <summary>
        /// Gets or Sets Status.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string Status { get; set; }

        /// <summary>
        /// Gets or Sets FailureReason.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string FailureReason { get; set; }
    }
}