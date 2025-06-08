using System;

namespace VA.TMP.Integration.Core.Exceptions.LobExceptions
{
    [Serializable]
    public class MissingProviderException : Exception
    {
        public MissingProviderException()
        {
        }

        public MissingProviderException(string message)
            : base(message)
        {
        }

        public MissingProviderException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}