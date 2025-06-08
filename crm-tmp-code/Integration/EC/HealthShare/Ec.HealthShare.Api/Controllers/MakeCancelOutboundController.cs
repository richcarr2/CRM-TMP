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
    public class MakeCancelOutboundController : ApiController
    {
        private readonly ILog _logger;
        private readonly Settings _settings;
        private readonly IServiceFactory _serviceFactory;
        private readonly TelemetryClient _telemetryClient;

        public MakeCancelOutboundController(ILog logger, Settings settings, IServiceFactory serviceFactory, TelemetryClient telemetryClient)
        {
            _logger = logger;
            _settings = settings;
            _serviceFactory = serviceFactory;
            _telemetryClient = telemetryClient;
        }

        [SwaggerOperation("MakeCancel")]
        public HttpResponseMessage Post([FromBody]EcHealthShareMakeCancelOutboundRequestMessage requestMessage)
        {
            try
            {
                if (requestMessage == null)
                {
                    _logger.Error("Error calling HealthShare Make Cancel Outbound. The request cannot be null");
                    var error = new EcHealthShareMakeCancelOutboundResponseMessage { ExceptionOccured = true, ExceptionMessage = "Error calling HealthShare Make Cancel Outbound. The request cannot be null" };
                    return Request.CreateResponse(HttpStatusCode.Created, error);
                }

                var response = new MakeCancelOutboundMessageHandler(_logger, _settings, _serviceFactory).HandleRequestResponse(requestMessage);

                return Request.CreateResponse(HttpStatusCode.Created, response);
            }
            catch (Exception ex)
            {                
                return Request.CreateResponse(HttpStatusCode.Created, CreateResponse(ex));
            }
        }

        private EcHealthShareMakeCancelOutboundResponseMessage CreateResponse<T>(T ex) where T : Exception
        {
            _logger.Error($"Error HealthShare MakeCancel Outbound: {ex.Message}", ex);
            _telemetryClient.TrackException(ex, new Dictionary<string, string>() { { "Method", "HealthShare MakeCancelOutbound" } });
            return new EcHealthShareMakeCancelOutboundResponseMessage { ExceptionOccured = true, ExceptionMessage = $"ERROR HealthShare MakeCancel Outbound: {Strings.BuildErrorMessage(ex)}" };
        }
    }
}
