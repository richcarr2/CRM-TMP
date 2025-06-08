using System;

namespace VA.TMP.Integration.Core.Exceptions.LobExceptions
{
    [Serializable]
    public class SamlTokenException : Exception
    {
        public SamlTokenException()
        {
        }

        public SamlTokenException(string message)
            : base(message)
        {
        }

        public SamlTokenException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}