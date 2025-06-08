using System;

namespace VA.TMP.Integration.Core.Exceptions.LobExceptions
{
    [Serializable]
    public class MissingIdentifiersException : Exception
    {
        public MissingIdentifiersException()
        {
        }

        public MissingIdentifiersException(string message)
            : base(message)
        {
        }

        public MissingIdentifiersException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}