using System.Diagnostics;
using log4net;
using VA.TMP.Integration.Common;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Messages.VirtualMeetingRoom;
using VA.TMP.Integration.VirtualMeetingRoom.Processor;

namespace VA.TMP.Integration.VirtualMeetingRoom.MessageHandler
{
    /// <summary>
    /// VMR On Demand handler.
    /// </summary>
    public class VmrOnDemandCreateHandler
    {
        private readonly ILog _logger;
        private readonly Settings _settings;
        private string _serializedRequest;
        private Stopwatch _timer;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public VmrOnDemandCreateHandler(ILog logger, Settings settings)
        {
            _logger = logger;
            _settings = settings;
        }

        /// <summary>
        /// Handle the VMR On Demand Request and send back a response.
        /// </summary>
        /// <param name="message">VmrOnDemandCreateRequestMessage.</param>
        /// <returns>VmrOnDemandCreateResponseMessage.</returns>
        public VmrOnDemandCreateResponseMessage HandleRequestResponse(VmrOnDemandCreateRequestMessage message)
        {
            _timer = new Stopwatch();
            _timer.Start();
            _serializedRequest = Serialization.DataContractSerialize(message);

            var requestProcessor = new VmrOnDemandCreateProcessor(_logger, _settings);
            var response = requestProcessor.Execute(message);

            if (response.ExceptionOccured) _logger.Error($"ERROR: Virtual Meeting Room Create: {response.ExceptionMessage}");

            response.VimtRequest = _serializedRequest;
            response.VimtResponse = Serialization.DataContractSerialize(response);
            _timer.Stop();
            response.VimtProcessingMs = (int)_timer.ElapsedMilliseconds;

            return response;
        }
    }
}
