using System;

namespace VA.TMP.Integration.Core.Exceptions.LobExceptions
{
    [Serializable]
    public class InvalidDurationException : Exception
    {
        public InvalidDurationException()
        {
        }

        public InvalidDurationException(string message)
            : base(message)
        {
        }

        public InvalidDurationException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}