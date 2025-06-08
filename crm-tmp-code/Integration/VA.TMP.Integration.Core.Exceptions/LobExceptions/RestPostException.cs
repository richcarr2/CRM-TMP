using System;

namespace VA.TMP.Integration.Core.Exceptions.LobExceptions
{
    [Serializable]
    public class RestPostException : Exception
    {
        public RestPostException()
        {
        }

        public RestPostException(string message)
            : base(message)
        {
        }

        public RestPostException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}