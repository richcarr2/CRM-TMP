using System;

namespace VA.TMP.Integration.Core.Exceptions.EcExceptions
{
    [Serializable]
    public class HealthSharePostException : Exception
    {
        public HealthSharePostException()
        {
        }

        public HealthSharePostException(string message)
            : base(message)
        {
        }

        public HealthSharePostException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}