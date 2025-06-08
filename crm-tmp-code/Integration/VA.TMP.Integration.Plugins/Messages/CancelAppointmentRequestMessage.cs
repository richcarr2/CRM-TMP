using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VA.TMP.Integration.Plugins.Messages
{
    public class CancelAppointmentRequestMessage : TmpVimtRequestMessage
    {
        /// <summary>
        /// Gets or sets the Service Appointment Id.
        /// </summary>
        public Guid AppointmentId { get; set; }

        /// <summary>
        /// List of patient IDs who are being canceled
        /// </summary>
        public List<Guid> CanceledPatients { get; set; }

        /// <summary>
        /// indiciates if an individual patient is being canceled, or if it is the whole appointment
        /// </summary>
        public bool WholeAppointmentCanceled { get; set; }

        public int IndividualCancelStatus { get; set; }

        public string SAMLToken { get; set; }

        public string PatUserDuz { get; set; }

        public string ProUserDuz { get; set; }

    }
}
