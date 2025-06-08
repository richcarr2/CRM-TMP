using System;

namespace VA.TMP.Integration.Core.Exceptions.EcExceptions
{
    [Serializable]
    public class VvsTokenResponseException : Exception
    {
        public VvsTokenResponseException()
        {
        }

        public VvsTokenResponseException(string message)
            : base(message)
        {
        }

        public VvsTokenResponseException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}