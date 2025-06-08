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
    public class UpdateController : ApiController
    {
        private readonly ILog _logger;
        private readonly Settings _settings;
        private readonly IServiceFactory _serviceFactory;
        private readonly TelemetryClient _telemetryClient;

        public UpdateController(ILog logger, Settings settings, IServiceFactory serviceFactory, TelemetryClient telemetryClient)
        {
            _logger = logger;
            _settings = settings;
            _serviceFactory = serviceFactory;
            _telemetryClient = telemetryClient;
        }

        [SwaggerOperation("Update")]
        public HttpResponseMessage Post([FromBody]EcTmpUpdateAppointmentRequest requestMessage)
        {
            try
            {
                if (requestMessage == null)
                {
                    _logger.Error("Error calling VVS Update. The request cannot be null");
                    var error = new EcTmpUpdateAppointmentResponse { ExceptionOccured = true, ExceptionMessage = "Error calling VVS Update. The request cannot be null" };
                    return Request.CreateResponse(HttpStatusCode.Created, error);
                }

                var response = new EcTmpUpdateAppointmentMessageHandler(_logger, _settings, _serviceFactory).HandleRequestResponse(requestMessage);

                return Request.CreateResponse(HttpStatusCode.Created, response);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.Created, CreateResponse(ex));
            }
        }

        private EcTmpUpdateAppointmentResponse CreateResponse<T>(T ex) where T : Exception
        {
            _logger.Error($"Error VVS Update: {ex.Message}", ex);
            _telemetryClient.TrackException(ex, new Dictionary<string, string>() { { "Method", "VVS Update" } });
            return new EcTmpUpdateAppointmentResponse { ExceptionOccured = true, ExceptionMessage = $"ERROR VVS Update: {Strings.BuildErrorMessage(ex)}" };
        }
    }
}
