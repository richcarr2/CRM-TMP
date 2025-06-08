using System;

namespace VA.TMP.Integration.Core.Exceptions.LobExceptions
{
    [Serializable]
    public class VmrNameException : Exception
    {
        public VmrNameException()
        {
        }

        public VmrNameException(string message)
            : base(message)
        {
        }

        public VmrNameException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}