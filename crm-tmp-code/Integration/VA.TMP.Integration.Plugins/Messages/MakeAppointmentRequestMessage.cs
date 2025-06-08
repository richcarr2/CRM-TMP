using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VA.TMP.Integration.Plugins.Messages
{
    public class MakeAppointmentRequestMessage : TmpVimtRequestMessage
    {
        /// <summary>
        /// Gets or sets the Service Appointment Id.
        /// </summary>
        public Guid AppointmentId { get; set; }

        /// <summary>
        /// List of patients being booked
        /// </summary>
        public List<Guid> AddedPatients { get; set; }

        public string SAMLToken { get; set; }

        public string PatUserDuz { get; set; }

        public string ProUserDuz { get; set; }

    }
}
