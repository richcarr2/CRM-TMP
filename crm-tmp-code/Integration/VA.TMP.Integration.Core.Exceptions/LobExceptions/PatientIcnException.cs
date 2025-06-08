using System;

namespace VA.TMP.Integration.Core.Exceptions.LobExceptions
{
    [Serializable]
    public class PatientIcnException : Exception
    {
        public PatientIcnException()
        {
        }

        public PatientIcnException(string message)
            : base(message)
        {
        }

        public PatientIcnException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
