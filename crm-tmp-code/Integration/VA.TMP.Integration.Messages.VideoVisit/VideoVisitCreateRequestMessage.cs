using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using VA.TMP.Integration.Core;

namespace VA.TMP.Integration.Messages.VideoVisit
{
    /// <summary>
    /// Message for VideoVisitCreateRequestMessage.
    /// </summary>
    [DataContract]
    public class VideoVisitCreateRequestMessage : TmpBaseRequestMessage
    {
        /// <summary>
        /// Gets or sets the Service Appointment Id.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public Guid AppointmentId { get; set; }

        /// <summary>
        /// List of patients being booked
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public List<Guid> AddedPatients { get; set; }
    }
}