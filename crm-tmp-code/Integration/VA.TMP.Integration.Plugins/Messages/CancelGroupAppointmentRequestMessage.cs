using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VA.TMP.Integration.Plugins.Messages
{
    public class CancelGroupAppointmentRequestMessage : TmpVimtRequestMessage
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

        /// <summary>
        /// Used only when one patient of many is removed from the group, then this value needs to come from the UI (therefore needs to pass through the plugin)
        /// </summary>
        public int CancelIndividualStatus { get; set; }

        public string SamlToken { get; set; }

        public string PatUserDuz { get; set; }

        public string ProUserDuz { get; set; }
    }
}
