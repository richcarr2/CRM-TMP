using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using log4net;
using Microsoft.ApplicationInsights;
using Swashbuckle.Swagger.Annotations;
using VA.TMP.Integration.Common;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.HealthShare.MessageHandler;
using VA.TMP.Integration.Messages.HealthShare;

namespace VA.TMP.Integration.Api.HealthShare.Controllers
{
    public class MakeCancelOutboundController : ApiController
    {
        private readonly ILog _logger;
        private readonly Settings _settings;
        private readonly TelemetryClient _telemetryClient;

        public MakeCancelOutboundController(ILog logger, Settings settings, TelemetryClient telemetryClient)
        {
            _logger = logger;
            _settings = settings;
            _telemetryClient = telemetryClient;
        }

        [SwaggerOperation("MakeCancelOutbound")]
        public HttpResponseMessage Post([FromBody]TmpHealthShareMakeCancelOutboundRequestMessage requestMessage)
        {
            var serializedRequestMessage = string.Empty;

            try
            {
                if (requestMessage == null)
                {
                    _logger.Error("Error calling HealthShare Make Cancel Outbound. The request cannot be null");
                    var error = new TmpHealthShareMakeCancelOutboundResponseMessage { ExceptionOccured = true, ExceptionMessage = "Error calling HealthShare Make Cancel Outbound. The request cannot be null" };
                    return Request.CreateResponse(HttpStatusCode.Created, error);
                }

                serializedRequestMessage = Serialization.DataContractSerialize(requestMessage);

                var response = new MakeCancelOutboundHandler(_logger, _settings).HandleRequestResponse(requestMessage);

                return Request.CreateResponse(HttpStatusCode.Created, response);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.Created, CreateResponse(ex, serializedRequestMessage));
            }
        }

        private TmpHealthShareMakeCancelOutboundResponseMessage CreateResponse<T>(T ex, string serializedRequestMessage) where T : Exception
        {
            var errorMessage = Strings.BuildErrorMessage(ex);

            _logger.Error($"Error HealthShare MakeCancel Outbound: {errorMessage}", ex);

            _telemetryClient.TrackException(ex, new Dictionary<string, string>() { { "Method", "HealthShare MakeCancel Outbound" } });

            var response = new TmpHealthShareMakeCancelOutboundResponseMessage
            {
                ExceptionOccured = true,
                ExceptionMessage = errorMessage,
                SerializedInstance = serializedRequestMessage
            };

            response.PatientIntegrationResultInformation.Add(
                new PatientIntegrationResultInformation
                {
                    ExceptionOccured = true,
                    ExceptionMessage = errorMessage,
                    VimtRequest = serializedRequestMessage
                });

            return response;
        }
    }
}
