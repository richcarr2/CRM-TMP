using System;

namespace VA.TMP.Integration.Core.Exceptions.LobExceptions
{
    [Serializable]
    public class PatientDfnException : Exception
    {
        public PatientDfnException()
        {
        }

        public PatientDfnException(string message)
            : base(message)
        {
        }

        public PatientDfnException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}