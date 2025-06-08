using System.Diagnostics;
using log4net;
using VA.TMP.Integration.Common;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Messages.VirtualMeetingRoom;
using VA.TMP.Integration.VirtualMeetingRoom.Processor;

namespace VA.TMP.Integration.VirtualMeetingRoom.MessageHandler
{
    /// <summary>
    /// Virtual Meeting Room handler.
    /// </summary>
    public class VirtualMeetingRoomDeleteHandler
    {
        private readonly ILog _logger;
        private readonly Settings _settings;
        private string _serializedRequest;
        private Stopwatch _timer;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public VirtualMeetingRoomDeleteHandler(ILog logger, Settings settings)
        {
            _logger = logger;
            _settings = settings;
        }

        /// <summary>
        /// Handle the Virtual Meeting Request and send back a response.
        /// </summary>
        /// <param name="message">VirtualMeetingRoomDeleteRequestMessage.</param>
        /// <returns>VirtualMeetingRoomDeleteResponseMessage.</returns>
        public VirtualMeetingRoomDeleteResponseMessage HandleRequestResponse(VirtualMeetingRoomDeleteRequestMessage message)
        {
            _timer = new Stopwatch();
            _timer.Start();
            _serializedRequest = Serialization.DataContractSerialize(message);

            var requestProcessor = new VirtualMeetingRoomDeleteProcessor(_logger, _settings);
            var response = requestProcessor.Execute(message);

            if (response.ExceptionOccured) _logger.Error($"ERROR: Virtual Meeting Room Delete: {response.ExceptionMessage}");

            response.VimtRequest = _serializedRequest;
            response.VimtResponse = Serialization.DataContractSerialize(response);
            _timer.Stop();
            response.VimtProcessingMs = (int)_timer.ElapsedMilliseconds;

            return response;
        }
    }
}
