using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Ec.JsonWebToken.Messages;
using Ec.VideoVisit.Messages;
using Ec.VideoVisit.Services.XSD;
using log4net;
using VA.TMP.Integration.Common;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Core.Exceptions.EcExceptions;
using VA.TMP.Integration.Token.Interface;

namespace Ec.VideoVisit.Services.Rest
{
    public class ServiceFactory : IServiceFactory
    {
        private readonly ILog _logger;
        private readonly Settings _settings;
        private readonly ITokenService _tokenService;

        private string BaseUri => _settings.Items.First(x => x.Key == "BaseUri").Value;

        private string RefererUri => _settings.Items.First(x => x.Key == "RefererUri").Value;

        private string CreateUri => _settings.Items.First(x => x.Key == "CreateUri").Value;

        private string GetLoanedDevicesUri => _settings.Items.First(x => x.Key == "GetLoanedDevicesUri").Value;

        private string UpdateUri => _settings.Items.First(x => x.Key == "UpdateUri").Value;

        private string CancelUri => _settings.Items.First(x => x.Key == "CancelUri").Value;

        private string JwtBaseUri => _settings.Items.First(x => x.Key == "JwtBaseUri").Value;

        private string JwtUri => _settings.Items.First(x => x.Key == "JwtUri").Value;

        private string SubscriptionId => _settings.Items.First(x => x.Key == "SubscriptionId").Value;

        private bool IsProdApi => Convert.ToBoolean(_settings.Items.First(x => x.Key == "IsProdApi").Value);

        private string SubscriptionIdEast => _settings.Items.First(x => x.Key == "SubscriptionIdEast").Value;

        private string SubscriptionIdSouth => _settings.Items.First(x => x.Key == "SubscriptionIdSouth").Value;

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="logger"></param>
        public ServiceFactory(ILog logger, Settings settings, ITokenService tokenService)
        {
            _logger = logger;
            _settings = settings;
            _tokenService = tokenService;
        }

        public writeResults CreateAppointment(appointment payload, string samlToken)
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(BaseUri);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
                client.DefaultRequestHeaders.Referrer = new Uri(RefererUri);

                var tokenRequest = new EcJwtEncryptTokenRequest { SamlToken = samlToken };
                var tokenResponse = PostToJwtService(tokenRequest).ConfigureAwait(false).GetAwaiter().GetResult();
                if (tokenResponse != null && tokenResponse.ExceptionOccured)
                {
                    _logger.Error(string.Format("VVS CreateAppointment. Error getting encrypted JWT token: {0}", tokenResponse.ExceptionMessage));
                    throw new VvsTokenResponseException(tokenResponse.ExceptionMessage);
                }

                if (tokenResponse == null)
                {
                    _logger.Error("VVS CreateAppointment. JWT token response is null");
                    throw new VvsTokenResponseException("VVS CreateAppointment. JWT token response is null");
                }

                _logger.Info($"Posting to {BaseUri}{CreateUri}");
                _logger.Info($"Payload: {Serialization.XmlSerializeInstance(payload)}");

                client.DefaultRequestHeaders.Add("X-VAMF-JWT", tokenResponse.EncryptedJwtToken);
                var response = client.PostAsync(CreateUri, payload, new XmlMediaTypeFormatter { UseXmlSerializer = true }).ConfigureAwait(false).GetAwaiter().GetResult();
                _logger.Info(string.Format("VVS CreateAppointment Status: {0} Reason: {1}", response.StatusCode, response.ReasonPhrase));
               
