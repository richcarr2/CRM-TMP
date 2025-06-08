using System;

namespace VA.TMP.Integration.Core.Exceptions.LobExceptions
{
    [Serializable]
    public class SignOnFacilityMappingException : Exception
    {
        public SignOnFacilityMappingException()
        {
        }

        public SignOnFacilityMappingException(string message)
            : base(message)
        {
        }

        public SignOnFacilityMappingException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}