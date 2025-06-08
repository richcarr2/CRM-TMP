using System;

namespace VA.TMP.Integration.Core.Exceptions.LobExceptions
{
    [Serializable]
    public class MissingResourceException : Exception
    {
        public MissingResourceException()
        {
        }

        public MissingResourceException(string message)
            : base(message)
        {
        }

        public MissingResourceException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}