using System;
using System.Runtime.Serialization;

namespace Ec.JsonWebToken.Messages
{
    /// <summary>
    /// EC Jwt Encrypt Token Response.
    /// </summary>
    [DataContract]
    public class EcJwtEncryptTokenResponse
    {
        /// <summary>
        /// Default Constructor.
        /// </summary>
        public EcJwtEncryptTokenResponse()
        {
            MessageId = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Gets or sets the Message Id.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string MessageId { get; set; }

        /// <summary>
        /// Gets/Sets Encrypted JWT.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string EncryptedJwtToken { get; set; }

        /// <summary>
        /// Gets/Sets whether Exception occurred.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool ExceptionOccured { get; set; }

        /// <summary>
        /// Gets/Set Exception message.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string ExceptionMessage { get; set; }
    }
}