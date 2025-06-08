using System.Runtime.Serialization;
using VA.TMP.Integration.Core;

namespace VA.TMP.Integration.Messages.VideoVisit
{
    /// <summary>
    /// Message for VideoVisitUpdateResponseMessage.
    /// </summary>
    [DataContract]
    public class VideoVisitUpdateResponseMessage : TmpBaseResponseMessage
    {
        /// <summary>
        /// individual booking/cancelation result for each patient
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public WriteResults WriteResults { get; set; }
    }
}