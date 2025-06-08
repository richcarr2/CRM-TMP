using System.Runtime.Serialization;
using VA.TMP.Integration.Core;

namespace VA.TMP.Integration.Messages.HealthShare
{
    [DataContract]
    public class TmpHealthSharePersonSearchResponseMessage : TmpBaseResponseMessage
    {
        /// <summary>
        /// Gets or sts the Retrieve or Search Person Response.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public TmpHealthShareRetrieveOrSearchPersonResponse RetrieveOrSearchPersonResponse { get; set; }
    }
}
