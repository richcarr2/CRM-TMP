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
using VA.TMP.Integration.Messages.Mvi;
using VA.TMP.Integration.Mvi.MessageHandler;

namespace VA.TMP.Integration.Api.Mvi.Controllers
{
    public class PersonSearchController : ApiController
    {
        private readonly ILog _logger;
        private readonly Settings _settings;
        private readonly TelemetryClient _telemetryClient;

        public PersonSearchController(ILog logger, Settings settings, TelemetryClient telemetryClient)
        {
            _logger = logger;
            _settings = settings;
            _telemetryClient = telemetryClient;
        }

        [SwaggerOperation("PersonSearch")]
        public HttpResponseMessage Post([FromBody]PersonSearchRequestMessage requestMessage)
        {
            var serializedRequestMessage = string.Empty;

            try
            {
                if (requestMessage == null)
                {
                    _logger.Error("Error calling MVI Person Search. The request cannot be null");
                    var error = new PersonSearchResponseMessage { ExceptionOccured = true, ExceptionMessage = "Error calling MVI Person Search. The request cannot be null" };
                    return Request.CreateResponse(HttpStatusCode.Created, error);
                }

                serializedRequestMessage = Serialization.DataContractSerialize(requestMessage);

                var response = new PersonSearchHandler(_logger, _settings).HandleRequestResponse(requestMessage);

                return Request.CreateResponse(HttpStatusCode.Created, response);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.Created, CreateResponse(ex, serializedRequestMessage));
            }
        }

        private PersonSearchResponseMessage CreateResponse<T>(T ex, string serializedRequestMessage) where T : Exception
        {
            _logger.Error($"Error MVI Person Search: {ex.Message}", ex);
            _telemetryClient.TrackException(ex, new Dictionary<string, string>() { { "Method", "MVI Person Search" } });
            return new PersonSearchResponseMessage
            {
                ExceptionOccured = true,
                ExceptionMessage = $"Error MVI Person Search: {Strings.BuildErrorMessage(ex)}",
                SerializedInstance = serializedRequestMessage
            };
        }
    }
}
