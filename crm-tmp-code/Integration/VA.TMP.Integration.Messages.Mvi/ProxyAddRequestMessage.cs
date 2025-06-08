using System;
using System.Runtime.Serialization;
using VA.TMP.Integration.Core;

namespace VA.TMP.Integration.Messages.Mvi
{
    /// <summary>
    /// Message for ProxyAddRequestMessage.
    /// </summary>
    [DataContract]
    public class ProxyAddRequestMessage : TmpBaseRequestMessage
    {
        /// <summary>
        /// Gets or sets User's first name
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string UserFirstName { get; set; }

        /// <summary>
        /// Gets or sets User's last name
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string UserLastName { get; set; }

        /// <summary>
        /// Gets or sets whether to use a fake response.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string FakeResponseType { get; set; }

        /// <summary>
        /// Gets or set the Service Appointment Id.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public Guid? ServiceAppointmentId { get; set; }

        /// <summary>
        /// Gets or set the AppointmentId
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public Guid? AppointmentId { get; set; }

        /// <summary>
        /// Gets or sets ProcessingCode.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string ProcessingCode { get; set; }

        /// <summary>
        /// Gets or sets ReturnMviMessagesInResponse.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool ReturnMviMessagesInResponse { get; set; }

        /// <summary>
        /// Gets or sets PatientVeteran.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool PatientVeteran { get; set; }

        /// <summary>
        /// Gets or sets PatientServiceConnected.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool PatientServiceConnected { get; set; }

        /// <summary>
        /// Gets or sets PatientType.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int PatientType { get; set; }

        /// <summary>
        /// Gets or sets Veteran Party Id.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public Guid VeteranPartyId { get; set; }
    }
}