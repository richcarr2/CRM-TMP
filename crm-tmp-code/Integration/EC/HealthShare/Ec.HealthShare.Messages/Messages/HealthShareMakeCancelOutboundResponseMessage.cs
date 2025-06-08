using System;
using System.Runtime.Serialization;

namespace Ec.HealthShare.Messages
{
    /// <summary>
    /// EC HealthShare Make Cancel Outbound Response.
    /// </summary>
    [DataContract]
    public class EcHealthShareMakeCancelOutboundResponseMessage
    {
        /// <summary>
        /// Default Constructor.
        /// </summary>
        public EcHealthShareMakeCancelOutboundResponseMessage()
        {
            MessageId = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Gets or sets the Message Id.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string MessageId { get; set; }

        /// <summary>
        /// Gets or sets whether Exception occurred.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool ExceptionOccured { get; set; }

        /// <summary>
        /// Gets or sets Exception message.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string ExceptionMessage { get; set; }
    }
}