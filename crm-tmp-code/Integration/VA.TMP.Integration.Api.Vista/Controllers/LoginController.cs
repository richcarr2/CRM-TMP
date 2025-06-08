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
using VA.TMP.Integration.Messages.Vista;
using VA.TMP.Integration.Vista.MessageHandler;

namespace VA.TMP.Integration.Api.Vista.Controllers
{
    public class LoginController : ApiController
    {
        private readonly ILog _logger;
        private readonly Settings _settings;
        private readonly TelemetryClient _telemetryClient;

        public LoginController(ILog logger, Settings settings, TelemetryClient telemetryClient)
        {
            _logger = logger;
            _settings = settings;
            _telemetryClient = telemetryClient;
        }

        [SwaggerOperation("Login")]
        public HttpResponseMessage Post([FromBody]ViaLoginRequestMessage requestMessage)
        {
            var serializedRequestMessage = string.Empty;

            try
            {
                if (requestMessage == null)
                {
                    _logger.Error("Error calling VIA Login. The request cannot be null");
                    var error = new ViaLoginResponseMessage { ExceptionOccured = true, ExceptionMessage = "Error calling VIA Login. The request cannot be null" };
                    return Request.CreateResponse(HttpStatusCode.Created, error);
                }

                serializedRequestMessage = Serialization.DataContractSerialize(requestMessage);

                var response = new ViaLoginHandler(_logger, _settings).HandleRequestResponse(requestMessage);

                return Request.CreateResponse(HttpStatusCode.Created, response);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.Created, CreateResponse(ex, serializedRequestMessage));
            }
        }

        private ViaLoginResponseMessage CreateResponse<T>(T ex, string serializedRequestMessage) where T : Exception
        {
            _logger.Error($"Error VIA Login: {ex.Message}", ex);
            _telemetryClient.TrackException(ex, new Dictionary<string, string>() { { "Method", "VIA Login" } });
            return new ViaLoginResponseMessage
            {
                ExceptionOccured = true,
                ExceptionMessage = $"Error calling VIA Login: {Strings.BuildErrorMessage(ex)}",
                SerializedInstance = serializedRequestMessage
            };
        }
    }
}
