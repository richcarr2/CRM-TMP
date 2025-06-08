using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using log4net;
using VA.TMP.Integration.Common;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Core.Exceptions.LobExceptions;
using VA.TMP.Integration.Rest.Interface;
using VA.TMP.Integration.Token.Interface;

namespace VA.TMP.Integration.Rest
{
    public class ServicePost : IServicePost
    {
        private readonly ILog _logger;
        private readonly ITokenService _tokenService;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public ServicePost(ILog logger, ITokenService tokenService)
        {
            _logger = logger;
            _tokenService = tokenService;
        }

        public async Task<R> PostToEc<T, R>(string ecName, string uri, Settings settings, T payload)
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            var shouldSendToken = Convert.ToBoolean(settings.Items.First(x => x.Key == "ShouldSendToken").Value);
            var ecBaseUri = settings.Items.First(x => x.Key == "EcBaseUri").Value;
            var subscriptionId = settings.Items.First(x => x.Key == "SubscriptionId").Value;
            var isProdApi = Convert.ToBoolean(settings.Items.First(x => x.Key == "IsProdApi").Value);
            var subscriptionIdEast = settings.Items.First(x => x.Key == "SubscriptionIdEast").Value;
            var subscriptionIdSouth = settings.Items.First(x => x.Key == "SubscriptionIdSouth").Value;

            _logger.Info($"Posting to {ecBaseUri}{uri}");
            _logger.Info($"Payload: {Serialization.DataContractSerialize(payload)}");

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(ecBaseUri);

                if (isProdApi)
                {
                    client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key-E", subscriptionIdEast);
                    client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key-S", subscriptionIdSouth);
                }
                else
                {
                    client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionId);
                }

                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.ConnectionClose = true;

                if (shouldSendToken)
                {
                    _logger.Info("Getting Token");
                    var token = await _tokenService.GetToken(ecName).ConfigureAwait(false);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                HttpResponseMessage response;

                using (var memStream = Serialization.ObjectToStream(payload))
                using (var sc = new StreamContent(memStream))
                {
                    sc.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    response = await client.PostAsync(uri, sc).ConfigureAwait(false);
                }

                if (!response.IsSuccessStatusCode)
                {
                    _logger.Error($"Failed posting to {ecName}. Reason : {response.StatusCode}");
                    throw new RestPostException($"Failed posting to {ecName}. Reason : {response.StatusCode}");
                }

                return await response.Content.ReadAsAsync<R>().ConfigureAwait(false);
            }
        }
    }
}