using System;

namespace VA.TMP.Integration.Core.Exceptions.LobExceptions
{
    [Serializable]
    public class MissingSchedulingPackageException : Exception
    {
        public MissingSchedulingPackageException()
        {
        }

        public MissingSchedulingPackageException(string message)
            : base(message)
        {
        }

        public MissingSchedulingPackageException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}