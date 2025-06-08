using System;

namespace VA.TMP.Integration.Core.Exceptions.EcExceptions
{
    [Serializable]
    public class VvsUnknownWriteResultsException : Exception
    {
        public VvsUnknownWriteResultsException()
        {
        }

        public VvsUnknownWriteResultsException(string message)
            : base(message)
        {
        }

        public VvsUnknownWriteResultsException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}