using System;
using System.Collections.Generic;
using System.Net.Http;
using MCSUtilities2011;
using VA.TMP.Integration.Common;
using VA.TMP.Integration.Plugins.Messages.Token;

namespace VA.TMP.Integration.Plugins.Helpers
{
    /// <summary>
    /// Class to get a token from Azure AD using REST API.
    /// </summary>
    public class AzureTokenHelper
    {
        /// <summary>
        /// Get Azure Token from Azure AD.
        /// </summary>
        /// <param name="resource">AppId URL of App Registration.</param>
        /// <param name="appId">AppId of App Registration.</param>
        /// <param name="secret">Secret of App Registration.</param>
        /// <param name="authority">Login Authority. Should be https://login.microsoftonline.us/ for this particular call.</param>
        /// <param name="tenantId">TenantId which is a GUID string.</param>
        /// <param name="logger">Logger.</param>
        /// <returns>Azure Access Token.</returns>
        public static string GetAzureToken(string resource, string appId, string secret, string authority, string tenantId, MCSLogger logger = null)
        {
            using (var httpClient = new HttpClient())
            {
                var formContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("resource", resource),
                    new KeyValuePair<string, string>("client_id", appId),
                    new KeyValuePair<string, string>("client_secret", secret),
                    new KeyValuePair<string, string>("grant_type", "client_credentials")
                });

                var uri = $"{authority}{tenantId}/oauth2/token";

                LogMessage(logger, "4-Before Post to Get Token");
                var response = httpClient.PostAsync(uri, formContent).ConfigureAwait(false).GetAwaiter().GetResult();
                LogMessage(logger, "5-After Post to Get Token");

                if (!response.IsSuccessStatusCode)
                {
                    LogMessage(logger, "Error: Unable to retrieve token to authenticate with LOB");
                    throw new Exception("Error: Unable to retrieve token to authenticate with LOB");
                }

                LogMessage(logger, "6-Before Token Deserialize");
                var token = Serialization.DeserializeTokenResponse<TokenResponse>(response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult());
                LogMessage(logger, "7-After Token Deserialize");

                return token.access_token;
            }
        }

        /// <summary>
        /// Provide Logging in Helper method.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="message">Message.</param>
        private static void LogMessage(MCSLogger logger, string message)
        {
            if (logger != null) logger.WriteDebugMessage($"TOKEN: {message}");
        }
    }
}
