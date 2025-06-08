using System.Runtime.Serialization;
using VA.TMP.Integration.Core;

namespace VA.TMP.Integration.Messages.HealthShare
{
    [DataContract]
    public class TmpHealthShareUnattendedSearchRequest : TmpBaseRequestMessage
    {
        /// <summary>
        /// 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool IsAttended { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string PatientIdentifier { get; set; }
    }
}
