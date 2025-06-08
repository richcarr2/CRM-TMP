using System;

namespace VA.TMP.Integration.Core.Exceptions.EcExceptions
{
    [Serializable]
    public class VvsValidationException : Exception
    {
        public VvsValidationException()
        {
        }

        public VvsValidationException(string message)
            : base(message)
        {
        }

        public VvsValidationException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}