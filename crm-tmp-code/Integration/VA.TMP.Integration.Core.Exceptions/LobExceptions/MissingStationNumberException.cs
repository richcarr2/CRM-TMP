using System;

namespace VA.TMP.Integration.Core.Exceptions.LobExceptions
{
    [Serializable]
    public class MissingStationNumberException : Exception
    {
        public MissingStationNumberException()
        {
        }

        public MissingStationNumberException(string message)
            : base(message)
        {
        }

        public MissingStationNumberException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}