using System.Diagnostics;
using log4net;
using VA.TMP.Integration.Common;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Messages.VideoVisit;
using VA.TMP.Integration.VideoVisit.Processor;

namespace VA.TMP.Integration.VideoVisit.MessageHandler
{
    /// <summary>
    /// Video Visit handler.
    /// </summary>
    public class VideoVisitCreateHandler
    {
        private readonly ILog _logger;
        private readonly Settings _settings;
        private string _serializedRequest;
        private Stopwatch _timer;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public VideoVisitCreateHandler(ILog logger, Settings settings)
        {
            _logger = logger;
            _settings = settings;
        }

        /// <summary>
        /// Handle the Video Visit Request and send back a response.
        /// </summary>
        /// <param name="message">VideoVisitCreateRequestMessage.</param>
        /// <returns>VideoVisitCreateResponseMessage.</returns>
        public VideoVisitCreateResponseMessage HandleRequestResponse(VideoVisitCreateRequestMessage message)
        {
            _timer = new Stopwatch();
            _timer.Start();

            _serializedRequest = Serialization.DataContractSerialize(message);

            var requestProcessor = new VideoVisitCreateProcessor(_logger, _settings);
            var response = requestProcessor.Execute(message);

            if (response.ExceptionOccured) _logger.Error(string.Format("ERROR: Video Visit Create: {0}", response.ExceptionMessage));

            response.VimtRequest = _serializedRequest;
            response.VimtResponse = Serialization.DataContractSerialize(response);
            _timer.Stop();
            response.VimtProcessingMs = (int)_timer.ElapsedMilliseconds;
            return response;
        }
    }
}
