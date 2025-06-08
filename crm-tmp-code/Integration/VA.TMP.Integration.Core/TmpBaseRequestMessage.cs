using System;
using System.Runtime.Serialization;

namespace VA.TMP.Integration.Core
{
    [DataContract]
    public abstract class TmpBaseRequestMessage
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public TmpBaseRequestMessage()
        {
            MessageId = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Gets or sets the MessageId.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string MessageId { get; set; }

        /// <summary>
        /// Gets or sets the CRM Organization Name.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string OrganizationName { get; set; }

        /// <summary>
        /// Gets or sets the UserId.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public Guid UserId { get; set; }

        /// <summary>
        /// Gets or sets whether to log requests.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool LogRequest { get; set; }
    }
}