using System;

namespace VA.TMP.Integration.Core.Exceptions.LobExceptions
{
    [Serializable]
    public class VvsBusinessRulesException : Exception
    {
        public VvsBusinessRulesException()
        {
        }

        public VvsBusinessRulesException(string message)
            : base(message)
        {
        }

        public VvsBusinessRulesException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}