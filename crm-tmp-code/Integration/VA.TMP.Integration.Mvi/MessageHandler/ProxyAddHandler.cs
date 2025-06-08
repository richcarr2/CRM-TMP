using System.Diagnostics;
using log4net;
using VA.TMP.Integration.Common;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Messages.Mvi;
using VA.TMP.Integration.Mvi.Processor;

namespace VA.TMP.Integration.Mvi.MessageHandler
{
    /// <summary>
    /// Proxy Add handler.
    /// </summary>
    public class ProxyAddHandler
    {
        private readonly ILog _logger;
        private readonly Settings _settings;
        private string _serializedRequest;
        private Stopwatch _timer;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public ProxyAddHandler(ILog logger, Settings settings)
        {
            _logger = logger;
            _settings = settings;
        }

        /// <summary>
        /// Handle the Person Search Request and send back a response.
        /// </summary>
        /// <param name="message">PersonSearchRequestMessage.</param>
        /// <returns>ProxyAddResponseMessage.</returns>
        public ProxyAddResponseMessage HandleRequestResponse(ProxyAddRequestMessage message)
        {
            _timer = new Stopwatch();
            _timer.Start();

            _serializedRequest = Serialization.DataContractSerialize(message);

            var requestProcessor = new ProxyAddProcessor(_logger, _settings);
            var response = requestProcessor.Execute(message);

            if (response.ExceptionOccured) _logger.Error($"ERROR: MVI Proxy Add: {response.ExceptionMessage}");

            response.VimtRequest = _serializedRequest;
            response.VimtResponse = Serialization.DataContractSerialize(response);
            _timer.Stop();
            response.VimtProcessingMs = (int)_timer.ElapsedMilliseconds;

            return response;
        }
    }
}
