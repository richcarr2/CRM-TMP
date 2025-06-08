using System.Runtime.Serialization;
using VA.TMP.Integration.Core;

namespace VA.TMP.Integration.Messages.Vista
{
    [DataContract]
    public class ViaLoginRequestMessage : TmpBaseRequestMessage
    {
        [DataMember(EmitDefaultValue = false)]
        public string AccessCode { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string VerifyCode { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string StationNumber { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string FakeResponseType { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string SamlToken { get; set; }
    }
}