                return HandleHttpResponse(response, "VVS CreateAppointment");
            }
        }

        public EcTmpGetLoanedDevicesResponse GetLoanedDevices(EcTmpGetLoanedDevicesRequest request)
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(BaseUri);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Referrer = new Uri(RefererUri);

                var tokenRequest = new EcJwtEncryptTokenRequest { SamlToken = request.SamlToken };
                var tokenResponse = PostToJwtService(tokenRequest).ConfigureAwait(false).GetAwaiter().GetResult();
                if (tokenResponse != null && tokenResponse.ExceptionOccured)
                {
                    _logger.Error(string.Format("VVS GetLoanedDevices. Error getting encrypted JWT token: {0}", tokenResponse.ExceptionMessage));
                    throw new VvsTokenResponseException(tokenResponse.ExceptionMessage);
                }

                if (tokenResponse == null)
                {
                    _logger.Error("VVS GetLoanedDevices. JWT token response is null");
                    throw new VvsTokenResponseException("VVS GetLoanedDevices. JWT token response is null");
                }

                _logger.Info($"Posting to {BaseUri}{string.Format(GetLoanedDevicesUri, request.ICN)}");
                _logger.Info($"request: {Serialization.XmlSerializeInstance(request)}");

                client.DefaultRequestHeaders.Add("X-VAMF-JWT", tokenResponse.EncryptedJwtToken);
                var response = client.GetAsync(string.Format(GetLoanedDevicesUri, request.ICN)).ConfigureAwait(false).GetAwaiter().GetResult();
                _logger.Info(string.Format("VVS GetLoanedDevices Status: {0} Reason: {1}", response.StatusCode, response.ReasonPhrase));

                return HandleHttpResponse<EcTmpGetLoanedDevicesResponse>(response, "VVS GetLoanedDevices");
            }
        }

        public writeResults UpdateAppointment(appointment payload, string samlToken)
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(BaseUri);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
                client.DefaultRequestHeaders.Referrer = new Uri(RefererUri);

                var tokenRequest = new EcJwtEncryptTokenRequest { SamlToken = samlToken };
                var tokenResponse = PostToJwtService(tokenRequest).ConfigureAwait(false).GetAwaiter().GetResult();
                if (tokenResponse != null && tokenResponse.ExceptionOccured)
                {
                    _logger.Error(string.Format("VVS UpdateAppointment. Error getting encrypted JWT token: {0}", tokenResponse.ExceptionMessage));
                    throw new VvsTokenResponseException(tokenResponse.ExceptionMessage);
                }

                if (tokenResponse == null)
                {
                    _logger.Error("VVS UpdateAppointment. JWT token response is null");
                    throw new VvsTokenResponseException("VVS UpdateAppointment. JWT token response is null");
                }

                var updateUri = string.Format(UpdateUri, payload.id);

                _logger.Info($"Posting to {BaseUri}{updateUri}");
                _logger.Info($"Payload: {Serialization.XmlSerializeInstance(payload)}");

                client.DefaultRequestHeaders.Add("X-VAMF-JWT", tokenResponse.EncryptedJwtToken);
                var response = client.PutAsync(updateUri, payload, new XmlMediaTypeFormatter { UseXmlSerializer = true }).ConfigureAwait(false).GetAwaiter().GetResult();
                _logger.Info(string.Format("VVS UpdateAppointment Status: {0} Reason: {1}", response.StatusCode, response.ReasonPhrase));

                return HandleHttpResponse(response, "VVS UpdateAppointment");
            }
        }

        public writeResults CancelAppointment(cancelAppointmentRequest payload, string samlToken)
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(BaseUri);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
                client.DefaultRequestHeaders.Referrer = new Uri(RefererUri);

                var tokenRequest = new EcJwtEncryptTokenRequest { SamlToken = samlToken };
                var tokenResponse = PostToJwtService(tokenRequest).ConfigureAwait(false).GetAwaiter().GetResult();
                if (tokenResponse != null && tokenResponse.ExceptionOccured)
                {
                    _logger.Error(string.Format("VVS CancelAppointment. Error getting encrypted JWT token: {0}", tokenResponse.ExceptionMessage));
                    throw new VvsTokenResponseException(tokenResponse.ExceptionMessage);
                }

                if (tokenResponse == null)
                {
                    _logger.Error("VVS CancelAppointment. JWT token response is null");
                    throw new VvsTokenResponseException("VVS CancelAppointment. JWT token response is null");
                }

                var cancelUri = string.Format(CancelUri, payload.id);

                _logger.Info($"Posting to {BaseUri}{CancelUri}");
                _logger.Info($"Payload: {Serialization.XmlSerializeInstance(payload)}");

                client.DefaultRequestHeaders.Add("X-VAMF-JWT", tokenResponse.EncryptedJwtToken);
                var response = client.PutAsync(cancelUri, payload, new XmlMediaTypeFormatter { UseXmlSerializer = true }).ConfigureAwait(false).GetAwaiter().GetResult();
                _logger.Error(string.Format("VVS CancelAppointment Status: {0} Reason: {1}", response.StatusCode, response.ReasonPhrase));

                return HandleHttpResponse(response, "VVS CancelAppointment");
            }
        }

        public writeResults HandleHttpResponse(HttpResponseMessage response, string reqType)
        {
            var unknownErrorString = response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();


            if (response.IsSuccessStatusCode)
            {
                _logger.Info(string.Format("{0} was successfully sent", reqType));
                _logger.Error($"Response : {unknownErrorString}");

                if (reqType == "VVS CancelAppointment")
                {
                    var apptResponse = response.Content.ReadAsAsync<appointmentResponse>(new List<MediaTypeFormatter> { new XmlMediaTypeFormatter { UseXmlSerializer = true } }).GetAwaiter().GetResult();
                    return apptResponse.WriteResults;
                }
                else
                {
                    var apptResponse = response.Content.ReadAsAsync<appointment>(new List<MediaTypeFormatter> { new XmlMediaTypeFormatter { UseXmlSerializer = true } }).GetAwaiter().GetResult();
                    return apptResponse.writeResults;
                }
            }

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var errors = response.Content.ReadAsAsync<validationErrors>(new List<MediaTypeFormatter> { new XmlMediaTypeFormatter { UseXmlSerializer = true } }).GetAwaiter().GetResult();
                var errorString = "Validation Error(s):";
                errors.errors.ToList().ForEach(e => errorString += string.Format("\nField Name={0} | Error={1}", e.fieldName, e.errorMessage));

                _logger.Error(string.Format("Validation Errors for {0}. Reason: {1}. The following validation errors were found: {2}", reqType, response.StatusCode, errorString));
                throw new VvsValidationException(string.Format("Validation Errors for {0}. Reason: {1}. The following validation errors were found: {2}", reqType, response.StatusCode, errorString));
            }

           // var unknownErrorString = response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            _logger.Error(string.Format("Unknown Error in {0}. Reason={1} | Unknown Error={2}.", reqType, response.StatusCode, unknownErrorString));
            throw new VvsUnknownWriteResultsException(string.Format("Unknown Error in {0}. Reason={1} | Unknown Error={2}.", reqType, response.StatusCode, unknownErrorString));
        }

        public T HandleHttpResponse<T>(HttpResponseMessage response, string reqType)
        {
            var unknownErrorString = response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();

            if (response.IsSuccessStatusCode)
            {
                _logger.Info(string.Format("{0} was successfully sent", reqType));
                _logger.Info($"Response : {unknownErrorString}");

                var apptResponse = response.Content.ReadAsAsync<T>().GetAwaiter().GetResult();
                return apptResponse;
            }

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var errors = response.Content.ReadAsAsync<validationErrors>(new List<MediaTypeFormatter> { new XmlMediaTypeFormatter { UseXmlSerializer = true } }).GetAwaiter().GetResult();
                var errorString = "Validation Error(s):";
                errors.errors.ToList().ForEach(e => errorString += string.Format("\nField Name={0} | Error={1}", e.fieldName, e.errorMessage));

                _logger.Error(string.Format("Validation Errors for {0}. Reason: {1}. The following validation errors were found: {2}", reqType, response.StatusCode, errorString));
                throw new VvsValidationException(string.Format("Validation Errors for {0}. Reason: {1}. The following validation errors were found: {2}", reqType, response.StatusCode, errorString));
            }

            _logger.Error(string.Format("Unknown Error in {0}. Reason={1} | Unknown Error={2}.", reqType, response.StatusCode, unknownErrorString));
            throw new VvsUnknownWriteResultsException(string.Format("Unknown Error in {0}. Reason={1} | Unknown Error={2}.", reqType, response.StatusCode, unknownErrorString));
        }

        private async Task<EcJwtEncryptTokenResponse> PostToJwtService(EcJwtEncryptTokenRequest request)
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            var token = await _tokenService.GetToken("VVS EC").ConfigureAwait(false);

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(JwtBaseUri);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                if (IsProdApi)
                {
                    client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key-E", SubscriptionIdEast);
                    client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key-S", SubscriptionIdSouth);
                }
                else
                {
                    client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", SubscriptionId);
                }

                _logger.Info($"Posting to {JwtBaseUri}{JwtUri}");

                var response = await client.PostAsXmlAsync(JwtUri, request).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.Error($"Failed posting to JWT Service EC. Reason : {response.StatusCode}");
                    throw new VvsTokenResponseException($"Failed posting to JWT Service EC. Reason : {response.StatusCode}");
                }

                return await response.Content.ReadAsAsync<EcJwtEncryptTokenResponse>();
            }
        }
    }
}
