using System;

namespace VA.TMP.Integration.Core.Exceptions.LobExceptions
{
    [Serializable]
    public class MissingViaCredentialsException : Exception
    {
        public MissingViaCredentialsException()
        {
        }

        public MissingViaCredentialsException(string message)
            : base(message)
        {
        }

        public MissingViaCredentialsException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}