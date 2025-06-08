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
    public class MakeCancelController : ApiController
    {
        private readonly ILog _logger;
        private readonly Settings _settings;
        private readonly TelemetryClient _telemetryClient;

        public MakeCancelController(ILog logger, Settings settings, TelemetryClient telemetryClient)
        {
            _logger = logger;
            _settings = settings;
            _telemetryClient = telemetryClient;
        }

        [SwaggerOperation("MakeCancel")]
        public HttpResponseMessage Post([FromBody]TmpHealthShareMakeAndCancelAppointmentRequestMessage requestMessage)
        {
            var serializedRequestMessage = string.Empty;

            try
            {
                if (requestMessage == null)
                {
                    _logger.Error("Error calling HealthShare Make And Cancel. The request cannot be null");
                    var error = new TmpHealthShareMakeAndCancelAppointmentResponseMessage { ExceptionOccured = true, ExceptionMessage = "Error calling HealthShare Make and Cancel. The request cannot be null" };
                    return Request.CreateResponse(HttpStatusCode.Created, error);
                }

                serializedRequestMessage = Serialization.DataContractSerialize(requestMessage);

                var response = new MakeCancelHandler(_logger, _settings).HandleRequestResponse(requestMessage);

                return Request.CreateResponse(HttpStatusCode.Created, response);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.Created, CreateResponse(ex, serializedRequestMessage));
            }
        }

        private TmpHealthShareMakeAndCancelAppointmentResponseMessage CreateResponse<T>(T ex, string serializedRequestMessage) where T : Exception
        {
            _logger.Error($"Error HealthShare Make And Cancel: {ex.Message}", ex);
            _telemetryClient.TrackException(ex, new Dictionary<string, string>() { { "Method", "HealthShare Make and Cancel" } });
            return new TmpHealthShareMakeAndCancelAppointmentResponseMessage
            {
                ExceptionOccured = true,
                ExceptionMessage = $"Error calling HealthShare Make and Cancel: {Strings.BuildErrorMessage(ex)}",
                SerializedInstance = serializedRequestMessage
            };
        }
    }
}
