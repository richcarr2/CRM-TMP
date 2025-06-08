using System;

namespace VA.TMP.Integration.Core.Exceptions.LobExceptions
{
    [Serializable]
    public class MissingMakeCancelRequest : Exception
    {
        public MissingMakeCancelRequest()
        {
        }

        public MissingMakeCancelRequest(string message)
            : base(message)
        {
        }

        public MissingMakeCancelRequest(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}