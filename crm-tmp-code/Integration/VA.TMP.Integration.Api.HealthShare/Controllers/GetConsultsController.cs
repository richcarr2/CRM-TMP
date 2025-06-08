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
    public class GetConsultsController : ApiController
    {
        private readonly ILog _logger;
        private readonly Settings _settings;
        private readonly TelemetryClient _telemetryClient;

        public GetConsultsController(ILog logger, Settings settings, TelemetryClient telemetryClient)
        {
            _logger = logger;
            _settings = settings;
            _telemetryClient = telemetryClient;
        }

        [SwaggerOperation("GetConsults")]
        public HttpResponseMessage Post([FromBody]TmpHealthShareGetConsultsRequest requestMessage)
        {
            var serializedRequestMessage = string.Empty;

            try
            {
                if (requestMessage == null)
                {
                    _logger.Error("Error calling HealthShare Get Consults. The request cannot be null");
                    var error = new TmpHealthShareGetConsultsResponse { ExceptionOccured = true, ExceptionMessage = "Error calling HealthShare Get Consults. The request cannot be null" };
                    return Request.CreateResponse(HttpStatusCode.Created, error);
                }

                serializedRequestMessage = Serialization.DataContractSerialize(requestMessage);

                var response = new GetConsultsHandler(_logger, _settings).HandleRequestResponse(requestMessage);

                return Request.CreateResponse(HttpStatusCode.Created, response);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.Created, CreateResponse(ex, serializedRequestMessage));
            }
        }

        private TmpHealthShareGetConsultsResponse CreateResponse<T>(T ex, string serializedRequestMessage) where T : Exception
        {
            _logger.Error($"Error HealthShare Get Consults: {ex.Message}", ex);
            _telemetryClient.TrackException(ex, new Dictionary<string, string>() { { "Method", "HealthShare Get Consults" } });
            return new TmpHealthShareGetConsultsResponse
            {
                ExceptionOccured = true,
                ExceptionMessage = $"Error calling HealthShare Get Consults: {Strings.BuildErrorMessage(ex)}",
                SerializedInstance = serializedRequestMessage
            };
        }
    }
}
