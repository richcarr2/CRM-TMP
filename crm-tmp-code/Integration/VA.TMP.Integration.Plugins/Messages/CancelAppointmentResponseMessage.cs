using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VA.TMP.Integration.Plugins.Messages
{
    public class CancelAppointmentResponseMessage : TmpVimtResponseMessage
    {
        public string SAMLToken { get; set; }

        public VistaAppointment PatVistaAppointment { get; set; }

        public VistaAppointment ProVistaAppointment { get; set; }
    }
}
