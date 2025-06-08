using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using VA.TMP.Integration.Core;

namespace VA.TMP.Integration.Messages.VideoVisit
{
    /// <summary>
    /// Message for VideoVisitUpdateRequestMessage.
    /// </summary>
    [DataContract]
    public class VideoVisitUpdateRequestMessage : TmpBaseRequestMessage
    {
        /// <summary>
        /// Gets or sets the Service Appointment Id.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public Guid AppointmentId { get; set; }

        /// <summary>
        /// Full list of patients being booked - including previously booked
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public List<Guid> Contacts { get; set; }
    }
}