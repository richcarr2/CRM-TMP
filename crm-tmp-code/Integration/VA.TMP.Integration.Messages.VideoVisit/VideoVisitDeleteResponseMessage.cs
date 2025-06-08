using System.Runtime.Serialization;
using VA.TMP.Integration.Core;

namespace VA.TMP.Integration.Messages.VideoVisit
{
    /// <summary>
    /// Message for VideoVisitDeleteResponseMessage.
    /// </summary>
    [DataContract]
    public class VideoVisitDeleteResponseMessage : TmpBaseResponseMessage
    {
        [DataMember(EmitDefaultValue = false)]
        public WriteResults WriteResults { get; set; }
    }
}