using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Ec.VideoVisit.MessageHandler;
using Ec.VideoVisit.Messages;
using Ec.VideoVisit.Services.Rest;
using log4net;
using Microsoft.ApplicationInsights;
using Swashbuckle.Swagger.Annotations;
using VA.TMP.Integration.Common;
using VA.TMP.Integration.Core;

namespace Ec.VideoVisit.Api.Controllers
{
    public class CancelController : ApiController
    {
        private readonly ILog _logger;
        private readonly Settings _settings;
        private readonly IServiceFactory _serviceFactory;
        private readonly TelemetryClient _telemetryClient;

        public CancelController(ILog logger, Settings settings, IServiceFactory serviceFactory, TelemetryClient telemetryClient)
        {
            _logger = logger;
            _settings = settings;
            _serviceFactory = serviceFactory;
            _telemetryClient = telemetryClient;
        }

        [SwaggerOperation("Cancel")]
        public HttpResponseMessage Post([FromBody]EcTmpCancelAppointmentRequest requestMessage)
        {
            try
            {
                if (requestMessage == null)
                {
                    _logger.Error("Error calling VVS Cancel. The request cannot be null");
                    var error = new EcTmpCancelAppointmentResponse { ExceptionOccured = true, ExceptionMessage = "Error calling VVS Cancel. The request cannot be null" };
                    return Request.CreateResponse(HttpStatusCode.Created, error);
                }

                var response = new EcTmpCancelAppointmentMeetingMessageHandler(_logger, _settings, _serviceFactory).HandleRequestResponse(requestMessage);

                return Request.CreateResponse(HttpStatusCode.Created, response);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.Created, CreateResponse(ex));
            }
        }

        private EcTmpCancelAppointmentResponse CreateResponse<T>(T ex) where T : Exception
        {
            _logger.Error($"Error VVS Cancel: {ex.Message}", ex);
            _telemetryClient.TrackException(ex, new Dictionary<string, string>() { { "Method", "VVS Cancel" } });
            return new EcTmpCancelAppointmentResponse { ExceptionOccured = true, ExceptionMessage = $"ERROR VVS Cancel: {Strings.BuildErrorMessage(ex)}" };
        }
    }
}
