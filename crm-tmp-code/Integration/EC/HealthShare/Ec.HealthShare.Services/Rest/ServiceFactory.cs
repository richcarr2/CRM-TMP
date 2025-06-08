using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using Ec.HealthShare.Messages;
using log4net;
using Newtonsoft.Json;
using VA.TMP.Integration.Certificate.Interface;
using VA.TMP.Integration.Common;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Core.Exceptions.EcExceptions;

namespace Ec.HealthShare.Services.Rest
{
    public class ServiceFactory : IServiceFactory
    {
        private const string CacheKey = "_IHS";

        private readonly ILog _logger;
        private readonly Settings _settings;
        private readonly IKeyVaultCert _keyVaultCert;

        private string BaseUri => _settings.Items.First(x => x.Key == "BaseUri").Value;

        private string GetConsultsUri => _settings.Items.First(x => x.Key == "GetConsultsUri").Value;

        private string MakeCancelOutboundUri => _settings.Items.First(x => x.Key == "MakeCancelOutboundUri").Value;

        private string RequiresClientCertificate => _settings.Items.First(x => x.Key == "RequiresClientCertificate").Value;

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="logger"></param>
        public ServiceFactory(ILog logger, Settings settings, IKeyVaultCert keyVaultCert)
        {
            _logger = logger;
            _settings = settings;
            _keyVaultCert = keyVaultCert;
        }

        public EcHealthShareGetConsultsResponse GetConsults(EcHealthShareGetConsultsRequest request)
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            using (var client = GetHttpClient())
            {
                var queryString = string.Format(GetConsultsUri, request.UniqueId, request.PatientDfn, request.PatientIcn, request.StationNumber);

                _logger.Info($"Get Consults URL for {request.Side} station: {queryString}");

                client.BaseAddress = new Uri(BaseUri);

                var stopWatch = Stopwatch.StartNew();

                var response = client.GetAsync(queryString).ConfigureAwait(false).GetAwaiter().GetResult();

                stopWatch.Stop();
                _logger.Info($"The call to HealthShare GetConsults took {stopWatch.ElapsedMilliseconds} ms");

                var result = response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();

                _logger.Info($"Get Consults Response Content: {result}");

                if (string.IsNullOrEmpty(result)) throw new HealthSharePostException("Results from GetConsults call are null or empty");

                var consults = JsonConvert.DeserializeObject<EcHealthShareGetConsultsResponse>(result);

                _logger.Info($"Get Consults: ControlId: {consults.ControlId}, Institution: {consults.Institution}");

                return consults;
            }
        }

        public EcHealthShareMakeCancelOutboundResponseMessage MakeCancelOutbound(EcHealthShareMakeCancelOutboundRequestMessage request)
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            using (var client = GetHttpClient())
            {
                client.BaseAddress = new Uri(BaseUri);

                _logger.Info($"Posting to {BaseUri}{MakeCancelOutboundUri}");
                _logger.Info($"Payload: {Serialization.DataContractSerialize(request)}");

                var stopWatch = Stopwatch.StartNew();

                var response = client.PostAsync(MakeCancelOutboundUri, request, new JsonMediaTypeFormatter { UseDataContractJsonSerializer = true }).ConfigureAwait(false).GetAwaiter().GetResult();

                stopWatch.Stop();
                _logger.Info($"The call to HealthShare Make Cancel Outbound took {stopWatch.ElapsedMilliseconds} ms");

                if (response?.Content != null)
                {
                    var result = response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                    _logger.Info($"Response Content: {result}");
                }

                return response != null && !response.IsSuccessStatusCode
                    ? new EcHealthShareMakeCancelOutboundResponseMessage { ExceptionOccured = true, ExceptionMessage = $"POST Make Cancel to HealthShare failed with status {response.StatusCode}" }
                    : new EcHealthShareMakeCancelOutboundResponseMessage { ExceptionOccured = false, ExceptionMessage = string.Empty };
            }
        }

        private HttpClient GetHttpClient()
        {
            var useCertificate = Convert.ToBoolean(RequiresClientCertificate);

            if (useCertificate)
            {
                var certificate = _keyVaultCert.GetKeyVaultCertificate(CacheKey).ConfigureAwait(false).GetAwaiter().GetResult();
                var clientHandler = new WebRequestHandler();
                clientHandler.ClientCertificates.Add(certificate);

                return new HttpClient(clientHandler);
            }

            return new HttpClient();
        }
    }
}
