using System;

namespace VA.TMP.Integration.Core.Exceptions.LobExceptions
{
    [Serializable]
    public class MissingSiteException : Exception
    {
        public MissingSiteException()
        {
        }

        public MissingSiteException(string message)
            : base(message)
        {
        }

        public MissingSiteException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}