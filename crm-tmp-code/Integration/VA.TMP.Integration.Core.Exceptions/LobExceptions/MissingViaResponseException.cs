using System;

namespace VA.TMP.Integration.Core.Exceptions.LobExceptions
{
    [Serializable]
    public class MissingViaResponseException : Exception
    {
        public MissingViaResponseException()
        {
        }

        public MissingViaResponseException(string message)
            : base(message)
        {
        }

        public MissingViaResponseException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}