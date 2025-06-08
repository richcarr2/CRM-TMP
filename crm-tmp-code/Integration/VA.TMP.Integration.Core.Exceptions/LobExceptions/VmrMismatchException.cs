using System;

namespace VA.TMP.Integration.Core.Exceptions.LobExceptions
{
    [Serializable]
    public class VmrMismatchException : Exception
    {
        public VmrMismatchException()
        {
        }

        public VmrMismatchException(string message)
            : base(message)
        {
        }

        public VmrMismatchException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}