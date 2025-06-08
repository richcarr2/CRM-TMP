using System;

namespace VA.TMP.Integration.Core.Exceptions.LobExceptions
{
    [Serializable]
    public class MissingClinicException : Exception
    {
        public MissingClinicException()
        {
        }

        public MissingClinicException(string message)
            : base(message)
        {
        }

        public MissingClinicException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}