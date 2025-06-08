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
    public class CreateController : ApiController
    {
        private readonly ILog _logger;
        private readonly Settings _settings;
        private readonly IServiceFactory _serviceFactory;
        private readonly TelemetryClient _telemetryClient;

        public CreateController(ILog logger, Settings settings, IServiceFactory serviceFactory, TelemetryClient telemetryClient)
        {
            _logger = logger;
            _settings = settings;
            _serviceFactory = serviceFactory;
            _telemetryClient = telemetryClient;
        }

        [SwaggerOperation("Create")]
        public HttpResponseMessage Post([FromBody]EcTmpCreateAppointmentRequest requestMessage)
        {
            try
            {
                if (requestMessage == null)
                {
                    _logger.Error("Error calling VVS Create. The request cannot be null");
                    var error = new EcTmpCreateAppointmentResponse { ExceptionOccured = true, ExceptionMessage = "Error calling VVS Create. The request cannot be null" };
                    return Request.CreateResponse(HttpStatusCode.Created, error);
                }

                var response = new EcTmpCreateAppointmentMessageHandler(_logger, _settings, _serviceFactory).HandleRequestResponse(requestMessage);

                return Request.CreateResponse(HttpStatusCode.Created, response);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.Created, CreateResponse(ex));
            }
        }

        private EcTmpCreateAppointmentResponse CreateResponse<T>(T ex) where T : Exception
        {
            _logger.Error($"Error VVS Create: {ex.Message}", ex);
            _telemetryClient.TrackException(ex, new Dictionary<string, string>() { { "Method", "VVS Create" } });
            return new EcTmpCreateAppointmentResponse { ExceptionOccured = true, ExceptionMessage = $"ERROR VVS Create: {Strings.BuildErrorMessage(ex)}" };
        }
    }
}
