namespace VA.TMP.Integration.Plugins.Messages.Token
{
    /// <summary>
    /// Class to represent an Azure AD token.
    /// </summary>
    public class TokenResponse
    {
        /// <summary>
        /// Gets or sets Token Type.
        /// </summary>
        public string token_type { get; set; }

        /// <summary>
        /// Gets or sets when token expires in.
        /// </summary>
        public string expires_in { get; set; }

        /// <summary>
        /// Gets or sets ext when token expires in.
        /// </summary>
        public string ext_expires_in { get; set; }

        /// <summary>
        /// Get or sets when token expires on.
        /// </summary>
        public string expires_on { get; set; }

        /// <summary>
        /// Gets or sets cutoff of token expiration.
        /// </summary>
        public string not_before { get; set; }

        /// <summary>
        /// Gets or sets Resource of token.
        /// </summary>
        public string resource { get; set; }

        /// <summary>
        /// Gets or sets the Access Token.
        /// </summary>
        public string access_token { get; set; }
    }
}