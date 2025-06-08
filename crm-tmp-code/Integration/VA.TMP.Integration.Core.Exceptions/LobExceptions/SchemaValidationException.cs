using System;

namespace VA.TMP.Integration.Core.Exceptions.LobExceptions
{
    [Serializable]
    public class SchemaValidationException : Exception
    {
        public SchemaValidationException()
        {
        }

        public SchemaValidationException(string message)
            : base(message)
        {
        }

        public SchemaValidationException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}