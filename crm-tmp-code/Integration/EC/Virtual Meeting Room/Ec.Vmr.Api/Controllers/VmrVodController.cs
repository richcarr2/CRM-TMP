using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Ec.VirtualMeetingRoom.MessageHandler;
using Ec.VirtualMeetingRoom.Messages;
using Ec.VirtualMeetingRoom.Services;
using log4net;
using Microsoft.ApplicationInsights;
using Swashbuckle.Swagger.Annotations;
using VA.TMP.Integration.Certificate.Interface;
using VA.TMP.Integration.Common;
using VA.TMP.Integration.Core;

namespace Ec.Vmr.Api.Controllers
{
    public class VmrVodController : ApiController
    {
        private readonly ILog _logger;
        private readonly Settings _settings;
        private readonly IKeyVaultCert _keyVaultCert;
        private readonly IServiceFactory _serviceFactory;
        private readonly TelemetryClient _telemetryClient;

        public VmrVodController(ILog logger, Settings settings, IKeyVaultCert keyVaultCert, IServiceFactory serviceFactory, TelemetryClient telemetryClient)
        {
            _logger = logger;
            _settings = settings;
            _keyVaultCert = keyVaultCert;
            _serviceFactory = serviceFactory;
            _telemetryClient = telemetryClient;
        }

        [SwaggerOperation("VOD")]
        public HttpResponseMessage Post([FromBody]EcVyoptaSMScheduleMeetingRequest requestMessage)
        {
            try
            {
                if (requestMessage == null)
                {
                    _logger.Error("Error calling VMR VOD Meeting. The request cannot be null");
                    var error = new EcVyoptaSMScheduleMeetingResponse { ExceptionOccured = true, ExceptionMessage = "Error calling VMR VOD Meeting. The request cannot be null" };
                    return Request.CreateResponse(HttpStatusCode.Created, error);
                }

                var response = new ScheduleMeetingMessageHandler(_logger, _settings, _keyVaultCert, _serviceFactory).HandleRequestResponse(requestMessage);

                return Request.CreateResponse(HttpStatusCode.Created, response);
            }
            catch (Exception ex)
            {
                _logger.Error($"Error VMR VOD Meeting: {ex.Message}", ex);
                _telemetryClient.TrackException(ex, new Dictionary<string, string>() { { "Method", "VMR VOD" } });
                var response = new EcVyoptaSMScheduleMeetingResponse { ExceptionOccured = true, ExceptionMessage = $"ERROR VMR VOD Meeting: {Strings.BuildErrorMessage(ex)}" };
                return Request.CreateResponse(HttpStatusCode.Created, response);
            }
        }
    }
}
