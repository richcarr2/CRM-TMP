using System;

namespace VA.TMP.Integration.Core.Exceptions.LobExceptions
{
    [Serializable]
    public class SchedulerNameMappingException : Exception
    {
        public SchedulerNameMappingException()
        {
        }

        public SchedulerNameMappingException(string message)
            : base(message)
        {
        }

        public SchedulerNameMappingException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}