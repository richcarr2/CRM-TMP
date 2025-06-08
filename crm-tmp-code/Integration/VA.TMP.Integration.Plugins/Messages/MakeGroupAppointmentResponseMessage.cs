using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VA.TMP.Integration.Plugins.Messages
{
    public class MakeGroupAppointmentResponseMessage : TmpVimtResponseMessage
    {
        public VistaAppointment ProVistaAppointment { get; set; }

        public VistaAppointment PatVistaAppointment { get; set; }

    }
}
