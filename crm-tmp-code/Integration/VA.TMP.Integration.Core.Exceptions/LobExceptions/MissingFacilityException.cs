using System;

namespace VA.TMP.Integration.Core.Exceptions.LobExceptions
{
    [Serializable]
    public class MissingFacilityException : Exception
    {
        public MissingFacilityException()
        {
        }

        public MissingFacilityException(string message)
            : base(message)
        {
        }

        public MissingFacilityException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}