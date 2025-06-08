using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using VA.TMP.Integration.Core;

namespace VA.TMP.Integration.Messages.HealthShare
{
    /// <summary>
    /// Message for TmpHealthShareMakeCancelOutboundRequestMessage.
    /// </summary>
    [DataContract]
    public class TmpHealthShareMakeCancelOutboundRequestMessage : TmpBaseRequestMessage
    {
        /// <summary>
        /// Default Constructor.
        /// </summary>
        public TmpHealthShareMakeCancelOutboundRequestMessage()
        {
            Patients = new List<Guid>();
        }

        /// <summary>
        /// Gets or Sets Patient.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public List<Guid> Patients { get; set; }

        /// <summary>
        /// Gets or Sets Service Appointment Id.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public Guid ServiceAppointmentId { get; set; }

        /// <summary>
        /// Gets or Sets Appointment Id.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public Guid? AppointmentId { get; set; }

        /// <summary>
        /// Gets or Sets VisitStatus.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string VisitStatus { get; set; }

        /// <summary>
        /// Gets or Sets Vista Integration Result in the case of Group Cancel.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public Guid? VistaIntegrationResultId { get; set; }
    }
}