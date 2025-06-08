using System;

namespace VA.TMP.Integration.Core.Exceptions.LobExceptions
{
    [Serializable]
    public class MissingIntegrationResultException : Exception
    {
        public MissingIntegrationResultException()
        {
        }

        public MissingIntegrationResultException(string message)
            : base(message)
        {
        }

        public MissingIntegrationResultException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}