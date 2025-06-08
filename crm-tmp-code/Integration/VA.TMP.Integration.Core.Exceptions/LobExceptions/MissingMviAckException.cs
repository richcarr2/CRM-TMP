using System;

namespace VA.TMP.Integration.Core.Exceptions.LobExceptions
{
    [Serializable]
    public class MissingMviAckException : Exception
    {
        public MissingMviAckException()
        {
        }

        public MissingMviAckException(string message)
            : base(message)
        {
        }

        public MissingMviAckException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}