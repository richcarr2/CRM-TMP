using System.Text.Json.Serialization;

namespace Ec.JsonWebToken.Messages.Messages
{
    public class EcJwtMobileAuthResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }
        [JsonPropertyName("token_type")]
        public string TokenType { get; set; }
        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
    }
}
