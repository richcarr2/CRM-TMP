using System.Net;
using System.Text.Json.Serialization;
using VA.TMP.Integration.Messages.TelehealthSpecialtyLocation.Interfaces;

namespace VA.TMP.Integration.Messages.TelehealthSpecialtyLocation
{
    public class TelehealthSpecialtyLocationsResponse : IResponseMessage
    {
        [JsonPropertyName("errorOccurred")]
        public bool ErrorOccurred { get; set; }

        [JsonPropertyName("errorMessage")]
        public string ErrorMessage { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("debugInfo")]
        public string DebugInfo { get; set; }

        public IResponseMessage ValidateResponse()
        {
            if (ErrorOccurred && !Status.Equals("PARTIAL RESULT") && !Status.Equals("FAILED"))
            {
                throw new WebException(ErrorMessage);
            }
            return this;
        }
    }
}
