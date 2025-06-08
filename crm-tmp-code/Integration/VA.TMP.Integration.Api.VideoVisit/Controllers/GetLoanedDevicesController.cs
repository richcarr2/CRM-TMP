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
using VA.TMP.Integration.Messages.VideoVisit;
using VA.TMP.Integration.VideoVisit.MessageHandler;

namespace VA.TMP.Integration.Api.VideoVisit.Controllers
{
    public class GetLoanedDevicesController : ApiController
    {
        private readonly ILog _logger;
        private readonly Settings _settings;
        private readonly TelemetryClient _telemetryClient;

        public GetLoanedDevicesController(ILog logger, Settings settings, TelemetryClient telemetryClient)
        {
            _logger = logger;
            _settings = settings;
            _telemetryClient = telemetryClient;
        }

        [SwaggerOperation("GetLoanedDevices")]
        public HttpResponseMessage Post([FromBody]VideoVisitGetLoanedDevicesRequestMessage requestMessage)
        {
            var serializedRequestMessage = string.Empty;

            try
            {
                if (requestMessage == null)
                {
                    _logger.Error("Error calling VVS GetLoanedDevices. The request cannot be null");
                    var error = new VideoVisitGetLoanedDevicesResponseMessage { ExceptionOccured = true, ExceptionMessage = "Error calling VVS GetLoanedDevices. The request cannot be null" };
                    return Request.CreateResponse(HttpStatusCode.Created, error);
                }

                serializedRequestMessage = Serialization.DataContractSerialize(requestMessage);

                var response = new VideoVisitGetLoanedDevicesHandler(_logger, _settings).HandleRequestResponse(requestMessage);

                return Request.CreateResponse(HttpStatusCode.Created, response);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.Created, CreateResponse(ex, serializedRequestMessage));
            }
        }

        private VideoVisitGetLoanedDevicesResponseMessage CreateResponse<T>(T ex, string serializedRequestMessage) where T : Exception
        {
            _logger.Error($"Error VVS GetLoanedDevices: {ex.Message}", ex);
            _telemetryClient.TrackException(ex, new Dictionary<string, string>() { { "Method", "VVS GetLoanedDevices" } });
            return new VideoVisitGetLoanedDevicesResponseMessage
            {
                ExceptionOccured = true,
                ExceptionMessage = $"Error calling VVS GetLoanedDevices: {Strings.BuildErrorMessage(ex)}",
                SerializedInstance = serializedRequestMessage
            };
        }
    }
}
