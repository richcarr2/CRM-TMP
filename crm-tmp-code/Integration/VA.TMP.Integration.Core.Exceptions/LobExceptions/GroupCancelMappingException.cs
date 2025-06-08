using System;

namespace VA.TMP.Integration.Core.Exceptions.LobExceptions
{
    [Serializable]
    public class GroupCancelMappingException : Exception
    {
        public GroupCancelMappingException()
        {
        }

        public GroupCancelMappingException(string message)
            : base(message)
        {
        }

        public GroupCancelMappingException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}