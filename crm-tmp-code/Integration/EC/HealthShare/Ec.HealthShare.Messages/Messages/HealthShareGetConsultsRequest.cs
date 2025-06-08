using System;
using System.Runtime.Serialization;

namespace Ec.HealthShare.Messages
{
    /// <summary>
    /// EC HealthShare Get Consults Request.
    /// </summary>
    [DataContract]
    public class EcHealthShareGetConsultsRequest
    {
        /// <summary>
        /// Default Constructor.
        /// </summary>
        public EcHealthShareGetConsultsRequest()
        {
            MessageId = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Gets or sets the Message Id.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string MessageId { get; set; }

        /// <summary>
        /// Gets or sets the Unique Id.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string UniqueId { get; set; }

        /// <summary>
        /// Gets or sets Patient DFN.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string PatientDfn { get; set; }

        /// <summary>
        /// Gets or sets Patient ICN.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string PatientIcn { get; set; }

        /// <summary>
        /// Gets or sets Station Number.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int StationNumber { get; set; }

        /// <summary>
        /// Gets or set the whether this is Patient/Provider Side.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string Side { get; set; }
    }
}