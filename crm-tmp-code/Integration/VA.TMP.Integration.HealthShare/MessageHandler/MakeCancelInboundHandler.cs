using System.Diagnostics;
using log4net;
using VA.TMP.Integration.Common;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.HealthShare.Processor;
using VA.TMP.Integration.Messages.HealthShare;

namespace VA.TMP.Integration.HealthShare.MessageHandler
{
    /// <summary>
    /// HealthShare Make Cancel Inbound Handler.
    /// </summary>
    public class MakeCancelInboundHandler
    {
        private readonly ILog _logger;
        private readonly Settings _settings;
        private string _serializedRequest;
        private Stopwatch _timer;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public MakeCancelInboundHandler(ILog logger, Settings settings)
        {
            _logger = logger;
            _settings = settings;
        }

        /// <summary>
        /// Handle the Make Cancel Inbound Request.
        /// </summary>
        /// <param name="message">TmpHealthShareMakeCancelInboundRequestMessage.</param>
        /// <returns>TmpHealthShareMakeCancelInboundResponseMessage.</returns>
        public TmpHealthShareMakeCancelInboundResponseMessage HandleRequestResponse(TmpHealthShareMakeCancelInboundRequestMessage message)
        {
            _timer = new Stopwatch();

            _timer.Start();
            _serializedRequest = Serialization.DataContractSerialize(message);

            var requestProcessor = new MakeCancelInboundProcessor(_logger, _settings);
            var response = requestProcessor.Execute(message);

            if (response.ExceptionOccured) _logger.Error($"ERROR: HealthShare Make Cancel Inbound: {response.ExceptionMessage}");

            response.VimtRequest = _serializedRequest;
            response.VimtResponse = Serialization.DataContractSerialize(response);
            _timer.Stop();
            response.VimtProcessingMs = (int)_timer.ElapsedMilliseconds;

            return response;
        }
    }
}
