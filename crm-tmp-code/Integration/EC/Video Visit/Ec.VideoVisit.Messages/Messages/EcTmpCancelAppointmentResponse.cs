using System;
using System.Runtime.Serialization;

namespace Ec.VideoVisit.Messages
{
    [DataContract]
    public class EcTmpCancelAppointmentResponse
    {
        public EcTmpCancelAppointmentResponse()
        {
            MessageId = Guid.NewGuid().ToString();
        }

        [DataMember(EmitDefaultValue = false)]
        public string MessageId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string HttpStatusCode { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public EcTmpWriteResults EcTmpWriteResults { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool ExceptionOccured { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string ExceptionMessage { get; set; }
    }
}