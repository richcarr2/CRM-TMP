using System.Text.Json.Serialization;

namespace VA.TMP.Integration.Api.TelehealthSpecialtyLocation.Helpers
{
    public class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }
        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
        [JsonPropertyName("ext_expires_in")]
        public int ExtExpiresIn { get; set; }
        [JsonPropertyName("token_type")]
        public string TokenType { get; set; }
    }
}
