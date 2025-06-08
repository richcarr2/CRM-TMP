using System;
using System.Runtime.Serialization;

namespace Ec.JsonWebToken.Messages
{
    /// <summary>
    /// EC Jwt Encrypt Token Request.
    /// </summary>
    [DataContract]
    public class EcJwtEncryptTokenRequest
    {
        /// <summary>
        /// Default Constructor.
        /// </summary>
        public EcJwtEncryptTokenRequest()
        {
            MessageId = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Gets or sets the Message Id.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string MessageId { get; set; }

        /// <summary>
        /// Gets/Sets SAML Token.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string SamlToken { get; set; }
    }
}