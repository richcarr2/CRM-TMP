using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Ec.HealthShare.MessageHandler;
using Ec.HealthShare.Messages;
using Ec.HealthShare.Services.Rest;
using log4net;
using Microsoft.ApplicationInsights;
using Swashbuckle.Swagger.Annotations;
using VA.TMP.Integration.Common;
using VA.TMP.Integration.Core;

namespace Ec.HealthShare.Api.Controllers
{
    public class ConsultsController : ApiController
    {
        private readonly ILog _logger;
        private readonly Settings _settings;
        private readonly IServiceFactory _serviceFactory;
        private readonly TelemetryClient _telemetryClient;

        public ConsultsController(ILog logger, Settings settings, IServiceFactory serviceFactory, TelemetryClient telemetryClient)
        {
            _logger = logger;
            _settings = settings;
            _serviceFactory = serviceFactory;
            _telemetryClient = telemetryClient;
        }

        [SwaggerOperation("GetConsults")]
        public HttpResponseMessage Post([FromBody]EcHealthShareGetConsultsRequest requestMessage)
        {
            try
            {
                if (requestMessage == null)
                {
                    _logger.Error("Error calling HealthShare Get Consults. The request cannot be null");
                    var error = new EcHealthShareGetConsultsResponse { ExceptionOccured = true, ExceptionMessage = "Error calling HealthShare Get Consults. The request cannot be null" };
                    return Request.CreateResponse(HttpStatusCode.Created, error);
                }

                var response = new GetConsultsMessageHandler(_logger, _settings, _serviceFactory).HandleRequestResponse(requestMessage);

                return Request.CreateResponse(HttpStatusCode.Created, response);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.Created, CreateResponse(ex));
            }
        }

        private EcHealthShareGetConsultsResponse CreateResponse<T>(T ex) where T : Exception
        {
            _logger.Error($"Error HealthShare Get Consults: {ex.Message}", ex);
            _telemetryClient.TrackException(ex, new Dictionary<string, string>() { { "Method", "HealthShare GetConsults" } });
            return new EcHealthShareGetConsultsResponse { ExceptionOccured = true, ExceptionMessage = $"ERROR HealthShare Get Consults: {Strings.BuildErrorMessage(ex)}" };
        }
    }
}
