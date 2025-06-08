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
using VA.TMP.Integration.Messages.VirtualMeetingRoom;
using VA.TMP.Integration.VirtualMeetingRoom.MessageHandler;

namespace VA.TMP.Integration.Api.VirtualMedicalRoom.Controllers
{
    public class VmrDeleteController : ApiController
    {
        private readonly ILog _logger;
        private readonly Settings _settings;
        private readonly TelemetryClient _telemetryClient;

        public VmrDeleteController(ILog logger, Settings settings, TelemetryClient telemetryClient)
        {
            _logger = logger;
            _settings = settings;
            _telemetryClient = telemetryClient;
        }

        [SwaggerOperation("Delete")]
        public HttpResponseMessage Post([FromBody]VirtualMeetingRoomDeleteRequestMessage requestMessage)
        {
            var serializedRequestMessage = string.Empty;

            try
            {
                if (requestMessage == null)
                {
                    _logger.Error("Error calling VMR Delete Meeting. The request cannot be null");
                    var error = new VirtualMeetingRoomDeleteResponseMessage { ExceptionOccured = true, ExceptionMessage = "Error calling VMR Delete Meeting. The request cannot be null" };
                    return Request.CreateResponse(HttpStatusCode.Created, error);
                }

                serializedRequestMessage = Serialization.DataContractSerialize(requestMessage);

                var response = new VirtualMeetingRoomDeleteHandler(_logger, _settings).HandleRequestResponse(requestMessage);

                return Request.CreateResponse(HttpStatusCode.Created, response);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.Created, CreateResponse(ex, serializedRequestMessage));
            }
        }

        private VirtualMeetingRoomCreateResponseMessage CreateResponse<T>(T ex, string serializedRequestMessage) where T : Exception
        {
            _logger.Error($"Error VMR Delete Meeting: {ex.Message}", ex);
            _telemetryClient.TrackException(ex, new Dictionary<string, string>() { { "Method", "VMR Delete Meeting" } });
            return new VirtualMeetingRoomCreateResponseMessage
            {
                ExceptionOccured = true,
                ExceptionMessage = $"Error calling VMR Delete Meeting: {Strings.BuildErrorMessage(ex)}",
                SerializedInstance = serializedRequestMessage
            };
        }
    }
}
