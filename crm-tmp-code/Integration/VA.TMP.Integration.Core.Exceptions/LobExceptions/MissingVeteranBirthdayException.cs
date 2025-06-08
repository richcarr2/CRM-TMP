using System;

namespace VA.TMP.Integration.Core.Exceptions.LobExceptions
{
    [Serializable]
    public class MissingVeteranBirthdayException : Exception
    {
        public MissingVeteranBirthdayException()
        {
        }

        public MissingVeteranBirthdayException(string message)
            : base(message)
        {
        }

        public MissingVeteranBirthdayException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}