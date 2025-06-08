using System.Runtime.Serialization;
using VA.TMP.Integration.Core;

namespace VA.TMP.Integration.Messages.VideoVisit
{
    [DataContract()]
    public class VideoVisitGetLoanedDevicesRequestMessage : TmpBaseRequestMessage
    {
        [DataMember(EmitDefaultValue = false)]
        public string ICN { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string SamlToken { get; set; }
    }
}
