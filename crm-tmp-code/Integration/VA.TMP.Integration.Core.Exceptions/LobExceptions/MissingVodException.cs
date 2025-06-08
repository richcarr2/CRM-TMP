using System;

namespace VA.TMP.Integration.Core.Exceptions.LobExceptions
{
    [Serializable]
    public class MissingVodException : Exception
    {
        public MissingVodException()
        {
        }

        public MissingVodException(string message)
            : base(message)
        {
        }

        public MissingVodException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}