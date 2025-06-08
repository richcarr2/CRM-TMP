using System.Runtime.Serialization;
using VA.TMP.Integration.Core;

namespace VA.TMP.Integration.Messages.VideoVisit
{
    /// <summary>
    /// Message for VideoVisitCreateResponseMessage.
    /// </summary>
    [DataContract]
    public class VideoVisitCreateResponseMessage : TmpBaseResponseMessage
    {
        [DataMember(EmitDefaultValue = false)]
        public WriteResults WriteResults { get; set; }
    }
}