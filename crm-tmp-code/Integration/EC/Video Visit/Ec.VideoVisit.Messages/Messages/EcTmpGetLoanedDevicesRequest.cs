using System.Runtime.Serialization;

namespace Ec.VideoVisit.Messages
{
    [DataContract()]
    public class EcTmpGetLoanedDevicesRequest
    {
        [DataMember(EmitDefaultValue = false)]
        public string ICN { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string SamlToken { get; set; }
    }
}
