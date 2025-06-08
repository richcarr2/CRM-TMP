using System;
using System.Collections.Generic;

namespace VA.TMP.Integration.Plugins.Messages
{
    public class MakeGroupAppointmentRequestMessage : TmpVimtRequestMessage
    {
        public Guid AppointmentId { get; set; }

        public List<Guid> AddedPatients { get; set; }

        public string SAMLToken { get; set; }

        public string PatUserDuz { get; set; }

        public string ProUserDuz { get; set; }
    }
}

