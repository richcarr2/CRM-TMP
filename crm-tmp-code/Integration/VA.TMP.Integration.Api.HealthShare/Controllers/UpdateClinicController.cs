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
    public class UpdateClinicController : ApiController
    {
        private readonly ILog _logger;
        private readonly Settings _settings;
        private readonly TelemetryClient _telemetryClient;

        public UpdateClinicController(ILog logger, Settings settings, TelemetryClient telemetryClient)
        {
            _logger = logger;
            _settings = settings;
            _telemetryClient = telemetryClient;
        }

        [SwaggerOperation("UpdateClinic")]
        public HttpResponseMessage Post([FromBody]TmpHealthShareUpdateClinicRequestMessage requestMessage)
        {
            var serializedRequestMessage = string.Empty;

            try
            {
                if (requestMessage == null)
                {
                    _logger.Error("Error calling HealthShare Update Clinic. The request cannot be null");
                    var error = new TmpHealthShareUpdateClinicResponseMessage { ExceptionOccured = true, ExceptionMessage = "Error calling HealthShare Update Clinic. The request cannot be null" };
                    return Request.CreateResponse(HttpStatusCode.Created, error);
                }

                serializedRequestMessage = Serialization.DataContractSerialize(requestMessage);

                var response = new UpdateClinicHandler(_logger, _settings).HandleRequestResponse(requestMessage);

                return Request.CreateResponse(HttpStatusCode.Created, response);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.Created, CreateResponse(ex, serializedRequestMessage));
            }
        }

        private TmpHealthShareUpdateClinicResponseMessage CreateResponse<T>(T ex, string serializedRequestMessage) where T : Exception
        {
            _logger.Error($"Error HealthShare Clinic Update: {ex.Message}", ex);
            _telemetryClient.TrackException(ex, new Dictionary<string, string>() { { "Method", "HealthShare Clinic Update" } });
            return new TmpHealthShareUpdateClinicResponseMessage
            {
                ExceptionOccured = true,
                ExceptionMessage = $"Error calling HealthShare Clinic Update: {Strings.BuildErrorMessage(ex)}",
                SerializedInstance = serializedRequestMessage
            };
        }
    }
}
