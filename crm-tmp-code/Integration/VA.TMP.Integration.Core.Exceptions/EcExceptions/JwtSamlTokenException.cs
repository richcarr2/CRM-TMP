using System;

namespace VA.TMP.Integration.Core.Exceptions.EcExceptions
{
    [Serializable]
    public class JwtSamlTokenException : Exception
    {
        public JwtSamlTokenException()
        {
        }

        public JwtSamlTokenException(string message)
            : base(message)
        {
        }

        public JwtSamlTokenException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}