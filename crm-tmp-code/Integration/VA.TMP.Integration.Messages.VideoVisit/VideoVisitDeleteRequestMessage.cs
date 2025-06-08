using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using VA.TMP.Integration.Core;

namespace VA.TMP.Integration.Messages.VideoVisit
{
    /// <summary>
    /// Message for VideoVisitDeleteRequestMessage.
    /// </summary>
    [DataContract]
    public class VideoVisitDeleteRequestMessage : TmpBaseRequestMessage
    {
        /// <summary>
        /// Gets or sets the Service Appointment Id.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public Guid AppointmentId { get; set; }

        /// <summary>
        /// List of patient IDs who are being canceled
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public List<Guid> CanceledPatients { get; set; }

        /// <summary>
        /// indiciates if an individual patient is being canceled, or if it is the whole appointment
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool WholeAppointmentCanceled { get; set; }
    }
}