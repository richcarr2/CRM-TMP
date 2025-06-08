using System;

namespace VA.TMP.Integration.Core.Exceptions.LobExceptions
{
    [Serializable]
    public class MissingVistaIntegrationResultException : Exception
    {
        public MissingVistaIntegrationResultException()
        {
        }

        public MissingVistaIntegrationResultException(string message)
            : base(message)
        {
        }

        public MissingVistaIntegrationResultException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}