using System.Runtime.Serialization;
using VA.TMP.Integration.Core;

namespace VA.TMP.Integration.Messages.Vista
{
    [DataContract]
    public class ViaLoginResponseMessage : TmpBaseResponseMessage
    {
        [DataMember(EmitDefaultValue = false)]
        public string UserDuz { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int VIMTProcessingTime { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int VIMTLag { get; set; }
    }
}