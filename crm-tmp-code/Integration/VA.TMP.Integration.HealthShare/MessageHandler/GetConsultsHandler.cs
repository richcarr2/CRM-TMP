using log4net;
using VA.TMP.Integration.Common;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.HealthShare.Processor;
using VA.TMP.Integration.Messages.HealthShare;

namespace VA.TMP.Integration.HealthShare.MessageHandler
{
    /// <summary>
    /// HealthShare Get Consults handler.
    /// </summary>
    public class GetConsultsHandler
    {
        private readonly ILog _logger;
        private readonly Settings _settings;
        private string _serializedRequest;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public GetConsultsHandler(ILog logger, Settings settings)
        {
            _logger = logger;
            _settings = settings;
        }

        /// <summary>
        /// Handle the Virtual Meeting Request and send back a response.
        /// </summary>
        /// <param name="message">VirtualMeetingRoomCreateRequestMessage.</param>
        /// <returns>VirtualMeetingRoomCreateResponseMessage.</returns>
        public TmpHealthShareGetConsultsResponse HandleRequestResponse(TmpHealthShareGetConsultsRequest message)
        {
            _serializedRequest = Serialization.DataContractSerialize(message);

            var requestProcessor = new GetConsultsProcessor(_logger, _settings);
            var response = requestProcessor.Execute(message);

            response.VimtRequest = _serializedRequest;
            response.VimtResponse = Serialization.DataContractSerialize(response);

            return response;
        }
    }
}