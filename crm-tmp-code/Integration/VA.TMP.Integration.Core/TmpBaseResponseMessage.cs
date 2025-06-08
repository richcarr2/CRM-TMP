using System;
using System.Runtime.Serialization;

namespace VA.TMP.Integration.Core
{
    [DataContract]
    public abstract class TmpBaseResponseMessage
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public TmpBaseResponseMessage()
        {
            MessageId = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Gets or sets the MessageId.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string MessageId { get; set; }

        /// <summary>
        /// Indicates if an exception anywhere along the way that wasn't caught
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool ExceptionOccured { get; set; }

        /// <summary>
        /// Message corresponding to any uncaught Exception
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string ExceptionMessage { get; set; }

        /// <summary>
        /// Serialized Ec Request
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string SerializedInstance { get; set; }

        /// <summary>
        /// Serialized Request Message
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string VimtRequest { get; set; }

        /// <summary>
        /// Serialized Response Message
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string VimtResponse { get; set; }

        /// <summary>
        /// # of milliseconds the code spent in the Enterprise Component(s)
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int EcProcessingMs { get; set; }

        /// <summary>
        /// # of milliseconds the code spent
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int VimtProcessingMs { get; set; }

        /// <summary>
        /// # of milliseconds the code spent from calling until returning the response
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int VimtLagMs { get; set; }
    }
}