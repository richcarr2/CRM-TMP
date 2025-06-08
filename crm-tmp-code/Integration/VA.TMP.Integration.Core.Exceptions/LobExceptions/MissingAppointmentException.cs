using System;

namespace VA.TMP.Integration.Core.Exceptions.LobExceptions
{
    [Serializable]
    public class MissingAppointmentException : Exception
    {
        public MissingAppointmentException()
        {
        }

        public MissingAppointmentException(string message)
            : base(message)
        {
        }

        public MissingAppointmentException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}