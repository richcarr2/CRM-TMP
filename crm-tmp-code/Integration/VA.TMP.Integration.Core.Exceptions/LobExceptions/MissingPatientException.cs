using System;

namespace VA.TMP.Integration.Core.Exceptions.LobExceptions
{
    [Serializable]
    public class MissingPatientException : Exception
    {
        public MissingPatientException()
        {
        }

        public MissingPatientException(string message)
            : base(message)
        {
        }

        public MissingPatientException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}