using System.Diagnostics;
using log4net;
using VA.TMP.Integration.Common;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.HealthShare.Processor;
using VA.TMP.Integration.Messages.HealthShare;

namespace VA.TMP.Integration.HealthShare.MessageHandler
{
    /// <summary>
    /// HealthShare Make Cancel Outbound Handler.
    /// </summary>
    public class MakeCancelOutboundHandler
    {
        private readonly ILog _logger;
        private readonly Settings _settings;
        private string _serializedRequest;
        private Stopwatch _timer;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public MakeCancelOutboundHandler(ILog logger, Settings settings)
        {
            _logger = logger;
            _settings = settings;
        }

        /// <summary>
        /// Handle the Make Cancel Outbound Request.
        /// </summary>
        /// <param name="message">TmpHealthShareMakeCancelOutboundRequestMessage.</param>
        /// <returns>TmpHealthShareMakeCancelOutboundResponseMessage.</returns>
        public TmpHealthShareMakeCancelOutboundResponseMessage HandleRequestResponse(TmpHealthShareMakeCancelOutboundRequestMessage message)
        {
            _timer = new Stopwatch();

            _timer.Start();
            _serializedRequest = Serialization.DataContractSerialize(message);

            var requestProcessor = new MakeCancelOutboundProcessor(_logger, _settings);
            var response = requestProcessor.Execute(message);

            _timer.Stop();
            response.VimtProcessingMs = (int)_timer.ElapsedMilliseconds;

            return response;
        }
    }
}
