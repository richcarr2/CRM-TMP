using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using AI = MCS.ApplicationInsights;
using MCSUtilities2011;
using VA.TMP.Integration.Common;

namespace VA.TMP.Integration.Plugins.Helpers
{
    /// <summary>
    /// Class to help the plugin POST REST messages.
    /// </summary>
    public class RestPoster
    {
        /// <summary>
        /// Post REST messages from plugin.
        /// </summary>
        /// <typeparam name="T">Type of request.</typeparam>
        /// <typeparam name="R">Type of response.</typeparam>
        /// <param name="lobName"></param>
        /// <param name="baseUri"></param>
        /// <param name="uri"></param>
        /// <param name="payload"></param>
        /// <param name="resource">AppId URL of App Registration.</param>
        /// <param name="appId">AppId of App Registration.</param>
        /// <param name="secret">Secret of App Registration.</param>
        /// <param name="authority">Login Authority. Should be https://login.microsoftonline.us/ for this particular call.</param>
        /// <param name="tenantId">TenantId which is a GUID string.</param>
        /// <param name="subscriptionId">Subscription Id.</param>
        /// <param name="lag">Lag</param>
        /// <returns>Response message.</returns>
        public static R Post<T, R>(string lobName, string baseUri, string uri, T payload, string resource, string appId,
            string secret, string authority, string tenantId, string subscriptionId, bool isProdApi, string subscriptionIdEast, string subscriptionIdSouth, out int lag)
        {
            MCSLogger logger = null;
            return Post<T, R>(lobName, baseUri, uri, payload, resource, appId, secret, authority, tenantId, subscriptionId, isProdApi, subscriptionIdEast, subscriptionIdSouth, out lag, logger);
        }

        /// <summary>
        /// Post REST messages from plugin.
        /// </summary>
        /// <typeparam name="T">Type of request.</typeparam>
        /// <typeparam name="R">Type of response.</typeparam>
        /// <param name="lobName"></param>
        /// <param name="baseUri"></param>
        /// <param name="uri"></param>
        /// <param name="payload"></param>
        /// <param name="resource">AppId URL of App Registration.</param>
        /// <param name="appId">AppId of App Registration.</param>
        /// <param name="secret">Secret of App Registration.</param>
        /// <param name="authority">Login Authority. Should be https://login.microsoftonline.us/ for this particular call.</param>
        /// <param name="tenantId">TenantId which is a GUID string.</param>
        /// <param name="subscriptionId">Subscription Id.</param>
        /// <param name="lag">Lag</param>
        /// <returns>Response message.</returns>
        public static R Post<T, R>(string lobName, string baseUri, string uri, T payload, string resource, string appId,
            string secret, string authority, string tenantId, string subscriptionId, bool isProdApi, string subscriptionIdEast, string subscriptionIdSouth, out int lag, 
            MCSLogger logger = null)
        {
          
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            LogMessage(logger, "3-Before Token");
            var token = AzureTokenHelper.GetAzureToken(resource, appId, secret, authority, tenantId, logger);
            LogMessage(logger, "8-After Token");

            if (string.IsNullOrEmpty(token))
            {
                LogMessage(logger, "Error: Access Token for LOB cannot be null or empty");
                throw new Exception("Error: Access Token for LOB cannot be null or empty");
            }

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(baseUri);
                LogMessage(logger, $"VVS Url : {baseUri}");

                if (isProdApi)
                {
                    client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key-E", subscriptionIdEast);
                    client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key-S", subscriptionIdSouth);
                }
                else
                {
                    client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionId);
                }

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.ConnectionClose = true;

                HttpResponseMessage response;

                using (var memStream = Serialization.ObjectToStream(payload))
                using (var sc = new StreamContent(memStream))
                {
                    sc.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    LogMessage(logger, "9-Before Post to LOB");
                    LogMessage(logger, $"9.1-Is HttpClient null? {client == null}");
                    response = client.PostAsync(uri, sc).ConfigureAwait(false).GetAwaiter().GetResult();
                    LogMessage(logger, "10-After Post to LOB");
                }

                var stopWatch = new Stopwatch();

                if (!response.IsSuccessStatusCode)
                {
                    LogMessage(logger, $"Error: Failed posting to {lobName}. Reason : {response.StatusCode}");
                    throw new Exception($"Error: Failed posting to {lobName}. Reason : {response.StatusCode}");
                }

