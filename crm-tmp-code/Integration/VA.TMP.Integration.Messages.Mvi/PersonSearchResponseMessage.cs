using System.Runtime.Serialization;
using VA.TMP.Integration.Core;

namespace VA.TMP.Integration.Messages.Mvi
{
    /// <summary>
    /// Message for PersonSearchResponseMessage.
    /// </summary>
    [DataContract]
    public class PersonSearchResponseMessage : TmpBaseResponseMessage
    {
        /// <summary>
        /// Gets or sts the Retrieve or Search Person Response.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public RetrieveOrSearchPersonResponse RetrieveOrSearchPersonResponse { get; set; }
    }
}