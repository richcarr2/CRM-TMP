using System;

namespace VA.TMP.Integration.Core.Exceptions.LobExceptions
{
    [Serializable]
    public class MissingLobRequestException : Exception
    {
        public MissingLobRequestException()
        {
        }

        public MissingLobRequestException(string message)
            : base(message)
        {
        }

        public MissingLobRequestException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}