                LogMessage(logger, "11-Before Getting Content and Deserialize");
                var stringResponse = response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();

              //("Sending Create Booking to Vista");            
                LogMessage(logger, $"Rest Response {stringResponse}");
                var returnValue = Serialization.DeserializeResponse<R>(stringResponse);
                LogMessage(logger, "12-After Getting Content and Deserialize");

                stopWatch.Stop();
                lag = (int)stopWatch.ElapsedMilliseconds;

                return returnValue;
            }
        }

        /// <summary>
        /// Post REST messages from plugin.
        /// </summary>
        /// <typeparam name="T">Type of request.</typeparam>
        /// <typeparam name="R">Type of response.</typeparam>
        /// <param name="lobName"></param>
        /// <param name="baseUri"></param>
        /// <param name="uri"></param>
        /// <param name="payload"></param>
        /// <param name="resource">AppId URL of App Registration.</param>
        /// <param name="appId">AppId of App Registration.</param>
        /// <param name="secret">Secret of App Registration.</param>
        /// <param name="authority">Login Authority. Should be https://login.microsoftonline.us/ for this particular call.</param>
        /// <param name="tenantId">TenantId which is a GUID string.</param>
        /// <param name="subscriptionId">Subscription Id.</param>
        /// <param name="lag">Lag</param>
        /// <returns>Response message.</returns>
        public static R Post<T, R>(string lobName, string baseUri, string uri, T payload, string resource, string appId,
            string secret, string authority, string tenantId, string subscriptionId, bool isProdApi, string subscriptionIdEast, string subscriptionIdSouth, out int lag, 
            AI.PluginLogger logger = null)
        {
          
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            LogMessage(logger, "3-Before Token");
            var token = AzureTokenHelper.GetAzureToken(resource, appId, secret, authority, tenantId, logger);
            LogMessage(logger, "8-After Token");

            if (string.IsNullOrEmpty(token))
            {
                LogMessage(logger, "Error: Access Token for LOB cannot be null or empty");
                throw new Exception("Error: Access Token for LOB cannot be null or empty");
            }

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(baseUri);
                LogMessage(logger, $"VVS Url : {baseUri}");

                if (isProdApi)
                {
                    client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key-E", subscriptionIdEast);
                    client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key-S", subscriptionIdSouth);
                }
                else
                {
                    client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionId);
                }

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.ConnectionClose = true;

                HttpResponseMessage response;

                using (var memStream = Serialization.ObjectToStream(payload))
                using (var sc = new StreamContent(memStream))
                {
                    sc.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    LogMessage(logger, "9-Before Post to LOB");
                    LogMessage(logger, $"9.1-Is HttpClient null? {client == null}");
                    response = client.PostAsync(uri, sc).ConfigureAwait(false).GetAwaiter().GetResult();
                    LogMessage(logger, "10-After Post to LOB");
                }

                var stopWatch = new Stopwatch();

                if (!response.IsSuccessStatusCode)
                {
                    LogMessage(logger, $"Error: Failed posting to {lobName}. Reason : {response.StatusCode}");
                    throw new Exception($"Error: Failed posting to {lobName}. Reason : {response.StatusCode}");
                }

                LogMessage(logger, "11-Before Getting Content and Deserialize");
                var stringResponse = response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();

              //("Sending Create Booking to Vista");            
                LogMessage(logger, $"Rest Response {stringResponse}");
                var returnValue = Serialization.DeserializeResponse<R>(stringResponse);
                LogMessage(logger, "12-After Getting Content and Deserialize");

                stopWatch.Stop();
                lag = (int)stopWatch.ElapsedMilliseconds;

                return returnValue;
            }
        }

        /// <summary>
        /// Provide Logging in Helper method.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="message">Message.</param>
        private static void LogMessage(MCSLogger logger, string message)
        {
            if (logger != null) logger.WriteDebugMessage($"REST: {message}");
        }

        /// <summary>
        /// Provide Logging in Helper method.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="message">Message.</param>
        private static void LogMessage(AI.PluginLogger logger, string message)
        {
            if (logger != null) logger.Trace($"REST: {message}", AI.LogLevel.Debug);
        }
    }
}
