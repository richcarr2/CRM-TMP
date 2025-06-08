using System;

namespace VA.TMP.Integration.Core.Exceptions.LobExceptions
{
    [Serializable]
    public class StartTimeMappingException : Exception
    {
        public StartTimeMappingException()
        {
        }

        public StartTimeMappingException(string message)
            : base(message)
        {
        }

        public StartTimeMappingException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}