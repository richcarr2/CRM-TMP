using System;

namespace VA.TMP.Integration.Core.Exceptions.EcExceptions
{
    [Serializable]
    public class JwtPostException : Exception
    {
        public JwtPostException()
        {
        }

        public JwtPostException(string message)
            : base(message)
        {
        }

        public JwtPostException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}